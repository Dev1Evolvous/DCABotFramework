using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
namespace Microsoft.Bot.Sample.QnABot
{
    [Serializable]
    public class FormFlowBuilder : IDialog<object>
    {

        public List<SuggestionQuestion> SuggestionQuestions
        {
            get;
            set;
        }

        public string HoldUserQuestion
        {
            get;
            set;
        }
        private enum ConfirmSelection
        {
            Yes, No
        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Sorry! Didn't you means of these questions\n {||} \n");
            var suggestionOptions = SuggestionOptionsFromSP.GetAllQuestionOptions();

            IEnumerable<string> ienum = (IEnumerable<string>)suggestionOptions;

            PromptDialog.Choice<string>(context, ResumeAfterSelectionQuestion,
                ienum, "Which question do you want?");
        }

        private async Task ResumeAfterSelectionQuestion(IDialogContext context, IAwaitable<string> result)
        {
            var selection = await result;
            HoldUserQuestion = selection;
            await context.PostAsync($"Answser of you selection:- {selection}.");
            PromptDialog.Choice(
             context: context,
             resume: UserSatisfiedReceivedAsync,
             options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
             prompt: "Are you satisfied?",
             retry: "Selected plan not avilabel . Please try again.",
             promptStyle: PromptStyle.Auto
             );
        }

        private async Task UserSatisfiedReceivedAsync(IDialogContext context, IAwaitable<ConfirmSelection> result)
        {
            var selection = await result;
            if (selection.Equals(ConfirmSelection.No))
            {
                PromptDialog.Choice(
                  context: context,
                  resume: ResumeDataFromOnlineSelectedQuestion,
                  options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                  prompt: "Are you want to get the data from online? ",
                  retry: "I didn't understand. Please try again.");
            }
            else
            {
                //PromptDialog.Choice(
                // context: context,
                // resume: ContactUserSupportTeam,
                // options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                // prompt: "Are you want to contact with support team?",
                // retry: "I didn't understand. Please try again.");

                await context.PostAsync("Thanks for your keen request.");
                context.Done(this);
            }
        }

        private async Task ContactUserSupportTeam(IDialogContext context, IAwaitable<ConfirmSelection> result)
        {
            var selection = await result;
            if (selection.Equals(ConfirmSelection.No))
            {
                await context.PostAsync("Thanks for your keen request.");
                context.Done(this);
            }
            else
            {
                await context.PostAsync("Thanks for your keen request. Our support team will contact you within 20 mins");
                context.Done(this);
            }
        }

        private async Task ResumeDataFromOnlineSelectedQuestion(IDialogContext context, IAwaitable<ConfirmSelection> result)
        {
            var message = await result;

            if (message.Equals(ConfirmSelection.No))
            {
                PromptDialog.Choice(
                  context: context,
                  resume: ContactUserSupportTeam,
                  options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                  prompt: "Are you want to contact with support team?",
                  retry: "I didn't understand. Please try again.");
            }
            else
            {
                await context.PostAsync($"Thanks for your keen request and here is data from online:- {HoldUserQuestion}");
                PromptDialog.Choice(
                 context: context,
                 resume: ResumeDataFromOnlineUserSatisfiedAsync,
                 options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                 prompt: "Are you satisfied?",
                 retry: "Selected plan not avilabel . Please try again.",
                 promptStyle: PromptStyle.Auto
                 );
            }
        }

        private async Task ResumeDataFromOnlineUserSatisfiedAsync(IDialogContext context, IAwaitable<ConfirmSelection> result)
        {
            var selection = await result;
            if (selection.Equals(ConfirmSelection.No))
            {
                await context.PostAsync("Thanks for your keen request. Our support team will contact you within 20 mins");
                context.Done(this);
            }
            else
            {
                await context.PostAsync("Thanks for your keen request.");

                context.Done(this);
            }
        }
    }
}