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
                //GetUserIDAsync
                GetNameAsync,
                //FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // private async Task<DialogTurnResult> GetUserIDAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        // {
        //     var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
        //     var userInfo = new PersonalDetails(){
        //         UserID = luisResult.Entities.userID,
        //     };
                
        //     if(luisResult.TopIntent().Equals(Luis.ElectionBot.Intent.None)){
        //         var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try rephrasing your message(intent was {luisResult.TopIntent().intent})";
        //         var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
        //     }

        //     return await stepContext.NextAsync(UserProfileDialog.UserID, cancellationToken);
        // }

        private async Task<DialogTurnResult> GetNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            var userInfo = new PersonalDetails(){
                Name = luisResult.Entities.name,
            };
                
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {userInfo.Name.FirstOrDefault()}, it's great to meet you!"), cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(ElectionDialog));
        }
    }
}   