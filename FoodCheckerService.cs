using FoodHealthChecker.Options;
using FoodHealthChecker.SemanticKernel.Filters;
using FoodHealthChecker.SemanticKernel.Plugins;
using Microsoft.SemanticKernel;

namespace FoodHealthChecker
{
    public class FoodCheckerService
    {
        private readonly Kernel _kernel;
        private readonly FoodCheckerPlugin _foodCheckerPlugin;
        public FoodCheckerService(IConfiguration config, FoodCheckerPlugin foodCheckerPlugin)
        {
            _foodCheckerPlugin = foodCheckerPlugin;
            var azureOptions = config.GetSection("AzureOpenAI").Get<AzureOpenAIOptions>();
            var oaiOptions = config.GetSection("OpenAI").Get<OpenAIOptions>();
            var kernelBuilder = Kernel.CreateBuilder();

            kernelBuilder.Services.AddLogging(config => { config.AddConsole(); config.SetMinimumLevel(LogLevel.Information); });
            if (azureOptions != null && azureOptions.isValid())
            {
                kernelBuilder.AddAzureOpenAIChatCompletion(azureOptions.DeploymentName, azureOptions.Endpoint, azureOptions.ApiKey);
            }
            else if (oaiOptions != null && oaiOptions.isValid())
            {
                kernelBuilder.AddOpenAIChatCompletion(oaiOptions.ModelID, oaiOptions.ApiKey);
            }
            else
            {
                throw new ArgumentException("Missing AI service configuration");
            }
            var kernel = kernelBuilder.Build();
            //TODO - implement
            //kernel.PromptRenderFilters.Add(new FoodCheckPromptRenderFilter());
            //kernel.FunctionInvocationFilters.Add(new FoodCheckFunctionFilter());
            kernel.Plugins.AddFromObject(foodCheckerPlugin);
            _kernel = kernel;
        }
        public IAsyncEnumerable<string> CheckFoodHealthAsync(string ingredientResponse, CancellationToken cancellationToken = default)
        {
            return _foodCheckerPlugin.CheckFoodHealthAsync(ingredientResponse, _kernel, cancellationToken);
        }

        public IAsyncEnumerable<string> GetIngredirentsAsync(string imageDataUrl, CancellationToken cancellationToken = default)
        {
            return _foodCheckerPlugin.GetIngredientsAsync(imageDataUrl, _kernel, cancellationToken);
        }
    }
}
