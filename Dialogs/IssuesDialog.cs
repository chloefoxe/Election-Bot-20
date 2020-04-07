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
    public class IssuesDialog : ComponentDialog
    {
        private readonly ConversationRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        public IssuesDialog(ConversationRecognizer luisRecognizer, EndConversationDialog endConversationDialog, ILogger<IssuesDialog> logger)
            : base(nameof(IssuesDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(endConversationDialog);
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskIssues,
                ElaborateIssuesAsync,
                ContinueStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskIssues(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            
            if (personalDetails.Issues == null)
            {
                await Task.Delay(1000);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"If I was elected, I'd finally bring high speed broadband to Kerry!"), cancellationToken);
                await Task.Delay(1500);
                var messageText = $"So {personalDetails.Name.First()}, imagine you were elected in the morning, what kinds of issues with you raise in Dáil Eireann?";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);
            return await stepContext.NextAsync(personalDetails.Location, cancellationToken);
        }

        private async Task<DialogTurnResult> ElaborateIssuesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            var luisResult = await _luisRecognizer.RecognizeAsync<Luis.ElectionBot>(stepContext.Context, cancellationToken);

            string[] housing, mentalHealth, health, unemployment, coronavirus, crime, mortgage, teachersPay, education;
            housing = new string[]{ "housing/rental crisis"};
            coronavirus = new string[]{ "coronavirus"};
            health = new string[]{"health service"};
            unemployment = new string[]{"unemployment"};
            mentalHealth = new string[]{"mental health"};
            crime = new string[]{"crime"};
            mortgage = new string[]{"mortgage"};
            education = new string[]{"education"};
            teachersPay = new string[]{"teacher's pay"};

            await Task.Delay(1500);

            switch (luisResult.TopIntent().intent)
            {
                case Luis.ElectionBot.Intent.discussHousing:
                    personalDetails.Issues = housing;

                    var housingText = "I agree with you on the housing crisis. It's a huge problem in modern times. Hopefully it can be solved soon right?";
                    var housingPromptMessage = MessageFactory.Text(housingText, housingText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = housingPromptMessage }, cancellationToken);
                
                case Luis.ElectionBot.Intent.discussCoronavirus:
                    personalDetails.Issues = coronavirus;

                    var coronavirusText = "Indeed, the coronavirus is at the top of everyone's agenda at the minute. Hopefully we can find a vaccine soon, right? ";
                    var coronavirusPromptMessage = MessageFactory.Text(coronavirusText, coronavirusText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = coronavirusPromptMessage }, cancellationToken);

                case Luis.ElectionBot.Intent.discussHealth:
                    personalDetails.Issues = health;

                    var healthText = "I agree, Ireland definitely needs better health infrastructure. Hopefullly that can be solved soon right?";
                    var healthPromptMessage = MessageFactory.Text(healthText, healthText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = healthPromptMessage }, cancellationToken);

                case Luis.ElectionBot.Intent.discussUnemployment:
                    personalDetails.Issues = unemployment;

                    var unemploymentText = "Unemployment is going to be huge after this pandemic. Hopefully it wont' affect our lives too much right?";
                    var unemploymentPromptMessage = MessageFactory.Text(unemploymentText, unemploymentText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = unemploymentPromptMessage }, cancellationToken);
                
                case Luis.ElectionBot.Intent.discussMentalHealth:
                    personalDetails.Issues = mentalHealth;

                    var mentalHealthText = "Indeed. I couldn't agree more, mental health is a huge topic at the minute. Hopefully government can fix this soon right?";
                    var mentalHealthPromptMessage = MessageFactory.Text(mentalHealthText, mentalHealthText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = mentalHealthPromptMessage }, cancellationToken);

                case Luis.ElectionBot.Intent.discussCrime:
                    personalDetails.Issues = crime;

                    var crimeText = "Yes. Crime in general is a huge problem, but especically gangland crime, hopefully the situation improves soon, right?";
                    var crimePromptMessage = MessageFactory.Text(crimeText, crimeText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = crimePromptMessage }, cancellationToken);

                case Luis.ElectionBot.Intent.discussMortgage:
                    personalDetails.Issues = mortgage;

                    var mortgageText = "Hmmm, yes. I don't envy anyone who is building a house at the moment. Hopefully that situation can improve soon right?";
                    var mortgagePromptMessage = MessageFactory.Text(mortgageText, mortgageText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = mortgagePromptMessage }, cancellationToken);
                
                case Luis.ElectionBot.Intent.discussEducation:
                    personalDetails.Issues = mortgage;

                    var educationText = "I 100% agree. Ireland needs more universities, and more affordable education. Hopefully it improves soon, right?";
                    var educationPromptMessage = MessageFactory.Text(educationText, educationText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = educationPromptMessage }, cancellationToken);

                case Luis.ElectionBot.Intent.discussTeachersPay:
                    personalDetails.Issues = mortgage;

                    var tecahersPayText = "Ah yes, teachers, the backbone of society. They should be paid more, right?";
                    var tecahersPayPromptMessage = MessageFactory.Text(tecahersPayText, tecahersPayText, InputHints.ExpectingInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = tecahersPayPromptMessage }, cancellationToken);

                default:
                    var didntUnderstandMessageText = $"Yeah I see. Hopefully some of the issues can be solved soon right?";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = didntUnderstandMessage }, cancellationToken);
            }

            //return await stepContext.NextAsync(personalDetails, cancellationToken);
        }
        private async Task<DialogTurnResult> ContinueStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var personalDetails = (PersonalDetails)stepContext.Options;
            
            await Task.Delay(1500);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Absolutely"), cancellationToken);

            return await stepContext.BeginDialogAsync(nameof(EndConversationDialog), personalDetails, cancellationToken);
        }


    }
}
