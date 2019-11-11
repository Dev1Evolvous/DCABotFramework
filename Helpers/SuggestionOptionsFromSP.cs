using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using SP = Microsoft.SharePoint.Client;
namespace Microsoft.Bot.Sample.QnABot
{
    public class SuggestionOptionsFromSP
    {
        public static List<SuggestionQuestion> GetAllMenuOptions()
        {
            return new List<SuggestionQuestion>() {
                new SuggestionQuestion() {
                    QuestionOption = "CrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChicken"
                },
                new SuggestionQuestion() {
                    QuestionOption = "CrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChicken"
                },
                new SuggestionQuestion() {
                    QuestionOption = "ChickenDrumStickChickenDrumStickChickenDrumStickChickenDrumStickChickenDrumStickChickenDrumStickChickenDrumStickChickenDrumStickChickenDrumStickChickenDrumStickChickenDrumStickChickenDrumStick"
                }

        };
        }

        public static List<string> GetAllQuestionOptions()
        {
            return new List<string>() {
                 "CrispyChickenCrispyChickenCrispy ChickenCrispyChicken CrispyChickenCrispy ChickenCrispyChicken CrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChickenCrispyChicken"
                ,
                 "CrispyChickenCrispyChickenCrispy ChickenCrispy ChickenCrispyChicken CrispyChicken CrispyChickenCrispy ChickenCrispyChickenCrispyChickenCrispyChickenCrispyChicken"
                ,
                 "ChickenDrumStickChicke nDrumStickChicken DrumStickChicken DrumStickChicken DrumStickChickenDrumStick ChickenDrumStickChickenDrumStickChickenDrumStickChickenDrumStickChickenDrumStickChickenDrumStick"
                };
        }


        public  List<string> GetAllQuestionSharePointOptions(string valueString)
        {
            try
            {
                List<string> myarray = new List<string>();

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
                        myarray.Add(itm["Title"].ToString());
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

        public static SecureString GetSecureString(string userPassword)
        {
            SecureString securePassword = new SecureString();

            foreach (char c in userPassword.ToCharArray())
            {
                securePassword.AppendChar(c);
            }

            return securePassword;
        }
    }
}