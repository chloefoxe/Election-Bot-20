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
    public class ElectionDialog : ComponentDialog
    {
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        public ElectionDialog(ConversationRecognizer luisRecognizer, ILogger<ElectionDialog> logger)
            : base(nameof(ElectionDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                AskVotedAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            
            if (personalDetails.Voted == null)
            {
                var messageText = "So, alot has happened since this year's general election then! Did you vote in February last?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            return await stepContext.NextAsync(personalDetails, cancellationToken);

        }

        private async Task<DialogTurnResult> AskVotedAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);

            string[] votedString, didNotVoteString;
            votedString = new string[]{ "Did not Vote"};
            didNotVoteString = new string[]{ "Did Vote"};

            switch (luisResult.TopIntent().intent)
            {
                case Luis.ElectionBot.Intent.didVote:
                    //personalDetails.Voted = votedString;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"So you did vote then"), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                
                case Luis.ElectionBot.Intent.didNotVote:
                    //personalDetails.Voted = didNotVoteString;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"So you didn't vote then"), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
