using FoodHealthChecker.Models;
using FoodHealthChecker.Options;
using FoodHealthChecker.SemanticKernel.Plugins;
using Microsoft.SemanticKernel;

namespace FoodHealthChecker
{
    /// <summary>
    /// Service for checking food health.
    /// </summary>
    public class FoodCheckerService
    {
        private Kernel _kernel;
        private bool isValid = false;
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
                isValid = true;
            }
            else if (oaiOptions != null && oaiOptions.isValid())
            {
                kernelBuilder.AddOpenAIChatCompletion(oaiOptions.ModelID, oaiOptions.ApiKey);
                isValid = true;
            }
            var kernel = kernelBuilder.Build();

            kernel.Plugins.AddFromObject(foodCheckerPlugin);
            _kernel = kernel;
        }

        /// <summary>
        /// Checks if the service is valid.
        /// </summary>
        /// <returns>True if the service is valid, false otherwise.</returns>
        public bool IsValid() => isValid;

        /// <summary>
        /// Checks the health of the food asynchronously.
        /// </summary>
        /// <param name="ingredientResponse">ingredients found</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The health check result.</returns>
        public IAsyncEnumerable<string> CheckFoodHealthAsync(string ingredientResponse, CancellationToken cancellationToken = default)
        {
            return _foodCheckerPlugin.CheckFoodHealthAsync(ingredientResponse, _kernel, cancellationToken);
        }

        /// <summary>
        /// Gets the ingredients asynchronously for the given hosted image url
        /// </summary>
        /// <param name="hostedImageUrl">The image URL.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>return the ingredients text present in the given imageUrl</returns>
        public IAsyncEnumerable<string> GetIngredirentsAsync(string hostedImageUrl, CancellationToken cancellationToken = default)
        {
            return _foodCheckerPlugin.GetIngredientsAsync(hostedImageUrl, _kernel, cancellationToken);
        }

        /// <summary>
        /// Gets the ingredients asynchronously for the given imageData.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="fileName">image File Name</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>return the ingredients text present in the given image</returns>
        public IAsyncEnumerable<string> GetIngredirentsAsync(ReadOnlyMemory<byte> imageData, string fileName, CancellationToken cancellationToken = default)
        {

            var imageDataUrl = new ImageContent(imageData) { MimeType = GetMimeType(fileName) }.ToString();

            return _foodCheckerPlugin.GetIngredientsAsync(imageDataUrl, _kernel, cancellationToken);
        }

        /// <summary>
        /// Updates the kernel using temporary Config set through UI
        /// </summary>
        /// <param name="config">The temporary configuration passed</param>
        public void UpdateTemporaryKernel(TemporaryConfig config)
        {
            var kernelBuilder = Kernel.CreateBuilder();

            if (config.IsAzureOpenAIConfigValid())
            {
                kernelBuilder.AddAzureOpenAIChatCompletion(config.AzureOpenAI_DeploymentName, config.AzureOpenAI_Endpoint, config.AzureOpenAI_ApiKey);
                isValid = true;
            }
            else if (config.IsOpenAIConfigValid())
            {
                kernelBuilder.AddOpenAIChatCompletion(config.OpenAI_ModelId, config.OpenAI_ApiKey);
                isValid = true;
            }
            var kernel = kernelBuilder.Build();
            kernel.Plugins.AddFromObject(_foodCheckerPlugin);
            _kernel = kernel;
        }


        /// <summary>
        /// Gets the MIME type of the file.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The MIME type.</returns>
        /// <exception cref="NotSupportedException">Thrown when the image format is unsupported.</exception>
        private static string GetMimeType(string fileName)
        {
            return Path.GetExtension(fileName) switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tiff" => "image/tiff",
                ".ico" => "image/x-icon",
                ".svg" => "image/svg+xml",
                _ => throw new NotSupportedException("Unsupported image format.")
            };
        }
    }
}
