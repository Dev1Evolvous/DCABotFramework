using Microsoft.Bot.Sample.QnABot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using SP = Microsoft.SharePoint.Client;

namespace Microsoft.Bot.Sample.QnABot
{
    public  class GetDataFromSP
    {


        private static string FetchUserLeaveBalance(string userName, string leaveType)
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
                        if (leaveType.Equals("Paternity.Leave"))
                        {
                            result += $"\n\n Paternity Leaves: {itm["PaternityLeaves"]} ";

                        }
                        else if (leaveType.Equals("Maternity.Leave"))
                        {
                            result += $"\n\n Maternity Leaves: {itm["MaternityLeaves"]} ";
                        }
                        else if (leaveType.Equals("Casual.Leave"))
                        {
                            result += $"\n\n Casual Leaves: {itm["CasualLeaves"]}";

                        }
                        else if (leaveType.Equals("Sick.Leave"))
                        {
                            result += $"\n\n Sick Leaves: {itm["SickLeaves"]}";
                        }
                        else if (leaveType.Equals("Earn.Leave"))
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

        private static SecureString GetSecureString(string userPassword)
        {
            SecureString securePassword = new SecureString();

            foreach (char c in userPassword.ToCharArray())
            {
                securePassword.AppendChar(c);
            }

            return securePassword;
        }


        public  async Task<List<string>> GetIntentSentimentScore(string activityText)
        {
            string[] result = new string[4];
            List<string> listResult = new List<string>();
            StockLUIS stockLUIS = new StockLUIS();
            SentimentLUIS sentimentLUIS = new SentimentLUIS();
            Dictionary<string, string> sentimentDic = new Dictionary<string, string>();
          
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/402fd33a-ccd4-4f55-8880-d9eb50fc1ac7?verbose=true&timezoneOffset=0&subscription-key=bfffb3860f844883a565c02e8202480f&q=" + activityText;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);
                if (msg.IsSuccessStatusCode)
                {
                    try
                    {
                        var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        stockLUIS = js.Deserialize<StockLUIS>(JsonDataResponse);
                        sentimentLUIS = js.Deserialize<SentimentLUIS>(JsonDataResponse);

                        foreach (var item in stockLUIS.Entities)
                        {
                            listResult.Add(item.entity);
                            listResult.Add(item.score.ToString());
                            break;
                        }
                        foreach (KeyValuePair<string, string> item in sentimentLUIS.SentimentAnalysis)
                        {
                            listResult.Add(item.Key);
                            listResult.Add(item.Value);
                        }


                    }
                    catch (Exception Ex)
                    {
                        result[0] = "Error";
                        result[1] = "Error:" + Ex.Message;
                        result[2] = "Error";
                        result[3] = "Error";
                        return listResult;
                    }
                }
                return listResult;
            }
        }

        
    }

}