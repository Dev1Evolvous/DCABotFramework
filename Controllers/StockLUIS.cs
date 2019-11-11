using System.Collections.Generic;
using System.Collections;
namespace Microsoft.Bot.Sample.QnABot
{
    internal class StockLUIS
    {
        //public string query { get; set; }
        //public List<topScoringIntent> topScoringIntent { get; set; }
        //public List<intents> intents { get; set; }
        public List<entities> Entities { get; set; }
        //public List<sentimentAnalysis> SentimentAnalysis { get; set; }
        //public string[] intents = { "", "" };
        //public string[] entities = { "", "", "", "", "", "" };
    }

    internal class SentimentLUIS
    {
        //public string query { get; set; }
        //public List<topScoringIntent> topScoringIntent { get; set; }
        //public List<intents> intents { get; set; }
        public Dictionary<string, string> SentimentAnalysis { get; set; }
        //public string[] intents = { "", "" };
        //public string[] entities = { "", "", "", "", "", "" };
    }
    class intents
    {
        public string intent { get; set; }
        public double? score { get; set; }
    }

    class topScoringIntent
    {
        public string intent { get; set; }
        public double? score { get; set; }
    }
    class entities
    {

        public string entity { get; set; }
        public string type { get; set; }
        public int? startIndex { get; set; }
        public int? endIndex { get; set; }
        public double? score { get; set; }
        public string role { get; set; }
    }

    class sentimentAnalysis
    {
        public string label { get; set; }
        public double? score { get; set; }
    }

    //    {
    //  "query": "explain SharePoint",
    //  "topScoringIntent": {
    //    "intent": "FAQ",
    //    "score": 0.9241065
    //  },
    //  "intents": [
    //    {
    //      "intent": "FAQ",
    //      "score": 0.9241065
    //    },
    //    {
    //      "intent": "HRPolicy",
    //      "score": 0.0409002528
    //    },
    //    {
    //      "intent": "LeaveBalance",
    //      "score": 0.0143305548
    //    },
    //    {
    //      "intent": "None",
    //      "score": 0.0138455573
    //    },
    //    {
    //      "intent": "LeaveQuery",
    //      "score": 0.00106703944
    //    }
    //  ],
    //  "entities": [
    //    {
    //      "entity": "sharepoint",
    //      "type": "FAQ",
    //      "startIndex": 8,
    //      "endIndex": 17,
    //      "score": 0.974862933,
    //      "role": "SharePoint"
    //    }
    //  ]
    //}
}