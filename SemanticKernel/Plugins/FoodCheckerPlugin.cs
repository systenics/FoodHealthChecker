using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoodHealthChecker.SemanticKernel.Plugins
{
    public class FoodCheckerPlugin
    {
        static readonly PromptExecutionSettings s_settings = new OpenAIPromptExecutionSettings()
        {
            Temperature = 0.2,
            TopP = 0.5,
            MaxTokens = 400
        };

        private readonly KernelFunction _checkFoodHealthFunction;

        [Description("The food health checker analyze the given image and check if they are healthy or not")]
        public FoodCheckerPlugin()
        {
            _checkFoodHealthFunction = KernelFunctionFactory.CreateFromPrompt(
                FoodCheckerTemplates.CheckFoodHealth,
                description: "Given the list of ingredients classfiy whether the Nutri-score and Nova-group along with the reasons in simpler terms\r\n",
                executionSettings: s_settings);
        }

        [KernelFunction, Description("Given the list of ingredients classfiy whether the Nutri-score and Nova-group along with the reasons in simpler term")]
        public async IAsyncEnumerable<string> CheckFoodHealthAsync([Description("List of ingredients and nutritional values")] string input, Kernel kernel, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var result in _checkFoodHealthFunction.InvokeStreamingAsync(kernel, new(s_settings) { { "input", input } }, cancellationToken))
            {
                var generatedText = result?.ToString() ?? string.Empty;
                Console.Write(generatedText);
                yield return generatedText;
            }
        }

        [KernelFunction, Description("Get the ingredients and nutritional values from the given food product images")]
        public async IAsyncEnumerable<string> GetIngredientsAsync([Description("Food ingredients image url")] string input, Kernel kernel, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chat = new ChatHistory(FoodCheckerTemplates.SystemMessage);
            chat.AddUserMessage(new ChatMessageContentItemCollection
            {
                new TextContent(FoodCheckerTemplates.GetIngredients),
                new ImageContent(new Uri(input))
            }); 
            await foreach (var result in chatService.GetStreamingChatMessageContentsAsync(chat, s_settings, kernel, cancellationToken))
            {
                var generatedText = result?.ToString() ?? string.Empty;
                Console.Write(generatedText);
                yield return generatedText;
            }
        }
    }
    public static class FoodCheckerTemplates
    {
        public const string SystemMessage = @"You are a AI Food expert";

        public const string CheckFoodHealth =
@"
[Instruction]    
Given the list of ingredients for a food product give it a Rating from Very Unhealthy to very Healthy. Also give the reasoning in ELI5 format using less than 3 sentences in the below response format. 
Also list any cancer causing or harmful substances if present.
[Ingredients]
{{$input}}
[RESPONSE]
**Predicted Rating**
**Reasoning**
";

        public const string GetIngredients =
@"
[Instruction]    
Get the ingredients and nutritional values from the given food product images as briefly as possible in the given format else respond with <|ERROR|> if nothing if found
[RESPONSE]
**Ingredients**
**Nutritional Values**";

    }
}
