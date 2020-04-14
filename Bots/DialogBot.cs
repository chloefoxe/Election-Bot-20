// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Azure;
using System.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler
        where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

         private static readonly CosmosDbPartitionedStorage query = new CosmosDbPartitionedStorage(new CosmosDbPartitionedStorageOptions
        {
            CosmosDbEndpoint = "https://electioncosmos.documents.azure.com:443/",
            AuthKey = "0zqSGejOcM7dqU603OFedsrfXf3HG6DBrwO0YZm85h2IlZrdyDY7la7tgfX0axd9ccNN4myrphorQMlxOuuBSw==",
            DatabaseId = "BotStoage",
            ContainerId = "Group2",
        });

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // preserve user input.
            var utterance = turnContext.Activity.Text;
            // make empty local logitems list.
            UtteranceLog logItems = null;

            // see if there are previous messages saved in storage.
            try
            {
                string[] utteranceList = { "UtteranceLog" };
                logItems = query.ReadAsync<UtteranceLog>(utteranceList).Result?.FirstOrDefault().Value;
            }
            catch
            {
                // Inform the user an error occured.
                //await turnContext.SendActivityAsync("Sorry, something went wrong reading your stored messages!");
            }

            // If no stored messages were found, create and store a new entry.
            if (logItems is null)
            {
                // add the current utterance to a new object.
                logItems = new UtteranceLog();
                logItems.UtteranceList.Add(utterance);
                // set initial turn counter to 1.
                logItems.TurnNumber++;

                // Create Dictionary object to hold received user messages.
                var changes = new Dictionary<string, object>();
                {
                    changes.Add("UtteranceLog", logItems);
                }
                try
                {
                    // Save the user message to your Storage.
                    await query.WriteAsync(changes, cancellationToken);
                }
                catch
                {
                    // Inform the user an error occured.
                    //await turnContext.SendActivityAsync("Sorry, something went wrong storing your message!");
                }
            }
            // Else, our Storage already contained saved user messages, add new one to the list.
            else
            {
                // add new message to list of messages to display.
                logItems.UtteranceList.Add(utterance);
                // increment turn counter.
                logItems.TurnNumber++;

                // Create Dictionary object to hold new list of messages.
                var changes = new Dictionary<string, object>();
                {
                    changes.Add("UtteranceLog", logItems);
                };

                try
                {
                    // Save new list to your Storage.
                    await query.WriteAsync(changes, cancellationToken);
                }
                catch
                {
                    // Inform the user an error occured.
                    //await turnContext.SendActivityAsync("Sorry, something went wrong storing your message!");
                }
            }
            
            Logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
    }
}
