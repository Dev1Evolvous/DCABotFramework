using Microsoft.Bot.Builder.Dialogs;
using QnAMakerDialog;
using QnAMakerDialog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
namespace Microsoft.Bot.Sample.QnABot
{
    [Serializable]
    [QnAMakerService("https://qnamakerai.azurewebsites.net/qnamaker",
        "ec049042-e746-4f4b-99cc-38fdcdd74deb",
        "97a5f25c-0596-49ef-8916-b4ff4f2e860f",
        MaxAnswers = 10)]

    public class QnADialog : QnAMakerDialog<object>
    {
        public override async Task NoMatchHandler(IDialogContext context, string originalQueryText)
        {
            await context.PostAsync($"Sorry, I couldn't find an answer for '{originalQueryText}'.");
            context.Wait(MessageReceived);
        }


    }





}