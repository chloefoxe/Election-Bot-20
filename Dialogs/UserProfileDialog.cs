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
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[UserInfo] = new PersonalDetails();

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") };

            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> GetUserIDAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            
            var userProfile = (PersonalDetails)stepContext.Values[UserInfo];
            userProfile.Name = (String[])luisResult.Entities.name;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter your user ID.") };

            // Ask the user to enter their age.
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            
            var userProfile = (PersonalDetails)stepContext.Values[UserInfo];
            userProfile.UserID = (String[])luisResult.Entities.userID;

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Thanks for participating, {((PersonalDetails)stepContext.Values[UserInfo]).Name}."),
                cancellationToken);

            // Ask the user to enter their age.
            return await stepContext.EndDialogAsync(stepContext.Values[UserInfo], cancellationToken);
        }

        // private async Task<DialogTurnResult> GetNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        // {
        //     var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
        //     var userInfo = new PersonalDetails(){
        //         Name = luisResult.Entities.name,
        //     };
                
        //     var messageText = $"Next, could you input your User ID, please?";
            
        //     var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
        //     return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        // }

        // private async Task<DialogTurnResult> GetUserIDAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        // {
        //     var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
        //     var userInfo = new PersonalDetails(){
        //         UserID = luisResult.Entities.userID,
        //     };

        //     await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks for that."), cancellationToken);

        //     return await stepContext.BeginDialogAsync(nameof(ElectionDialog));
        // }
    }
}   