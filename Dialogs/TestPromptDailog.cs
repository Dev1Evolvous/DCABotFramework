using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.FormFlow;

namespace Microsoft.Bot.Sample.QnABot
{
    [Serializable]
    public class TestPromptDailog : IDialog<object>
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
        private enum Selection
        {
            Yes, No
        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync($"Sorry! Didn't you means of these questions");
            var menuItems = SuggestionOptionsFromSP.GetAllQuestionOptions();
            IEnumerable<string> ienum = (IEnumerable<string>)menuItems;
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
             options: (IEnumerable<Selection>)Enum.GetValues(typeof(Selection)),
             prompt: "Are you satisfied?",
             retry: "Selected plan not avilabel . Please try again.",
             promptStyle: PromptStyle.Auto
             );
        }

        private async Task UserSatisfiedReceivedAsync(IDialogContext context, IAwaitable<Selection> result)
        {
            var selection = await result;
            if (selection.Equals(Selection.No))
            {
                PromptDialog.Choice(
                  context: context,
                  resume: ResumeDataFromOnlineSelectedQuestion,
                  options: (IEnumerable<Selection>)Enum.GetValues(typeof(Selection)),
                  prompt: "Are you want to get the data from online? ",
                  retry: "I didn't understand. Please try again.");
            }
            else
            {
                PromptDialog.Choice(
                 context: context,
                 resume: ContactUserSupportTeam,
                 options: (IEnumerable<Selection>)Enum.GetValues(typeof(Selection)),
                 prompt: "Are you want to contact with support team?",
                 retry: "I didn't understand. Please try again.");
            }
        }

        private async Task ContactUserSupportTeam(IDialogContext context, IAwaitable<Selection> result)
        {
            var selection = await result;
            if (selection.Equals(Selection.No))
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

        private async Task ResumeDataFromOnlineSelectedQuestion(IDialogContext context, IAwaitable<Selection> result)
        {
            var message = await result;

            if (message.Equals(Selection.No))
            {
                PromptDialog.Choice(
                  context: context,
                  resume: ContactUserSupportTeam,
                  options: (IEnumerable<Selection>)Enum.GetValues(typeof(Selection)),
                  prompt: "Are you want to contact with support team?",
                  retry: "I didn't understand. Please try again.");
            }
            else
            {
                await context.PostAsync($"Thanks for your keen request and here is data from online:- {HoldUserQuestion}");
                PromptDialog.Choice(
                 context: context,
                 resume: ResumeDataFromOnlineUserSatisfiedAsync,
                 options: (IEnumerable<Selection>)Enum.GetValues(typeof(Selection)),
                 prompt: "Are you satisfied?",
                 retry: "Selected plan not avilabel . Please try again.",
                 promptStyle: PromptStyle.Auto
                 );
            }
        }

        private async Task ResumeDataFromOnlineUserSatisfiedAsync(IDialogContext context, IAwaitable<Selection> result)
        {
            var selection = await result;
            if (selection.Equals(Selection.No))
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

