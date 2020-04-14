

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
    public class ConstituencyDialog : ComponentDialog
    {
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        public ConstituencyDialog(ConversationRecognizer luisRecognizer, IssuesDialog issuesDialog, ILogger<ConstituencyDialog> logger)
            : base(nameof(ConstituencyDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(issuesDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskConstituency,
                RemarkOnLocationAsync,
                AgreeStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskConstituency(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            
            if (personalDetails.Location == null)
            {
                await Task.Delay(1500);
                var messageText = "My voting area is in Kerry, the kingdom 😉! Where is yours?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            return await stepContext.NextAsync(personalDetails.Location, cancellationToken);
        }

        private async Task<DialogTurnResult> RemarkOnLocationAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            personalDetails.Location = luisResult.Entities.location;

            if(luisResult.Entities.location == null) {
                var messageText = $"I see, I see. Surprising result in general, wasn't it?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            else {
                if(personalDetails.Location.First() == "wexford") {
                    var messageText = $"The Sunny South East 😎! A big win for Johnny Mythen down there, a surprising result don't you think?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (personalDetails.Location.First() == "dublin" || personalDetails.Location.First() == "dublin south west" || personalDetails.Location.First() == "dublin central" || personalDetails.Location.First() == "dublin south - west") {
                    var messageText = $"Interesting. Dublin's poll was dominated by Sinn Féin with 24% of the preference. Surprising result don't you think?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (personalDetails.Location.First() == "dun-laoghaire" || personalDetails.Location.First() == "dun laoighre" || personalDetails.Location.First() == "dun-laoighre" || personalDetails.Location.First() == "dún-laoghaire" || personalDetails.Location.First() == "dún laoighre") {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Wow! I love a good Scrumdiddly's"), cancellationToken);
                    await Task.Delay(1000);
                    var messageText = $"Fine Gael with a third of first preference votes up there! Surprising result don't you think?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (personalDetails.Location.First() == "carlow" || personalDetails.Location.First() == "kilkenny") {
                    var messageText = $"Interesting. A big win for Kathleen Funchion in the Carlow-Kilkenny constituency. An unsurprising result don't you think?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (personalDetails.Location.First() == "donegal") {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Donegal haigh! I was up there in the Gaeltacht before!"), cancellationToken);
                    await Task.Delay(1000);
                    var messageText = $"Anyways, a big win for Sinn Féin's Pearse Doherty in the Donegal area. An unsurprising result don't you think?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (personalDetails.Location.First() == "galway") {
                    var messageText = $"Very good. A big result for the independent Seán Canney in Galway. A surprising result don't you think?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (personalDetails.Location.First() == "leitrim") {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Leitrim haigh! The county with the shortest coastline in Ireland!"), cancellationToken);
                    var messageText = $"Kenny Martin elected with 15,000 votes on count one! An unsurprising result don't you think!";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (personalDetails.Location.First() == "kildare") {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"ah Kildare yes, I was there on my holidays before!"), cancellationToken);
                    await Task.Delay(1000);
                    var messageText = $"Sinn Féin with 22% of number ones! A surprising result don't you think!";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (personalDetails.Location.First() == "cavan" ) {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Ah Cavan - the home of my pa Johnathan Swift!"), cancellationToken);
                    await Task.Delay(1000);
                    var messageText = $"Anyways, An 11% incerase in first preference votes in Cavan for Sinn Féin! What did you think of that?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (personalDetails.Location.First() == "mayo" ) {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Ah Mayo on the Wild Atlantic Way - what a fabulous place it is!"), cancellationToken);
                    await Task.Delay(1000);
                    var messageText = $"Anyways, An 13% incerase in first preference votes in Mayo for Sinn Féin! What did you think of that?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (personalDetails.Location.First() == "louth" ) {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Ah yes Louth, what a fabulous place it is!"), cancellationToken);
                    await Task.Delay(1000);
                    var messageText = $"Anyways, 48% of first preference votes in Louth for Sinn Féin! What did you think of that?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (personalDetails.Location.First() == "cork") {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"ah you're from Cork! Sorry for your troubles."), cancellationToken);
                    await Task.Delay(1000);
                    var messageText = $"Sinn Féin with a 12.5% increase in votes since 2016... very surprising don't you think?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else if (personalDetails.Location.First() == "clare") {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"ah you're from Clare! It's a lovely place."), cancellationToken);
                    await Task.Delay(1000);
                    var messageText = $"Anyways, Fianna Fáil with 34% of first preference votes! Surprising result don't you think?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
                else {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Oh yes, {personalDetails.Location.First()}, I know the place."), cancellationToken);
                    await Task.Delay(1000);
                    var messageText = $"Most people think Sinn Féin's win was surprising, did you think that too?";
                    var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
                }
            }
        }

        private async Task<DialogTurnResult> AgreeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            
            await Task.Delay(2000);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Yeah I thought so too."), cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(IssuesDialog), personalDetails, cancellationToken);
        }
    }
}
