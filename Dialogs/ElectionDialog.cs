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

        public ElectionDialog(ConversationRecognizer luisRecognizer, ILogger<ElectionDialog> logger, ConstituencyDialog constituencyDialog)
            : base(nameof(ElectionDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(constituencyDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                AskVotedAsync,
                FillerStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            
            if (personalDetails.Voted == null)
            {
                await Task.Delay(1500);
                var messageText = "So, alot has happened since this year's general election then! Did you cast your vote in February?";
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
            votedString = new string[]{ "did vote"};
            didNotVoteString = new string[]{ "did not vote"};

            switch (luisResult.TopIntent().intent)
            {
                case Luis.ElectionBot.Intent.didVote:
                    personalDetails.Voted = votedString;

                    var votedText = "Good job 👍🏻 Everyone should use their vote don't you think?";
                    var votedPromptMessage = MessageFactory.Text(votedText, votedText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = votedPromptMessage }, cancellationToken);
                
                case Luis.ElectionBot.Intent.didNotVote:
                    personalDetails.Voted = didNotVoteString;

                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Awh that's a pity. I couldn't vote either. 😐"), cancellationToken);
                    await Task.Delay(1500);
                    var didNotVoteText = "... apartently I'm not classed as a real citizen! - Isn't that strange?";
                    var promptMessage = MessageFactory.Text(didNotVoteText, didNotVoteText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);

                //case Luis.ElectionBot.Intent.didNotVote:
                //     // Catch all for unhandled intents
                //     var didntUnderstandMessageText = $"That's interesting!";
                //     var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                //     return await stepContext.NextAsync(personalDetails, cancellationToken);
            }

            return await stepContext.NextAsync(personalDetails, cancellationToken);
        }

        private async Task<DialogTurnResult> FillerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            
            await Task.Delay(1500);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You know what? I totally agree with you."), cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(ConstituencyDialog), personalDetails, cancellationToken);
        }
    }
}
