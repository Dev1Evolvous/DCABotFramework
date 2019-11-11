using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Azure.CognitiveServices.Search.WebSearch;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.QnABot
{
    public class BingHelper
    {
        public async static Task SearchWebAsync(IDialogContext context, string key, string query)
        {
            IWebSearchClient client = new WebSearchClient(new ApiKeyServiceClientCredentials(key));
            var result = await client.Web.SearchAsync(query: query, 
                count: 3, 
                safeSearch: SafeSearch.Strict);

            if (result?.WebPages?.Value?.Count > 0)
            {
                await context.PostAsync($"Search result for **{query}**");
                foreach (var item in result.WebPages.Value)
                {
                    HeroCard card = new HeroCard
                    {
                        Title = item.Name,
                        Text = item.Snippet,
                        Buttons = new List<CardAction>
                        {
                            new CardAction(ActionTypes.OpenUrl, "Open Page", value:item.Url)
                        }
                    };
                    var message = context.MakeMessage();
                    message.Attachments.Add(card.ToAttachment());
                    await context.PostAsync(message);
                }
            }
        }
    }
}