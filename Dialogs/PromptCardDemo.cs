using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using SP = Microsoft.SharePoint.Client;
namespace Microsoft.Bot.Sample.QnABot
{
    [Serializable]
    public class PromptCardDemo : IDialog<object>
    {
        private string name;
        private long age;
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Thanks for using Bot Application registration \r\n\r\n Fill below details to complete registration");
            context.Wait(GetNameAsync);
        }

        private Task GetNameAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            PromptDialog.Text(
                context: context,
                resume: ResumeGetName,
                prompt: "Please enter your name",
                retry: "Sorry I didn't understand that. Please try again"
                );
            return Task.CompletedTask;
        }
        private async Task ResumeGetName(IDialogContext context, IAwaitable<string> result)
        {
            name = await result;
            PromptDialog.Number(
                context: context,
                resume: ResumeGetAge,
                prompt: $"{name}, Please enter your age",
                retry: "Sorry I didn't understand that. Please try again",
                attempts: 3,
                min: 20,
                max: 50
                );
        }

        private async Task ResumeGetAge(IDialogContext context, IAwaitable<long> result)
        {
            age = await result;
            PromptDialog.Confirm(
                context: context,
                resume: ResumeConfirm,
                prompt: $"Your name is *{name}* and your age is *{age}* Right?",
                retry: "Sorry I didn't understand that. Please try again",
                options: new string[] { "Yes", "No" },
                promptStyle: PromptStyle.PerLine
                );
        }

        private async Task ResumeConfirm(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {
                await context.PostAsync($"You are registered successfully. \r\n\r\n Your name is **{name}** and your age is **{age}**");
            }
            else
            {
                await context.PostAsync("Yeah, I have doubt");
                context.Done(string.Empty);
            }

        }
    }
}