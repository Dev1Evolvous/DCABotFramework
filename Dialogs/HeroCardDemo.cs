using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
namespace Microsoft.Bot.Sample.QnABot
{
    [Serializable]
    public class HeroCardDemo : IDialog<object>
    {

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {

            var message = context.MakeMessage();
            var activity = await result;
            message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            message.Attachments.Add(GetCard(activity.Text));
            await context.PostAsync(message);

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
            string imageUrl = imageUrls[title];
            string mediaUrl = mediaUrls[title];
            var heroCard = new AudioCard()
            {
                Title = title,
                Subtitle = "Subtitle of a card",
                Text = "Some descriptive text that will display on card",
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
    }
}