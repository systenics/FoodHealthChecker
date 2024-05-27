using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

namespace FoodHealthChecker.SemanticKernel.Plugins
{
    public class FoodCheckerFilterPlugin
    {
        private readonly KernelFunction _verifyFoodRelatedImages;
        private readonly KernelFunction _verifyFoodIngredients;
        [Description("The food health checker analyze the given image and check if they are healthy or not")]
        public FoodCheckerFilterPlugin()
        {
            PromptExecutionSettings settings = new OpenAIPromptExecutionSettings()
            {
                Temperature = 0.0,
                TopP = 0.9,
                MaxTokens = 10
            };

            _verifyFoodRelatedImages = KernelFunctionFactory.CreateFromPrompt(
                FoodCheckerFilterPluginTemplates.VerifyFoodRelatedImages,
                description: "",
                executionSettings: settings);

            _verifyFoodIngredients = KernelFunctionFactory.CreateFromPrompt(
                FoodCheckerFilterPluginTemplates.VerifyFoodIngredients,
                description: "",
                executionSettings: settings);
        }


    }

    public class FoodCheckerFilterPluginTemplates
    {
        public const string VerifyFoodIngredients = @"TODO";
        public const string VerifyFoodRelatedImages = @"TODO";
    }
}
