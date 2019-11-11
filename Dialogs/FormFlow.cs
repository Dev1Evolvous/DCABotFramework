using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Connector;


namespace Microsoft.Bot.Sample.QnABot
{
    #region



    public enum LaptopBrand
    {
        Lenvo, HP, Dell, Apple, Acer, Microsoft
    }
    //public enum LaptopType
    //{
    //    Laptop, Gaming, Ultrabook, Surface, Netbook
    //}
    public enum SatisfiedRepport
    {
        [Describe("Yes")]
        satisfied = 0,
        [Describe("No")]
        Nosatisfied = 1
    }
    public enum LaptopType
    {
        [Describe("Yes")]
        satisfied = 0,
        [Describe("No")]
        Nosatisfied = 1
    }
    public enum LaptopProcessor
    {
        [Describe("Intel core I3")]
        [Terms("I3")]
        IntelcoreI3,
        [Describe("Intel core I5")]
        IntelcoreI5,
        [Describe("Intel core I7")]
        IntelcoreI7,
        [Describe("Intel core I9")]
        IntelcoreI9,
        [Describe("AMS Dual Core")]
        [Terms("amd")]
        AMSDualCore,
        [Describe("Intel Core M")]
        IntelCoreM
    }

    public enum LaptopOperatingSystem
    {
        Window8, Window10, MSDos, Linux
    }
    public enum DeliveryOptions
    {
        TakeAway = 1,
        Delivery
    }
    #endregion
    [Serializable]
    public class FormFlow
    {
        public List<SuggestionQuestion> MenuItems
        {
            get;
            set;
        }
        
        public string UserName
        {
            get;
            set;
        }
        public string Phone
        {
            get;
            set;
        }
        public string address
        {
            get;
            set;
        }

        //public LaptopType? LaptopType;
        //[Optional]
        //[Describe(
        //    description: "Company",
        //    title: "Laptop Brand",
        //    subTitle: "There are serveral other brand present but we are not selling those.")]
        //public LaptopBrand? Brand;
        //public LaptopProcessor? Process;
        
        //[Template(TemplateUsage.EnumSelectOne,
        //    "Select preferrable {&}:{||}",
        //    ChoiceStyle = ChoiceStyleOptions.PerLine)]
        //public LaptopOperatingSystem? OperatingSystem;
        //public bool? RequiredTouch;

        //[Numeric(2, 16)]
        //[Describe(description: "Minimum capacity of RAM")]

        //public int MinimumRamSize;
        //[Pattern(@"^[789]\d{9}$")]
        //public string userMobileNumber;
     
        public SatisfiedRepport? satisfied;
        public static IForm<FormFlow> GetForm()
        {
            var menuItems = SuggestionOptionsFromSP.GetAllMenuOptions();
            var builder = new FormBuilder<FormFlow>();

            OnCompletionAsyncDelegate<FormFlow> onFormCompletion = async (context, state) =>
            {
                await context.PostAsync(@"We have responded your requirment.Try with other query.");
            };
            //return new FormBuilder<FormFlow>()
            //   .Message("Welcome to Laptop suggestion Bot Application")
            //    //.Field(nameof(userMobileNumber))
            //   .Field(nameof(Brand))
            //   .Field(nameof(Process))
            //   .Build();
            //    return new FormBuilder<FormFlow>()

            //       .Field(nameof(pingPong))
            //       .Confirm(async (state) =>
            //       {
            //           int price = 0;
            //           switch (state.Process)
            //           {
            //               case LaptopProcessor.IntelcoreI3: price = 200; break;
            //               case LaptopProcessor.IntelcoreI5: price = 300; break;
            //               case LaptopProcessor.IntelcoreI7: price = 400; break;
            //               case LaptopProcessor.IntelcoreI9: price = 500; break;
            //               case LaptopProcessor.AMSDualCore: price = 250; break;
            //               case LaptopProcessor.IntelCoreM: price = 280; break;
            //           }
            //           return new PromptAttribute($"Minimum price for this processor will be {price}. Is that okay?");
            //       })
            //       .Field(nameof(userMobileNumber),
            //       validate: async (state, response) =>
            //       {
            //           var validation = new ValidateResult { IsValid = true, Value = response };
            //           if ((response as string).Equals("8882263462"))
            //           {
            //               validation.IsValid = false;
            //               validation.Feedback = "8882263462 is not allowed";
            //           }
            //           return validation;
            //       })
            //       .Confirm("You required Laptop with {Process} and Mobile no is {userMobileNumber}")
            //       .OnCompletion(onFormCompletion)
            //       .Build();
            //}

            //return new FormBuilder<FormFlow>()
            //.Confirm("are you satisfied?{||}")
            //.Field("What is SharePoint?")
            //.Build();

            builder.Message("Welcome to bot!")
                .Field(
                new FieldReflector<FormFlow>(
                    nameof(MenuItems))
                .SetType(null)
                .SetDefine((state, field) =>
                 {
                     foreach (var item in menuItems)
                     {
                         field.AddDescription(item, item.QuestionOption)
                             .AddTerms(item, item.QuestionOption);
                     }
                     return Task.FromResult(true);
                 })
                .SetPrompt(new PromptAttribute(" We have suggested questions for you \n {||} \n")
                 {
                    ChoiceStyle = ChoiceStyleOptions.Buttons
                 }))            
                .OnCompletion(async (context, state) => {
                    await context.PostAsync($"Thanks, the task is complete.");
                });
            return builder.Build();

        }

        private static string[] GetStringValue()
        {
            string[] valueName =
            {
                "This",
                "Are, you, confirm",
                "Happy"
            };

            return valueName;
        }
    }
}