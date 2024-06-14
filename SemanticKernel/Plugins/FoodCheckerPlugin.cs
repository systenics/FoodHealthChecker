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

        public FoodCheckerPlugin()
        {
            _checkFoodHealthFunction = KernelFunctionFactory.CreateFromPrompt(
                FoodCheckerTemplates.CheckFoodHealth,
                description: "Given the list of ingredients for a food product give it a Rating from Very Unhealthy to very Healthy.",
                executionSettings: s_settings);
        }

        /// <summary>
        /// Checks the healthiness of food based on the given list of ingredients and nutritional values.
        /// </summary>
        /// <param name="input">List of ingredients and nutritional values</param>
        /// <param name="kernel">Kernel for the function</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>Generated text about the healthiness of the food</returns>
        [KernelFunction, Description("Given the list of ingredients for a food product give it a Rating from Very Unhealthy to very Healthy.")]
        public async IAsyncEnumerable<string> CheckFoodHealthAsync([Description("List of ingredients and nutritional values")] string input, Kernel kernel, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var result in _checkFoodHealthFunction.InvokeStreamingAsync(kernel, new(s_settings) { { "input", input } }, cancellationToken))
            {
                var generatedText = result?.ToString() ?? string.Empty;
                Console.Write(generatedText);
                yield return generatedText;
            }
        }

        /// <summary>
        /// Gets the ingredients and nutritional values from the given food product images.
        /// </summary>
        /// <param name="input">URL of the food ingredients image</param>
        /// <param name="kernel">Kernel for the function</param>
        /// <param name="cancellationToken">the cancellation token</param>
        /// <returns>Generated text about the ingredients and nutritional values of the food</returns>
        [KernelFunction, Description("Get the ingredients and nutritional values from the given food product images")]
        public async IAsyncEnumerable<string> GetIngredientsAsync([Description("List of Food ingredients image url")] List<string> input, Kernel kernel, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            ChatHistory chat = new(FoodCheckerTemplates.SystemMessage);
            var msgCollection = new ChatMessageContentItemCollection
            {
                new TextContent(FoodCheckerTemplates.GetIngredients),
            };
            foreach(var imgurl in input)
            {
                msgCollection.Add(new ImageContent(new Uri(imgurl, UriKind.Absolute)));
            }
            chat.AddUserMessage(msgCollection);
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
        public const string SystemMessage = @"You are an AI Food expert with extensive knowledge in Nutrion";

        public const string GetIngredients =
@"
[Instruction]    
Get the ingredients and nutritional values in english from the given food product images as briefly as possible in the given format else respond with <|ERROR|> if nothing if found
[RESPONSE]
**Ingredients**
**Nutritional Values**";
        public const string CheckFoodHealth =
@"
[Instruction]    
Given the list of ingredients for a food product give it a Rating from Very Unhealthy to very Healthy. Also give the reasoning in ELI5 format using less than 3 sentences in the below response format. 
Also list any allergens, cancer causing or harmful substances if present along with exact reason.
[Ingredients]
{{$input}}
[RESPONSE]
**Predicted Rating**
**Reasoning**
**Harmful substances**
";

    }
}
