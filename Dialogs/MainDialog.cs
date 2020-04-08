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

        private readonly UserState _userState;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(UserState userState, ConversationRecognizer luisRecognizer, ElectionDialog electionDialog, UserProfileDialog userProfileDialog, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(userProfileDialog);
            AddDialog(electionDialog);
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
            await Task.Delay(1000);

            // // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "My name is BotWise, hope you are well?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        public async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            var personalDetails = new PersonalDetails();

            switch (luisResult.TopIntent().intent)
            {
                case Luis.ElectionBot.Intent.discussFeeling:
                    return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), personalDetails, cancellationToken);
                
                case Luis.ElectionBot.Intent.askMood:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm great! Thanks for asking."), cancellationToken);
                    return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), personalDetails, cancellationToken);

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string name, location, userID, voted, issues, party;

            if (stepContext.Result is PersonalDetails result)
            {
                if (result.Name == null){ 
                    name = "not disclosed";
                }
                else {
                    name = result.Name.First();
                }

                if (result.Location == null){ 
                    location = "not disclosed";
                }
                else {
                    location = result.Location.First();
                }

                if (result.UserID == null){ 
                    userID = "not disclosed";
                }
                else {
                    userID = result.UserID.First();
                }

                if (result.Voted == null){ 
                    voted = "not disclosed";
                }
                else {
                    voted = result.Voted.First();
                }

                if (result.Issues == null){ 
                    issues = "not disclosed";
                }
                else {
                    issues = result.Issues.First();
                }

                if (result.Party == null){ 
                    party = "not disclosed";
                }
                else {
                    party = result.Party.First();
                }

                /** -------------------- NAME --------------------- **/

                await Task.Delay(1500);

                if(name == "not disclosed"){
                    await Task.Delay(1);
                }
                else{
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"So first of all, your name is {name}"), cancellationToken);
                }

                /** -------------------- VOTING --------------------- **/

                await Task.Delay(1500);

                if(voted == "not disclosed"){
                    await Task.Delay(1);
                }
                else if(voted == "did vote"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You voted in the last general election, which probably means that you have an interest in politics and care about your right to vote."), cancellationToken);
                }
                else if(voted == "did not vote"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You did not vote in the last general election, which means you probably feel indifferent about polotics, or don't have the right to vote in Ireland."), cancellationToken);
                }

                /** -------------------- LOCATION --------------------- **/

                await Task.Delay(4000);

                if(location == "not disclosed"){
                    await Task.Delay(1);
                }
                else {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You're from {location}"), cancellationToken);
                }

                /** -------------------- ISSUES --------------------- **/
                
                await Task.Delay(1500);

                if(issues == "not disclosed"){
                    await Task.Delay(1);
                }
                else if(issues == "education"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You are either a teacher or a student, who cares about improving education in ireland."), cancellationToken);
                }
                else if(issues == "housing"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Improving housing is important to you, I can infer that you are paying expensive rent in dublin as a student or finding it difficult to find affordable housing."), cancellationToken);
                }
                else if(issues == "teachers pay"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I can guess that you're probably a teacher because you care about getting equal pay"), cancellationToken);
                }
                else if(issues == "health"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Improving health infrastructure is important to you so or someone you know have probably experienced long waiting times in hospitals recently."), cancellationToken);
                }
                else if(issues == "coronavirus"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You're worried about the coronavirus and the implications it may cause for society. There's a chance you could be part of an 'at risk' health group."), cancellationToken);
                }
                else if(issues == "mortgage"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You're worried about the mortgage situation, I can assume that you might be building a house in the future."), cancellationToken);
                }
                else if(issues == "unemployment"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Improving Ireland's employment rates are important to you which may mean that you might be unemployed at the minute."), cancellationToken);
                }
                else if(issues == "mental health"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($""), cancellationToken);
                }

                /** -------------------- PARTY --------------------- **/

                await Task.Delay(4000);

                if(location == "not disclosed"){
                    await Task.Delay(1);
                }
                else if(party == "green party" || party == "greens"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You support the Green Party"), cancellationToken);
                }
                else if(party == "sinn fein"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You support Sinn Féin, "), cancellationToken);
                }
                else if(party == "fianna fail"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You support Fianna Fáil"), cancellationToken);
                }
                else if(party == "fine gael"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You support Fine Gael"), cancellationToken);
                }
                else if(party == "labour"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You support Labour"), cancellationToken);
                }
                else if(party == "independent"){
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You see yourseld as an independent which possibly means you don't neccesarily align to any political party."), cancellationToken);
                }
            }

            await Task.Delay(10000);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
