/* The party dialog asks for user preference on political party*/

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
    public class PartyDialog : ComponentDialog
    {
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        public PartyDialog(ConversationRecognizer luisRecognizer, EndConversationDialog endConversationDialog, ILogger<PartyDialog> logger)
            : base(nameof(PartyDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(endConversationDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskParty,               // Ask for user input
                ElaboratePartyAsync,    // Make a remark on input
                ContinueStepAsync,      // Continue to next dialog
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /* ask for user input on party preference*/
        private async Task<DialogTurnResult> AskParty(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            
            if (personalDetails.Party == null)
            {
                await Task.Delay(1500);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Continuing on with our imaginary scenario..."), cancellationToken);
                await Task.Delay(2500);
                var messageText = $"Personally, if choosing parties I'd be between Renua and Labour! What about you? Would you join a party or go as an independent?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            return await stepContext.NextAsync(personalDetails, cancellationToken);
        }

        /* Check user utterance for party and make remark on input*/
        private async Task<DialogTurnResult> ElaboratePartyAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);

            string[] def;
            def = new string[]{ "not disclosed"};

            await Task.Delay(2000);

            switch (luisResult.TopIntent().intent)
            {
                case Luis.ElectionBot.Intent.discussParty:
                    /*If LUIS classes user input as a party that is not contained in the model*/
                    if(luisResult.Entities.party_name == null){
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I think that's a good shout!"), cancellationToken);
                        await Task.Delay(1500);
                        var partyText = "But sure hey! It's all politics right?";
                        var housingPromptMessage = MessageFactory.Text(partyText, partyText, InputHints.ExpectingInput);
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = housingPromptMessage }, cancellationToken);
                    }
                    /* Otherwise just return the name of the party disclosed*/
                    else{
                        personalDetails.Party = luisResult.Entities.party_name;
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"The {personalDetails.Party.First()} party?!"), cancellationToken);
                        await Task.Delay(1500);
                        var partyText = "hmmm not sure if I agree with you on that! But sure it's all politics right?";
                        var housingPromptMessage = MessageFactory.Text(partyText, partyText, InputHints.ExpectingInput);
                        return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = housingPromptMessage }, cancellationToken);
                    }
                /* if the user doesn't disclose a party*/
                default:
                    personalDetails.Party = def;
                    var didntUnderstandMessageText = $"hmmm not sure if I agree with you on that! But sure it's all politics right?";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = didntUnderstandMessage }, cancellationToken);
            }
        }
        /* End conversation and continue to last dialog*/
        private async Task<DialogTurnResult> ContinueStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            
            await Task.Delay(2000);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"So maybe let's finish up before we go too far and get into an argument on political views?"), cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), personalDetails, cancellationToken);
        }
    }
}
