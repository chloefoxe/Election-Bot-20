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

        private const string UserInfo = "value-userInfo";

       public UserProfileDialog(ConversationRecognizer luisRecognizer, ElectionDialog electionDialog, ILogger<UserProfileDialog> logger)
            : base(nameof(UserProfileDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;
            
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(electionDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GetNameAsync,
                GetUserIDAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        static string UppercaseFirst(string s)
        {
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        private async Task<DialogTurnResult> GetNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetials = (PersonalDetails)stepContext.Options;
            
            if (personalDetials.Name == null)
            {
                var messageText = "Can I ask what is your name?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            return await stepContext.NextAsync(personalDetials.Name, cancellationToken);
        }

        private async Task<DialogTurnResult> GetUserIDAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);

            personalDetails.Name = luisResult.Entities.name;

            string uncapped, capped;

            uncapped = personalDetails.Name.First();

            capped = UppercaseFirst(uncapped);

            string [] capitalisedName = new string [1];

            capitalisedName[0] = capped;

            personalDetails.Name = capitalisedName;

            if (personalDetails.UserID== null)
            {                                                                                                                                                                    
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Nice to meet you {personalDetails.Name.First()}"), cancellationToken);
                await Task.Delay(1500);
                var messageText = "Next, could you input your User ID, please?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(personalDetails.UserID, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            personalDetails.UserID = luisResult.Entities.userID;

            personalDetails = (PersonalDetails)stepContext.Options;

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks for that {personalDetails.Name.First()}"), cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(ElectionDialog), personalDetails, cancellationToken);
        }
    }
}   