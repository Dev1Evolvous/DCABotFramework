using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using SP = Microsoft.SharePoint.Client;
using System.Security;
using System.Configuration;
using Microsoft.Bot.Builder.FormFlow;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.IO;
using edu.stanford.nlp.coref.data;
using java.io;
using edu.stanford.nlp.process;
using com.sun.org.apache.xerces.@internal.impl.io;
using ikvm.io;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using System.ComponentModel;


namespace Microsoft.Bot.Sample.QnABot
{
    [Serializable]
    [LuisModel("402fd33a-ccd4-4f55-8880-d9eb50fc1ac7", "bfffb3860f844883a565c02e8202480f")]
    public class LuisDialogs : LuisDialog<object>
    {
        const string obtainDescriptionFromImage = "Description of image";
        private const string BING_KEY = "9f6d1e6a20d248f79592c4a0b2647cd4";
        //const AzureRegions VISION_REGION = AzureRegions;
        private string searchType = string.Empty;
        private const string searchWeb = "Search Web";

        private enum WelcomeAwesome
        {
            howdy,
            [Description("welcome")]
            welcome,
            [Description("how goes it")]
            howgoes,
            [Description("what's happening")]
            whathappen,
            [Description("What's up")]
            whatup,
            [Description("Good morning")]
            Goodmoring,
            [Description("Hey")]
            hey,
            [Description("How are you")]
            howryou,
            [Description("Hello")]
            hello,
            hi,
            gm,
            [Description("good evening")]
            Goodevening,
            [Description("Good Morning")]
            Goodmorning

        }

        private enum ThankAwesome
        {
            Bye,
            [Description("Good bye")]
            goodbye,
            [Description("bye bye")]
            byebye,
            [Description("byebye")]
            byebye1,
            [Description("goodnight")]
            goodnight,
            [Description("goodbye")]
            goodbye1,
            [Description("goodbyes")]
            goodbyes,
            [Description("byeee")]
            byeee,
            [Description("goobye")]
            goobye,
            [Description("byeeee")]
            byeeee,
            [Description("takecare")]
            takecare,
            [Description("okok")]
            okok,
            [Description("okayyyy")]
            okayyyy

        }
        private enum ConfirmSelection
        {
            Yes, No
        }
        public string HoldUserQuestion
        {
            get;
            set;
        }

        public string HoldUserAnswerAboutQuestion
        {
            get;
            set;
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            try
            {
                var typingMsg = context.MakeMessage();
                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);

                string reply = string.Empty;
                var messageToForward = await message as Activity;

                HoldUserQuestion = messageToForward.Text;

                GetDataFromSP getDataFromSP = new GetDataFromSP();

                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);
                double? scoredEntity = 0.0;
                double? scoredSent = 0.0;
                double? scored = luisResult.TopScoringIntent.Score;
                string entityRole = string.Empty;
                string sentRole = string.Empty;
                string sentLabel = string.Empty;
                var activity = await message as Activity;
                StockLUIS stockLUIS = new StockLUIS();
                SentimentLUIS sentimentLUIS = new SentimentLUIS();
                List<string> listResult = new List<string>();
                listResult = await getDataFromSP.GetIntentSentimentScore(activity.Text);
                if (listResult.Count == 6)
                {

                    for (int i = 0; i < listResult.Count; i++)
                    {
                        if (i == 0)
                        {
                            entityRole = listResult[0];
                        }
                        if (i == 1)
                        {

                            scoredEntity = Convert.ToDouble(listResult[1].Length > 0 ? listResult[1] : "0");
                        }
                        if (i == 2)
                        {
                            sentRole = listResult[2];
                        }
                        if (i == 3)
                        {
                            sentLabel = listResult[3];
                        }
                        //if (i == 4)
                        //    scoredSent = Convert.ToDouble(listResult[3]);
                        if (i == 5)
                        {
                            scoredSent = Convert.ToDouble(listResult[5].Length > 0 ? listResult[5] : "0");
                        }
                    }
                }

                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);

                // First run simple query 
                List<HRResultList> faqReport = new List<HRResultList>();
                faqReport = DCAFAQQuestionDataWithQuery(activity.Text);

                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);

                if (faqReport.Count > 0)
                {
                    foreach (var item in faqReport)
                    {
                        if (item.ResultItem.Length > 0)
                        {
                            await context.PostAsync(item.ResultItem);
                        }
                        else
                        {
                            HoldUserAnswerAboutQuestion = "";
                            HoldUserQuestion = activity.Text;
                        }
                    }
                }
                else
                {
                    if (entityRole.ToString().ToLower().Equals("Parternity Leave".ToLower())
                       || entityRole.ToString().ToLower().Equals("Materni Leave".ToLower())
                       || entityRole.ToString().ToLower().Equals("Earn Leave".ToLower())
                       || entityRole.ToString().ToLower().Equals("Sick Leave".ToLower())
                       || entityRole.ToString().ToLower().Equals("Casual Leave".ToLower()))
                    {
                        typingMsg.Type = ActivityTypes.Typing;
                        typingMsg.Text = null;
                        await context.PostAsync(typingMsg);

                        EntityRecommendation entity = null;
                        string leaveReport = string.Empty;
                        luisResult.TryFindEntity("Leave.Type", out entity);
                        if (entity != null)
                        {
                            object leaveBalance;
                            var userName = context.Activity.From.Name;
                            if (string.IsNullOrEmpty(userName))
                                userName = "dev1";
                            string messageName = $"Hello {userName}! ...";
                            await context.PostAsync(messageName);
                            entity.Resolution.TryGetValue("values", out leaveBalance);
                            List<string> sourceName = ((System.Collections.IEnumerable)leaveBalance).Cast<string>().ToList();
                            foreach (string item in sourceName)
                            {
                                if (item.Equals("Paternity Leave"))
                                {
                                    leaveReport = FetchUserLeaveBalance(userName, item.ToString());
                                    await context.PostAsync(leaveReport);
                                    break;
                                }
                                else if (item.Equals("Maternity Leave"))
                                {
                                    leaveReport = FetchUserLeaveBalance(userName, item.ToString());
                                    await context.PostAsync(leaveReport);
                                    break;
                                }
                                else if (item.Equals("Casual Leave"))
                                {
                                    leaveReport = FetchUserLeaveBalance(userName, item.ToString());
                                    await context.PostAsync(leaveReport);
                                    break;
                                }
                                else if (item.Equals("Sick Leave"))
                                {
                                    leaveReport = FetchUserLeaveBalance(userName, item.ToString());
                                    await context.PostAsync(leaveReport);
                                    break;
                                }
                                else if (item.Equals("Earn Leave"))
                                {
                                    leaveReport = FetchUserLeaveBalance(userName, item.ToString());
                                    await context.PostAsync(leaveReport);
                                    break;
                                }
                                else
                                {
                                    //leaveReport = FetchUserLeaveBalance(userName, item.ToString());
                                    await context.PostAsync("No data available");
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //string reply = "Sorry, We have some suggested question(s) for you";
                        reply = "Sorry, I was not able to find any results. ";
                        await context.PostAsync(reply);
                        //await context.PostAsync("We have some suggested question(s) for you");

                        typingMsg.Type = ActivityTypes.Typing;
                        typingMsg.Text = null;
                        await context.PostAsync(typingMsg);

                        SuggestionOptionsFromSP suggestionOptionsFromSP = new SuggestionOptionsFromSP();
                        List<string> suggestionOptions = suggestionOptionsFromSP.GetAllQuestionSharePointOptions(entityRole);

                        typingMsg.Type = ActivityTypes.Typing;
                        typingMsg.Text = null;
                        await context.PostAsync(typingMsg);

                        IEnumerable<string> ienum = (IEnumerable<string>)suggestionOptions;
                        if (ienum != null)
                        {
                            if (ienum.Count() > 0)
                            {
                                PromptDialog.Choice(
                                  context: context,
                                  resume: ResumeAfterSelectionQuestion,
                                  options: ienum,
                                  prompt: "Which question do you want?",
                                  retry: "I didn't understand. Please try again."
                            );
                            }
                            else
                            {
                                PromptDialog.Choice(
                                  context: context,
                                  resume: ResumeDataFromOnlineSelectedQuestion,
                                  options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                                  prompt: "Are you want to get the data from online? ",
                                  retry: "I didn't understand. Please try again.");
                            }
                        }
                        else
                        {
                            PromptDialog.Choice(
                              context: context,
                              resume: ResumeDataFromOnlineSelectedQuestion,
                              options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                              prompt: "Are you want to get the data from online? ",
                              retry: "I didn't understand. Please try again.");
                        }
                    }
                }
                //context.Wait(MessageReceived);

                //PromptDialog.Choice(
                //  context: context,
                //  resume: ResumeDataFromOnlineSelectedQuestion,
                //  options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                //  prompt: "Are you want to get the data from online? ",
                //  retry: "I didn't understand. Please try again.");

                ////context.Wait(MessageReceived);
            }
            catch (Exception Ex)
            {
                //throw Ex;
                await context.PostAsync(Ex.Message);
            }
        }

        //public static readonly TokenizerFactory TokenizerFactory = PTBTokenizer.factory(new CoreLabelTokenFactory(),
        //        "normalizeParentheses=false,normalizeOtherBrackets=false,invertible=true");

        //public string ParseFile(string fileName)
        //{
        //    using (var stream = System.IO.File.OpenRead(fileName))
        //    {
        //        return SplitSentences(stream);
        //    }
        //}

        //public string SplitSentences(Stream stream)
        //{
        //    var preProcessor = new edu.stanford.nlp.process.DocumentPreprocessor(new UTF8Reader(new InputStreamWrapper(stream)));
        //    preProcessor.setTokenizerFactory(TokenizerFactory);

        //    foreach (java.util.List sentence in preProcessor)
        //    {
        //        return ProcessSentence(sentence);
        //    }
        //    return "";
        //}

        //// print the sentence with original spaces and punctuation.
        //public string ProcessSentence(java.util.List sentence)
        //{
        //    return edu.stanford.nlp.util.StringUtils.joinWithOriginalWhiteSpace(sentence);
        //}



        [LuisIntent("Welcome")]
        public async Task Welcome(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {

            try
            {
                var typingMsg = context.MakeMessage();
                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);
                var messageToForward = await message as Activity;
                bool skipStep = false;

                foreach (WelcomeAwesome colorEnum in Enum.GetValues(typeof(WelcomeAwesome)))
                {
                    var temp = colorEnum.GetDescription();

                    if (messageToForward.Text.ToString().ToLower().Equals(colorEnum.GetDescription().ToString().ToLower()))
                    {
                        string reply = string.Empty;
                        var userName = context.Activity.From.Name;
                        if (string.IsNullOrEmpty(userName))
                            userName = "dev1";
                        reply = $"Hello {userName} ! Welcome to our bot. What can I do for you?";
                        await context.PostAsync(reply);
                        skipStep = true;
                        break;
                    }
                }

                if (skipStep.Equals(false))
                {
                    HoldUserQuestion = messageToForward.Text;
                    PromptDialog.Choice(
                      context: context,
                      resume: ResumeDataFromOnlineSelectedQuestion,
                      options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                      prompt: "Are you want to get the data from online? ",
                      retry: "I didn't understand. Please try again.");
                }
                ////context.Wait(MessageReceived);
            }
            catch (Exception Ex)
            {
                //throw Ex;
                await context.PostAsync(Ex.Message);
            }
        }



        [LuisIntent("LeaveQuery")]
        public async Task QueryLeave(IDialogContext context, LuisResult luisResult)
        {
            try
            {
                var typingMsg = context.MakeMessage();
                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);

                EntityRecommendation entity = null;
                luisResult.TryFindEntity("LeaveType", out entity);

                var userName = context.Activity.From.Name;
                if (string.IsNullOrEmpty(userName))
                    userName = "dev1";
                string message = $"Hello {userName}! ...";
                await context.PostAsync(message);
                string leaveReport = FetchUserLeave(userName);
                if (leaveReport.Length > 0)
                    await context.PostAsync(leaveReport);
                else
                {
                    await context.PostAsync("No data available");
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        [LuisIntent("Thankswelcome")]
        public async Task ThanksWelcome(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            try
            {
                var typingMsg = context.MakeMessage();
                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);
                var messageToForward = await message as Activity;

                EntityRecommendation entity = null;
                luisResult.TryFindEntity("LeaveType", out entity);

                var userName = context.Activity.From.Name;
                if (string.IsNullOrEmpty(userName))
                    userName = "dev1";

                bool skipStep = false;

                foreach (ThankAwesome colorEnum in Enum.GetValues(typeof(ThankAwesome)))
                {
                    if (messageToForward.Text.ToString().ToLower().Equals(colorEnum.GetDescription().ToString().ToLower()))
                    {
                        string reply = string.Empty;
                        string message1 = $"We are happy to help you";
                        await context.PostAsync(message1);
                        context.Done(this);
                        skipStep = true;
                        break;
                    }
                }

                if (skipStep.Equals(false))
                {
                    HoldUserQuestion = messageToForward.Text;
                    PromptDialog.Choice(
                      context: context,
                      resume: ResumeDataFromOnlineSelectedQuestion,
                      options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                      prompt: "Are you want to get the data from online? ",
                      retry: "I didn't understand. Please try again.");
                }
                //context.Wait(MessageReceived);

            }
            catch (Exception Ex)
            {
                //throw Ex;
                await context.PostAsync(Ex.Message);
            }
        }

        [LuisIntent("Leave.Query")]
        public async Task QueryLeaveBalance(IDialogContext context, LuisResult luisResult)
        {
            try
            {
                var typingMsg = context.MakeMessage();
                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);

                EntityRecommendation entity = null;
                string leaveReport = string.Empty;
                luisResult.TryFindEntity("Leave.Type", out entity);
                if (entity != null)
                {
                    object leaveBalance;
                    var userName = context.Activity.From.Name;
                    if (string.IsNullOrEmpty(userName))
                        userName = "dev1";
                    string message = $"Hello {userName}! ...";
                    await context.PostAsync(message);
                    entity.Resolution.TryGetValue("values", out leaveBalance);
                    List<string> sourceName = ((System.Collections.IEnumerable)leaveBalance).Cast<string>().ToList();
                    foreach (string item in sourceName)
                    {
                        if (item.Equals("Paternity Leave"))
                        {
                            leaveReport = FetchUserLeaveBalance(userName, item.ToString());
                            await context.PostAsync(leaveReport);
                            break;
                        }
                        else if (item.Equals("Maternity Leave"))
                        {
                            leaveReport = FetchUserLeaveBalance(userName, item.ToString());
                            await context.PostAsync(leaveReport);
                            break;
                        }
                        else if (item.Equals("Casual Leave"))
                        {
                            leaveReport = FetchUserLeaveBalance(userName, item.ToString());
                            await context.PostAsync(leaveReport);
                            break;
                        }
                        else if (item.Equals("Sick Leave"))
                        {
                            leaveReport = FetchUserLeaveBalance(userName, item.ToString());
                            await context.PostAsync(leaveReport);
                            break;
                        }
                        else if (item.Equals("Earn Leave"))
                        {
                            leaveReport = FetchUserLeaveBalance(userName, item.ToString());
                            await context.PostAsync(leaveReport);
                            break;
                        }
                        else
                        {
                            //leaveReport = FetchUserLeaveBalance(userName, item.ToString());
                            await context.PostAsync("No data available");
                            break;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                //throw Ex;
                await context.PostAsync(Ex.Message);
            }
        }

        [LuisIntent("ITFAQ")]
        public async Task ITFAQSuggestionQuestion(IDialogContext context, IAwaitable<IMessageActivity> message,
            LuisResult luisResult)
        {
            try
            {

                GetDataFromSP getDataFromSP = new GetDataFromSP();

                var typingMsg = context.MakeMessage();
                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);
                double? scoredEntity = 0.0;
                double? scoredSent = 0.0;
                double? scored = luisResult.TopScoringIntent.Score;
                string entityRole = string.Empty;
                string sentRole = string.Empty;
                string sentLabel = string.Empty;
                var activity = await message as Activity;
                StockLUIS stockLUIS = new StockLUIS();
                SentimentLUIS sentimentLUIS = new SentimentLUIS();
                List<string> listResult = new List<string>();
                listResult = await getDataFromSP.GetIntentSentimentScore(activity.Text);

                if (listResult.Count == 6)
                {
                    for (int i = 0; i < listResult.Count; i++)
                    {
                        if (i == 0)
                            entityRole = listResult[0];
                        if (i == 1)
                            scoredEntity = Convert.ToDouble(listResult[1].Length > 0 ? listResult[1] : "0");
                        if (i == 2)
                            sentRole = listResult[2];
                        if (i == 3)
                            sentLabel = listResult[3];
                        //if (i == 4)
                        //    scoredSent = Convert.ToDouble(listResult[3]);
                        if (i == 5)
                            scoredSent = Convert.ToDouble(listResult[5].Length > 0 ? listResult[5] : "0");
                    }
                }

                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);

                // First run simple query 
                List<HRResultList> faqReport = new List<HRResultList>();
                faqReport = DCAFAQQuestionDataWithQuery(activity.Text);

                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);

                if (faqReport.Count > 0)
                {
                    foreach (var item in faqReport)
                    {
                        if (item.ResultItem.Length > 0)
                        {
                            await context.PostAsync(item.ResultItem);
                        }
                        else
                        {
                            HoldUserAnswerAboutQuestion = "";
                            HoldUserQuestion = activity.Text;
                        }
                    }
                }
                else
                {
                    //string reply = "Sorry, We have some suggested question(s) for you";
                    await context.PostAsync("Sorry, We have some suggested question(s) for you");

                    typingMsg.Type = ActivityTypes.Typing;
                    typingMsg.Text = null;
                    await context.PostAsync(typingMsg);

                    SuggestionOptionsFromSP suggestionOptionsFromSP = new SuggestionOptionsFromSP();
                    List<string> suggestionOptions = suggestionOptionsFromSP.GetAllQuestionSharePointOptions(entityRole);

                    typingMsg.Type = ActivityTypes.Typing;
                    typingMsg.Text = null;
                    await context.PostAsync(typingMsg);

                    IEnumerable<string> ienum = (IEnumerable<string>)suggestionOptions;

                    if (ienum != null)
                    {
                        if (ienum.Count() > 0)
                        {
                            PromptDialog.Choice(
                              context: context,
                              resume: ResumeAfterSelectionQuestion,
                              options: ienum,
                              prompt: "Which question do you want?",
                              retry: "I didn't understand. Please try again."
                        );
                        }
                        else
                        {
                            PromptDialog.Choice(
                              context: context,
                              resume: ResumeDataFromOnlineSelectedQuestion,
                              options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                              prompt: "Are you want to get the data from online? ",
                              retry: "I didn't understand. Please try again.");
                        }
                    }
                    else
                    {
                        PromptDialog.Choice(
                          context: context,
                          resume: ResumeDataFromOnlineSelectedQuestion,
                          options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                          prompt: "Are you want to get the data from online? ",
                          retry: "I didn't understand. Please try again.");
                    }

                }
                //context.Wait(MessageReceived);
                //EntityRecommendation location;
                //EntityRecommendation POS;

                //luisResult.TryFindEntity("Weather.Location", out location);
                //luisResult.TryFindEntity("POS", out POS);

                ////I tried with passing entities (it doesn't recognize the entities in formBuild)

                //if (scoredSent > 90)
                //{
                //    formFlow = new FormFlow();
                //}
                //else
                //{
                //    List<string> listString = new List<string>();
                //    listString.Add("What is SharePoint");
                //    listString.Add("Define SharePoint");
                //    listString.Add("Explain SharePoint");
                //    listString.Add("How is SharePoint");

                //    IEnumerable<string> ienum = (IEnumerable<string>)listString;

                //    if (activity.Text.ToLower().Contains("yes"))
                //    {
                //        PromptDialog.Text(
                //            context: context,

                //            resume: ResumeGetPhone,
                //            prompt: "Please share your good name",
                //            retry: "Sorry, I didn't understand that. Please try again."
                //        );
                //    }
                //    else
                //    {
                //        context.Done(this);
                //    }
                //}

                //string reply = "Sorry, We have some suggested question(s) for you";
                //await context.PostAsync(reply);
            }
            catch (Exception Ex)
            {
                //throw Ex;
                await context.PostAsync(Ex.Message);
            }
        }

        private async Task ResumeAfterSelectionQuestion(IDialogContext context, IAwaitable<string> result)
        {
            var selection = await result;
            HoldUserQuestion = selection;
            //await context.PostAsync($"Answser of you selection:- {selection}.");
            //if (HoldUserAnswerAboutQuestion != null)
            //{
            //    if (HoldUserAnswerAboutQuestion.Length > 0)
            //    {
            //        PromptDialog.Choice(
            //        context: context,
            //        resume: UserSatisfiedReceivedAsync,
            //        options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
            //        prompt: "Are you satisfied?",
            //        retry: "Selected plan not avilabel. Please try again.",
            //        promptStyle: PromptStyle.Auto
            //        );
            //    }
            //}
            //else
            //    {
            var typingMsg = context.MakeMessage();
            List<HRResultList> faqReport = new List<HRResultList>();
            faqReport = DCAFAQQuestionDataWithQuery(HoldUserQuestion);
            typingMsg.Type = ActivityTypes.Typing;
            typingMsg.Text = null;
            await context.PostAsync(typingMsg);

            if (faqReport.Count > 0)
            {
                foreach (var item in faqReport)
                {
                    if (item.ResultItem.Length > 0)
                    {
                        await context.PostAsync(item.ResultItem);
                    }
                    else
                    {
                        HoldUserAnswerAboutQuestion = "";
                        //HoldUserQuestion = activity.Text;
                        PromptDialog.Choice(
                         context: context,
                         resume: ResumeDataFromOnlineSelectedQuestion,
                         options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                         prompt: "Sorry, we have no answer. Can we go for online? ",
                         retry: "I didn't understand. Please try again.");
                    }
                }
            }
            else
            {
                PromptDialog.Choice(
                 context: context,
                 resume: ResumeDataFromOnlineSelectedQuestion,
                 options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                 prompt: "Sorry, we have no answer. Can we go for online? ",
                 retry: "I didn't understand. Please try again.");
            }
            // }

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

        private string FetchUserLeaveBalance(string userName, string leaveType)
        {
            string userNameResult = string.Empty;
            try
            {
                string result = string.Empty;
                using (SP.ClientContext context = new SP.ClientContext("https://Evolvous.sharepoint.com/sites/bot"))
                {
                    SecureString securePassword = GetSecureString("6.F43H7.CwuBuXA");
                    //context.Credentials = new SP.SharePointOnlineCredentials("admin@supertechinfo.onmicrosoft.com", securePassword);
                    context.Credentials = new SP.SharePointOnlineCredentials("dev1@evolvous.com", securePassword);

                    SP.List list = context.Web.Lists.GetByTitle("PendingLeaves");

                    SP.User user = context.Web.CurrentUser;
                    try
                    {
                        userNameResult = user.Id.ToString();
                    }
                    catch (Exception Ex)
                    {
                        userNameResult = userName.ToString();
                    }
                    SP.CamlQuery query = new SP.CamlQuery();
                    query.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='Title' /><Value Type='Text'>{userName}</Value></Eq></Where></Query></View>";
                    //query.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='Title' /><Value Type='Text'>User</Value></Eq></Where></Query></View>";

                    SP.ListItemCollection items = list.GetItems(query);
                    context.Load(items);
                    context.ExecuteQuery();
                    result = $"Total Count: {items.Count} \n";

                    foreach (SP.ListItem itm in items)
                    {
                        if (leaveType.Equals("Paternity Leave"))
                        {
                            result += $"\n\n Paternity Leaves: {itm["PaternityLeaves"]} ";

                        }
                        else if (leaveType.Equals("Maternity Leave"))
                        {
                            result += $"\n\n Maternity Leaves: {itm["MaternityLeaves"]} ";
                        }
                        else if (leaveType.Equals("Casual Leave"))
                        {
                            result += $"\n\n Casual Leaves: {itm["CasualLeaves"]}";

                        }
                        else if (leaveType.Equals("Sick Leave"))
                        {
                            result += $"\n\n Sick Leaves: {itm["SickLeaves"]}";
                        }
                        else if (leaveType.Equals("Earn Leave"))
                        {
                            result += $"\n\n Earn Leaves: {itm["EarnLeaves"]}";
                        }
                        else
                        {
                            result += $"\n\n Earn Leaves: {itm["EarnLeaves"]}";
                            result += $"\n\n Casual Leaves: {itm["CasualLeaves"]}";
                            result += $"\n\n Sick Leaves: {itm["SickLeaves"]}";
                            result += $"\n\n Paternity Leaves: {itm["PaternityLeaves"]} ";
                        }
                    }

                    return result;
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        [LuisIntent("FAQ")]
        public async Task FAQ(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            try
            {
                GetDataFromSP getDataFromSP = new GetDataFromSP();
                List<HRResultList> faqReport;
                faqReport = new List<HRResultList>();

                var typingMsg = context.MakeMessage();
                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);
                double? scoredEntity = 0.0;
                double? scoredSent = 0.0;
                double? scored = luisResult.TopScoringIntent.Score;
                string entityRole = string.Empty;
                string sentRole = string.Empty;
                string sentLabel = string.Empty;
                var activity = await message as Activity;
                StockLUIS stockLUIS = new StockLUIS();
                SentimentLUIS sentimentLUIS = new SentimentLUIS();
                List<string> listResult = new List<string>();
                #region
                //using (HttpClient client = new HttpClient())
                //{
                //    string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/5e61631e-1925-4437-bc6d-8db02168c1ae?verbose=true&timezoneOffset=0&subscription-key=7c25d040f0c74f0ebc5118eef3142261&q=" + activity.Text;
                //    HttpResponseMessage msg = await client.GetAsync(RequestURI);
                //    if (msg.IsSuccessStatusCode)
                //    {
                //        try
                //        {
                //            var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                //            JavaScriptSerializer js = new JavaScriptSerializer();
                //            stockLUIS = js.Deserialize<StockLUIS>(JsonDataResponse);
                //            sentimentLUIS = js.Deserialize<SentimentLUIS>(JsonDataResponse);
                //        }
                //        catch (Exception Ex)
                //        {
                //            await context.PostAsync(Ex.Message);
                //        }
                //    }
                //}

                listResult = await getDataFromSP.GetIntentSentimentScore(activity.Text);

                for (int i = 0; i < listResult.Count; i++)
                {
                    if (i == 0)
                        entityRole = listResult[0];
                    if (i == 1)
                        scoredEntity = Convert.ToDouble(listResult[1]);
                    if (i == 2)
                        sentRole = listResult[2];
                    if (i == 3)
                        sentLabel = listResult[3];
                    //if (i == 4)
                    //    scoredSent = Convert.ToDouble(listResult[3]);
                    if (i == 5)
                        scoredSent = Convert.ToDouble(listResult[5]);
                }
                //Console.WriteLine(list[i]);
                //return Data;
                #endregion
                //if (stockLUIS.Entities != null)
                //{
                //    foreach (var item in stockLUIS.Entities)
                //    {
                //        entityRole = item.entity;
                //        scoredEntity = item.score;
                //    }
                //}

                if (scored > scoredEntity)
                {
                    scoredEntity = scored;
                }

                if (scoredEntity > 0.80)
                {
                    var userName = context.Activity.From.Name;
                    //ISpellCheckAPI client =new ISpellCheckAPI() 
                    //string message1 = $"...";
                    //await context.PostAsync(message1);
                    faqReport = FetchUserFAQ(entityRole);

                    if (faqReport.Count > 0)
                    {
                        foreach (var item in faqReport)
                        {
                            await context.PostAsync(item.ResultItem);
                        }
                    }
                    else
                    {
                        HoldUserQuestion = activity.Text;
                        PromptDialog.Choice(
                          context: context,
                          resume: ResumeDataFromOnlineSelectedQuestion,
                          options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                          prompt: "Are you want to get the data from online? ",
                          retry: "I didn't understand. Please try again.");
                    }
                }
                else
                {

                    HoldUserQuestion = activity.Text;
                    PromptDialog.Choice(
                      context: context,
                      resume: ResumeDataFromOnlineSelectedQuestion,
                      options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                      prompt: "Are you want to get the data from online? ",
                      retry: "I didn't understand. Please try again.");

                }
            }
            catch (Exception Ex)
            {
                //throw Ex;
                await context.PostAsync(Ex.Message);
            }
        }

        [LuisIntent("HRPolicy")]
        public async Task FAQHRPolicy(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            try
            {
                var activity = await message as Activity;
                double? scoredEntity = 0.0;
                double? scored = luisResult.TopScoringIntent.Score;
                string entityRole = string.Empty;
                var typingMsg = context.MakeMessage();
                typingMsg.Type = ActivityTypes.Typing;
                typingMsg.Text = null;
                await context.PostAsync(typingMsg);

                StockLUIS stockLUIS = new StockLUIS();
                #region
                using (HttpClient client = new HttpClient())
                {
                    string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/402fd33a-ccd4-4f55-8880-d9eb50fc1ac7?verbose=true&timezoneOffset=0&subscription-key=bfffb3860f844883a565c02e8202480f&q=" + activity.Text;
                    HttpResponseMessage msg = await client.GetAsync(RequestURI);
                    if (msg.IsSuccessStatusCode)
                    {
                        try
                        {
                            var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            stockLUIS = js.Deserialize<StockLUIS>(JsonDataResponse);
                        }
                        catch (Exception Ex)
                        {
                            await context.PostAsync(Ex.Message);
                        }
                    }
                }
                //return Data;
                #endregion

                if (stockLUIS.Entities != null)
                {
                    foreach (var item in stockLUIS.Entities)
                    {
                        entityRole = item.entity;
                        scoredEntity = item.score;
                    }
                }
                if (scored > scoredEntity)
                {
                    scoredEntity = scored;
                }

                if (scoredEntity > 0.80)
                {
                    var userName = context.Activity.From.Name;

                    List<HRResultList> faqReport = FetchHRFAQ("Human Resource");

                    if (GetFAQHRArticle(context, faqReport, entityRole) != null)
                    {
                        await context.PostAsync(GetFAQHRArticle(context, faqReport, entityRole));
                        //context.Wait(MessageReceived);
                    }
                    else
                    {
                        HoldUserQuestion = activity.Text;
                        PromptDialog.Choice(
                          context: context,
                          resume: ResumeDataFromOnlineSelectedQuestion,
                          options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                          prompt: "Are you want to get the data from online? ",
                          retry: "I didn't understand. Please try again.");
                    }
                }
                else
                {
                    HoldUserQuestion = activity.Text;
                    PromptDialog.Choice(
                      context: context,
                      resume: ResumeDataFromOnlineSelectedQuestion,
                      options: (IEnumerable<ConfirmSelection>)Enum.GetValues(typeof(ConfirmSelection)),
                      prompt: "Are you want to get the data from online? ",
                      retry: "I didn't understand. Please try again.");
                }
            }
            catch (Exception Ex)
            {
                //throw Ex;
                await context.PostAsync(Ex.Message);
            }
        }

        [LuisIntent("Age")]
        public async Task AgeIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            var typingMsg = context.MakeMessage();
            typingMsg.Type = ActivityTypes.Typing;
            typingMsg.Text = null;
            await context.PostAsync(typingMsg);

            var messageToForward = await message as Activity;

            string reply = "Sorry, My age is not defined";
            await context.PostAsync(reply);
        }

        [LuisIntent("Appearance")]
        public async Task AppearanceIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            var typingMsg = context.MakeMessage();
            typingMsg.Type = ActivityTypes.Typing;
            typingMsg.Text = null;
            await context.PostAsync(typingMsg);

            var messageToForward = await message as Activity;

            string reply = "Sorry, My age is not defined";
            await context.PostAsync(reply);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            var typingMsg = context.MakeMessage();
            typingMsg.Type = ActivityTypes.Typing;
            typingMsg.Text = null;
            await context.PostAsync(typingMsg);

            var messageToForward = await message as Activity;

            //string reply = "What can I help you?";
            //await context.PostAsync(reply);

            Attachment attachment = GetHelpAskingQuestion();
            typingMsg.Attachments = new List<Attachment> { attachment };
            await context.PostAsync(typingMsg);

        }

        private Attachment GetHelpAskingQuestion()
        {
            return new Attachment
            {
                Name = "Queries! You can ask questions from bot",
                ContentType = "application/pdf",
                ContentUrl = "https://evolvous.sharepoint.com/Shared%20Documents/Learning_Azure_Cognitive_Services.pdf"

            };
        }

        [LuisIntent("Hobby")]
        public async Task HobbyIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            var typingMsg = context.MakeMessage();
            typingMsg.Type = ActivityTypes.Typing;
            typingMsg.Text = null;
            await context.PostAsync(typingMsg);


            string reply = "My hobby is to help user";
            await context.PostAsync(reply);
        }
        [LuisIntent("Language")]
        public async Task LanguageIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            var typingMsg = context.MakeMessage();
            typingMsg.Type = ActivityTypes.Typing;
            typingMsg.Text = null;
            await context.PostAsync(typingMsg);

            string reply = "I know only english";
            await context.PostAsync(reply);
        }

        [LuisIntent("Location")]
        public async Task LocationIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            var typingMsg = context.MakeMessage();
            typingMsg.Type = ActivityTypes.Typing;
            typingMsg.Text = null;
            await context.PostAsync(typingMsg);


            string reply = "Your city";
            await context.PostAsync(reply);
        }

        [LuisIntent("Name")]
        public async Task NameIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            var typingMsg = context.MakeMessage();
            typingMsg.Type = ActivityTypes.Typing;
            typingMsg.Text = null;
            await context.PostAsync(typingMsg);

            string reply = "My name is DCA Bot";
            await context.PostAsync(reply);
        }

        [LuisIntent("Reality")]
        public async Task RealityIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            var typingMsg = context.MakeMessage();
            typingMsg.Type = ActivityTypes.Typing;
            typingMsg.Text = null;
            await context.PostAsync(typingMsg);

            string reply = "I am virtual";
            await context.PostAsync(reply);
        }

        [LuisIntent("State")]
        public async Task StateIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            var typingMsg = context.MakeMessage();
            typingMsg.Type = ActivityTypes.Typing;
            typingMsg.Text = null;
            await context.PostAsync(typingMsg);
            string reply = "Sorry! I don't have";
            await context.PostAsync(reply);
        }

        [LuisIntent("Time")]
        public async Task TimeIntent(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            var typingMsg = context.MakeMessage();
            typingMsg.Type = ActivityTypes.Typing;
            typingMsg.Text = null;
            await context.PostAsync(typingMsg);

            string reply = $"Time is {DateTime.Now.ToString("dd-MMM-yyyy hh:mm")} and day is {DateTime.Now.Day}";
            await context.PostAsync(reply);
        }

        private Attachment GetCard(string title)
        {
            #region Hero Card
            //string imageUrl = $"https://dummyimage.com/600x300/f0dff0/232540.jpg&text={title}";
            //string docsUrl = "https://docs.microsoft.com/en-us/";
            //var heroCard = new HeroCard()
            //{
            //    Title = title,
            //    Subtitle = "Subtitle of a card",
            //    Text = "Some descriptive text that will display on card",
            //    Images = new List<CardImage>
            //    {
            //        new CardImage(imageUrl)
            //    },
            //    Buttons = new List<CardAction>
            //    {

            //        new CardAction(ActionTypes.OpenUrl,"Open Docs",value: docsUrl),
            //        new CardAction(ActionTypes.OpenUrl,"View Image",value: imageUrl)
            //    }
            //};
            #endregion

            #region Thumbnail card
            //string imageUrl = $"https://dummyimage.com/600x300/f0dff0/232540.jpg&text={title}";
            //string docsUrl = "https://docs.microsoft.com/en-us/";
            //var heroCard = new ThumbnailCard()
            //{
            //    Title = title,
            //    Subtitle = "Subtitle of a card",
            //    Text = "Some descriptive text that will display on card",
            //    Images = new List<CardImage>
            //    {
            //        new CardImage(imageUrl)
            //    },
            //    Buttons = new List<CardAction>
            //    {

            //        new CardAction(ActionTypes.OpenUrl,"Open Docs",value: docsUrl),
            //        new CardAction(ActionTypes.OpenUrl,"View Image",value: imageUrl)
            //    }
            //};
            #endregion

            Dictionary<string, string> imageUrls = new Dictionary<string, string>
                    {
                        {"audio","https://www.google.com/logos/doodles/2019/vikram-sarabhais-100th-birthday-4695275183538176-l.png"},
                        {"Audio","https://www.google.com/logos/doodles/2019/vikram-sarabhais-100th-birthday-4695275183538176-l.png"},
                        {"Video","https://www.google.com/logos/doodles/2019/vikram-sarabhais-100th-birthday-4695275183538176-l.png"},
                        {"Animation","https://www.google.com/logos/doodles/2019/vikram-sarabhais-100th-birthday-4695275183538176-l.png" }
                    };

            Dictionary<string, string> mediaUrls = new Dictionary<string, string>
                     {
                         {"audio","https://file-examples.com/wp-content/uploads/2017/11/file_example_WAV_1MG.wav"},
                         {"Audio","https://file-examples.com/wp-content/uploads/2017/11/file_example_WAV_1MG.wav"},
                         {"Video","http://clips.vorwaerts-gmbh.de/VfE_html5.mp4"},
                         {"Animation","https://mir-s3-cdn-cf.behance.net/project_modules/disp/6c4a7769916383.5b940de055aba.gif" }
                     };

            #region Audio URL
            string[] resultArray = title.ToString().Split(';');// ConfigurationManager.AppSettings["HeroCardValue"].ToString().Split(';');
            int lengthA = resultArray.Length;
            string imageUrl = imageUrls[resultArray[3]];
            string mediaUrl = mediaUrls[resultArray[4]];
            var heroCard = new AudioCard()
            {
                Title = resultArray[0],
                Subtitle = resultArray[1],
                Text = resultArray[2],
                Image = new ThumbnailUrl
                {
                    Url = imageUrl
                },
                Autoloop = true,
                Autostart = true,
                Media = new List<MediaUrl>
                        {
                         new MediaUrl(mediaUrl)
                        },
                Buttons = new List<CardAction>
                      {
                          new CardAction(ActionTypes.OpenUrl, "View media file", value: mediaUrl)
                      }
            };

            #endregion

            #region Video URL
            //string imageUrl = imageUrls[title];
            //string mediaUrl = mediaUrls[title];
            //var heroCard = new VideoCard()
            //{
            //    Title = title,
            //    Subtitle = "Subtitle of a card",
            //    Text = "Some descriptive text that will display on card",
            //    Image = new ThumbnailUrl
            //    {
            //        Url = imageUrl
            //    },
            //    Autoloop = true,
            //    Autostart = true,
            //    Media = new List<MediaUrl>
            //    {
            //     new MediaUrl(mediaUrl)
            //    },
            //    Buttons = new List<CardAction>
            //    {
            //        new CardAction(ActionTypes.OpenUrl, "View media file", value: mediaUrl)
            //    }
            //};
            #endregion

            #region Animation URL
            //string imageUrl = imageUrls[title];
            //string mediaUrl = mediaUrls[title];
            //var heroCard = new AnimationCard()
            //{
            //    Title = title,
            //    Subtitle = "Subtitle of a card",
            //    Text = "Some descriptive text that will display on card",
            //    Image = new ThumbnailUrl
            //    {
            //        Url = imageUrl
            //    },
            //    Autoloop = true,
            //    Autostart = true,
            //    Media = new List<MediaUrl>
            //    {
            //     new MediaUrl(mediaUrl)
            //    },
            //    Buttons = new List<CardAction>
            //    {
            //        new CardAction(ActionTypes.OpenUrl, "View media file", value: mediaUrl)
            //    }
            //};
            #endregion

            return heroCard.ToAttachment();
        }

        private static List<HRResultList> FetchUserFAQ(string valueString)
        {
            try
            {
                List<HRResultList> myarray = new List<HRResultList>();
                using (SP.ClientContext context = new SP.ClientContext("https://Evolvous.sharepoint.com"))
                {
                    SecureString securePassword = GetSecureString("6.F43H7.CwuBuXA");
                    //context.Credentials = new SP.SharePointOnlineCredentials("admin@supertechinfo.onmicrosoft.com", securePassword);
                    context.Credentials = new SP.SharePointOnlineCredentials("dev1@evolvous.com", securePassword);

                    SP.List list = context.Web.Lists.GetByTitle("FAQs");

                    var query = new SP.CamlQuery() { ViewXml = $"<View><Query><Where><Contains><FieldRef Name='Keywords' /><Value Type='Text'>{valueString}</Value></Contains></Where></Query></View>" };

                    SP.ListItemCollection items = list.GetItems(query);

                    context.Load(items);
                    context.ExecuteQuery();
                    //result = $"Total Count: {items.Count}";
                    int i = 0;

                    foreach (SP.ListItem itm in items)
                    {
                        myarray.Add(new HRResultList() { ResultItem = $"{itm["AnswerResult"]}", KeywordItem = $"{itm["Keywords"]}" });
                        i++;
                    }
                    if (i == 0)
                    {
                        myarray.Add(new HRResultList() { ResultItem = "Sorry, I was not able to find any results. Let me look online...", KeywordItem = "Sorry no answer in FAQ" });
                    }
                    return myarray;
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        private static List<HRResultList> FetchHRFAQ(string valueString)
        {
            try
            {
                List<HRResultList> myarray = new List<HRResultList>();

                using (SP.ClientContext context = new SP.ClientContext("https://Evolvous.sharepoint.com"))
                {
                    SecureString securePassword = GetSecureString("6.F43H7.CwuBuXA");
                    //context.Credentials = new SP.SharePointOnlineCredentials("admin@supertechinfo.onmicrosoft.com", securePassword);
                    context.Credentials = new SP.SharePointOnlineCredentials("dev1@evolvous.com", securePassword);

                    SP.List list = context.Web.Lists.GetByTitle("FAQs");

                    var query = new SP.CamlQuery() { ViewXml = $"<View><Query><Where><Contains><FieldRef Name='Keywords' /><Value Type='Text'>{valueString}</Value></Contains></Where></Query></View>" };

                    SP.ListItemCollection items = list.GetItems(query);

                    context.Load(items);
                    context.ExecuteQuery();
                    //result = $"Total Count: {items.Count}";
                    int i = 0;
                    foreach (SP.ListItem itm in items)
                    {
                        myarray.Add(new HRResultList() { ResultItem = $"{itm["AnswerResult"]}", KeywordItem = $"{itm["Keywords"]}" });
                        i++;
                    }
                    if (i == 0)
                    {
                        myarray.Add(new HRResultList() { ResultItem = "Sorry no answer in FAQ", KeywordItem = "Sorry no answer in FAQ" });
                    }
                    return myarray;
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        private List<HRResultList> DCAFAQQuestionDataWithQuery(string valueString)
        {
            try
            {
                List<HRResultList> myarray = new List<HRResultList>();

                using (SP.ClientContext context = new SP.ClientContext("https://Evolvous.sharepoint.com"))
                {
                    SecureString securePassword = GetSecureString("6.F43H7.CwuBuXA");
                    //context.Credentials = new SP.SharePointOnlineCredentials("admin@supertechinfo.onmicrosoft.com", securePassword);
                    context.Credentials = new SP.SharePointOnlineCredentials("dev1@evolvous.com", securePassword);

                    SP.List list = context.Web.Lists.GetByTitle("DCAFAQQuestion");

                    var query = new SP.CamlQuery()
                    {
                        ViewXml = $"<View><Query><Where><Eq><FieldRef Name='Title' />" +
                        $"<Value Type='Text'>{valueString}</Value></Eq></Where></Query></View>"
                    };

                    SP.ListItemCollection items = list.GetItems(query);

                    context.Load(items);
                    context.ExecuteQuery();
                    //result = $"Total Count: {items.Count}";
                    int i = 0;
                    foreach (SP.ListItem itm in items)
                    {
                        myarray.Add(new HRResultList() { ResultItem = $"{itm["Answers"]}", KeywordItem = $"{itm["Keyword"]}" });
                        i++;
                    }

                    return myarray;
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        private List<HRResultList> DCAFAQQuestionQuerySuggestion(string valueString)
        {
            try
            {
                List<HRResultList> myarray = new List<HRResultList>();

                using (SP.ClientContext context = new SP.ClientContext("https://Evolvous.sharepoint.com"))
                {
                    SecureString securePassword = GetSecureString("6.F43H7.CwuBuXA");
                    //context.Credentials = new SP.SharePointOnlineCredentials("admin@supertechinfo.onmicrosoft.com", securePassword);
                    context.Credentials = new SP.SharePointOnlineCredentials("dev1@evolvous.com", securePassword);

                    SP.List list = context.Web.Lists.GetByTitle("DCAFAQQuestion");

                    var query = new SP.CamlQuery() { ViewXml = $"<View><Query><Where><Contains><FieldRef Name='Title' /><Value Type='Text'>{valueString}</Value></Contains></Where></Query></View>" };

                    SP.ListItemCollection items = list.GetItems(query);

                    context.Load(items);
                    context.ExecuteQuery();
                    //result = $"Total Count: {items.Count}";
                    int i = 0;
                    foreach (SP.ListItem itm in items)
                    {
                        myarray.Add(new HRResultList() { ResultItem = $"{itm["AnswerResult"]}", KeywordItem = $"{itm["Keywords"]}" });
                        i++;
                    }
                    if (i == 0)
                    {
                        myarray.Add(new HRResultList() { ResultItem = "Sorry no answer in FAQ", KeywordItem = "Sorry no answer in FAQ" });
                    }
                    return myarray;
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }
        public string FetchUserLeave(string userName)
        {
            string userNameResult = string.Empty;
            try
            {
                string result = string.Empty;
                using (SP.ClientContext context = new SP.ClientContext("https://Evolvous.sharepoint.com/sites/bot"))
                {
                    SecureString securePassword = GetSecureString("6.F43H7.CwuBuXA");
                    //context.Credentials = new SP.SharePointOnlineCredentials("admin@supertechinfo.onmicrosoft.com", securePassword);
                    context.Credentials = new SP.SharePointOnlineCredentials("dev1@evolvous.com", securePassword);

                    SP.List list = context.Web.Lists.GetByTitle("PendingLeaves");
                    SP.User user = context.Web.CurrentUser;
                    try
                    {
                        userNameResult = user.Id.ToString();
                    }
                    catch (Exception Ex)
                    {
                        userNameResult = userName.ToString();
                    }
                    SP.CamlQuery query = new SP.CamlQuery();
                    query.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='Title' /><Value Type='Text'>{userName}</Value></Eq></Where></Query></View>";
                    //query.ViewXml = $"<View><Query><Where><Eq><FieldRef Name='Title' /><Value Type='Text'>User</Value></Eq></Where></Query></View>";

                    SP.ListItemCollection items = list.GetItems(query);
                    context.Load(items);
                    context.ExecuteQuery();
                    result = $"Total Count: {items.Count} \n";

                    foreach (SP.ListItem itm in items)
                    {
                        result += $"\n\n Earn Leaves: {itm["EarnLeaves"]}";
                        result += $"\n\n Casual Leaves: {itm["CasualLeaves"]}";
                        result += $"\n\n Sick Leaves: {itm["SickLeaves"]}";
                        result += $"\n\n Paternity Leaves: {itm["PaternityLeaves"]} ";
                    }
                    return result;
                }
            }
            catch (Exception Ex)
            {

                throw Ex;
            }
        }

        public static SecureString GetSecureString(string userPassword)
        {
            SecureString securePassword = new SecureString();

            foreach (char c in userPassword.ToCharArray())
            {
                securePassword.AppendChar(c);
            }

            return securePassword;
        }

        private static Attachment CreateAttachmentFromFAQ(FAQArticle fAQArticle)
        {

            var heroCard = new HeroCard()
            {
                Title = fAQArticle.Title,
                Subtitle = fAQArticle.Author,
                Text = fAQArticle.Description,
                Images = new List<CardImage>
                {
                   new CardImage(fAQArticle.URLToImage),
                },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.OpenUrl, "View media file", value: fAQArticle.URL)
                }
            };

            return heroCard.ToAttachment();
        }

        public static IMessageActivity GetFAQHRArticle(IDialogContext dialogContext, List<HRResultList> resultFAQ, string searchResult)
        {
            FAQArticle fAQArticle;
            var message = dialogContext.MakeMessage();
            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            // string[] searchResultArray = searchResult.Split(' ');
            foreach (var item in resultFAQ)
            {
                //foreach (var itemKey in searchResultArray)
                //{
                if (item.KeywordItem.ToLower().Contains(searchResult.ToLower()) == true)
                {
                    string[] resultArray = item.ResultItem.ToString().Split(';'); // ConfigurationManager.AppSettings["HeroCardValue"].ToString().Split(';');
                    int lengthA = resultArray.Length;
                    if (lengthA.Equals(6))
                    {
                        fAQArticle = new FAQArticle()
                        {
                            Title = resultArray[0].ToString(),
                            Author = resultArray[1].ToString(),
                            Description = resultArray[2].ToString(),
                            URLToImage = resultArray[3].ToString(),
                            URL = resultArray[4].ToString()
                        };
                        message.Attachments.Add(CreateAttachmentFromFAQ(fAQArticle));
                    }
                    else
                    {

                        message = null;
                    }
                }
                //}
            }
            return message;
        }

        private async Task ResumeAfterEnteringQuery(IDialogContext context, IAwaitable<string> result)
        {
            string query = (await result) as string;
            switch (searchType)
            {
                case obtainDescriptionFromImage:
                    {
                        await BingHelper.SearchWebAsync(context, BING_KEY, query);
                        break;
                    }
            }
            //context.Wait(MessageReceivedAsync);
        }

        private async Task ResumeDataFromOnlineSelectedQuestion(IDialogContext context, IAwaitable<ConfirmSelection> result)
        {
            var message = await result;
            //var typingMsg = context.MakeMessage();
            //typingMsg.Type = ActivityTypes.Typing;
            //typingMsg.Text = null;
            //await context.PostAsync(typingMsg);


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
                await BingHelper.SearchWebAsync(context, BING_KEY, HoldUserQuestion);
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

        private async Task ContactUserSupportTeam(IDialogContext context, IAwaitable<ConfirmSelection> result)
        {
            //var typingMsg = context.MakeMessage();
            //typingMsg.Type = ActivityTypes.Typing;
            //typingMsg.Text = null;
            //await context.PostAsync(typingMsg);

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

        private async Task ResumeDataFromOnlineUserSatisfiedAsync(IDialogContext context, IAwaitable<ConfirmSelection> result)
        {
            //var typingMsg = context.MakeMessage();
            //typingMsg.Type = ActivityTypes.Typing;
            //typingMsg.Text = null;
            //await context.PostAsync(typingMsg);

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

    public class FAQArticle
    {
        public FAQSource source { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public string URLToImage { get; set; }
        public DateTime? PublishAt { get; set; }

    }

    public class FAQResponse
    {
        public string status { get; set; }
        public int TotalRecords { get; set; }
        public List<FAQArticle> Articles { get; set; }

    }

    public class FAQSource
    {

        public string ID { get; set; }
        public string Name { get; set; }

    }

    public class HRResultList
    {
        public string ResultItem { get; set; }
        public string KeywordItem { get; set; }
    }
}