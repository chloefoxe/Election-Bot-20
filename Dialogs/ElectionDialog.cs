/* This is the Election dialog which asks users whether they voted in the previous election. The bot parses the answer through LUIS and attempts to make an intelligent response.
 Following some chit-chat (conversation fillers) at the end of this dialog, the conversation flow moves to the issuesDialog. */


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

        public ElectionDialog(ConversationRecognizer luisRecognizer, ILogger<ElectionDialog> logger, ConstituencyDialog constituencyDialog
        )
            : base(nameof(ElectionDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(constituencyDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,     // Bot asks if the user voted in the Election
                AskVotedAsync,      // Stores result and comments on answer (Yes/No)
                FillerStepAsync,    // Conversation chit-chat filler
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /* Asks if the user voted in the previous election */
        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            
            if (personalDetails.Voted == null)
            {
                await Task.Delay(1500);     // Pause for effect
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

            /* Booolean level strings to hold the result from the LUIS model, so these can be referenced at the end of the dialog 
             with the same token for each user specfic to whether they voted or not. */
            string[] votedString, didNotVoteString;
            votedString = new string[]{ "did vote"};
            didNotVoteString = new string[]{ "did not vote"};

            // Retrives the intent and makes a response based on whether user voted or not
            switch (luisResult.TopIntent().intent)
            {
                case Luis.ElectionBot.Intent.didVote:
                    personalDetails.Voted = votedString;   // Store result in personal Details object

                    var votedText = "Good job 👍🏻 Everyone should use their vote don't you think?";
                    var votedPromptMessage = MessageFactory.Text(votedText, votedText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = votedPromptMessage }, cancellationToken);
                
                case Luis.ElectionBot.Intent.didNotVote:
                    personalDetails.Voted = didNotVoteString;   // Store result in personal Details object

                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Awh that's a pity. I couldn't vote either. 😐"), cancellationToken);
                    await Task.Delay(1500);
                    var didNotVoteText = "... apartently I'm not classed as a real citizen! - Isn't that strange?";
                    var promptMessage = MessageFactory.Text(didNotVoteText, didNotVoteText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(personalDetails, cancellationToken);
        }

        /* Continue to next dialog */
        private async Task<DialogTurnResult> FillerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            
            await Task.Delay(1500);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You know what? I totally agree with you."), cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(ConstituencyDialog), personalDetails, cancellationToken);
        }
    }
}
