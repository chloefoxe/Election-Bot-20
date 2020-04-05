// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ConversationRecognizer luisRecognizer, /*ElectionDialog electionDialog, PartyDialog partyDialog,*/ UserProfileDialog userProfileDialog, /*EndConversationDialog endConversationDialog,*/ ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            //AddDialog(electionDialog);
            AddDialog(userProfileDialog);
            //AddDialog(partyDialog);
            //AddDialog(endConversationDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the web.config file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "My name is BotWise, I'd love to have chat with you today! To kick things off, can I ask you your name? ";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        public async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), new PersonalDetails(), cancellationToken);
            }

            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            
            switch (luisResult.TopIntent().intent)
            {
                case Luis.ElectionBot.Intent.Greeting:
                    var userInfo = new PersonalDetails()
                    {
                        Name = luisResult.Entities.name,
                    };
                
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {userInfo.Name.FirstOrDefault()}, it's great to meet you!"), cancellationToken);

                    return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), userInfo, cancellationToken);

                    // Initialize BookingDetails with any entities we may have found in the response.
                    // var personalDetails = new PersonalDetails()
                    // {
                    //     // Get destination and origin from the composite entities arrays.
                    //     Destination = luisResult.ToEntities.Airport,
                    //     Origin = luisResult.FromEntities.Airport,
                    //     TravelDate = luisResult.TravelDate,
                    // };

                    // Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                    // return await stepContext.BeginDialogAsync(nameof(BookingDialog), personalDetails, cancellationToken);

                case Luis.ElectionBot.Intent.discussCandidate:
                    // // We haven't implemented the GetWeatherDialog so we just display a TODO message.
                    // var getWeatherMessageText = "TODO: get weather flow here";
                    // var getWeatherMessage = MessageFactory.Text(getWeatherMessageText, getWeatherMessageText, InputHints.IgnoringInput);
                    // await stepContext.Context.SendActivityAsync(getWeatherMessage, cancellationToken);
                    // break;

                case Luis.ElectionBot.Intent.discussLocation:
                
                case Luis.ElectionBot.Intent.discussParty:

                case Luis.ElectionBot.Intent.discussPersonal:

                case Luis.ElectionBot.Intent.discussPolitics:

                case Luis.ElectionBot.Intent.disscussIssues:

                default:
                    // Catch all for unhandled intents
                    //var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessageText = $"NONE INTENT";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("BookingDialog") was cancelled, the user failed to confirm or if the intent wasn't BookFlight
            // the Result here will be null.
            // if (stepContext.Result is PersonalDetails result)
            // {
            //     // Now we have all the booking details call the booking service.

            //     // If the call to the booking service was successful tell the user.

            //     var timeProperty = new TimexProperty(result.TravelDate);
            //     var travelDateMsg = timeProperty.ToNaturalLanguage(DateTime.Now);
            //     var messageText = $"I have you booked to {result.Destination} from {result.Origin} on {travelDateMsg}";
            //     var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            //     await stepContext.Context.SendActivityAsync(message, cancellationToken);
            // }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
