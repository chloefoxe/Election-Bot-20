using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class UserProfileDialog : ComponentDialog
    {
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

       public UserProfileDialog(ConversationRecognizer luisRecognizer, ElectionDialog electionDialog, /*EndConversationDialog endConversationDialog*/ ILogger<UserProfileDialog> logger)
            : base(nameof(UserProfileDialog))
        {
            // _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(electionDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                
                GetNameAsync,
                GetUserIDAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            var userInfo = new PersonalDetails(){
                Name = luisResult.Entities.name,
            };
                
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Next, could you input your User ID, please?"), cancellationToken);
            var messageText = stepContext.Options?.ToString() ?? $"Next, could you input your User ID, please?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);

            //return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> GetUserIDAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            var userInfo = new PersonalDetails(){
                UserID = luisResult.Entities.userID,
            };

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks for that."), cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(ElectionDialog));
        }
    }
}   