﻿using FoodHealthChecker.Models;
using FoodHealthChecker.Options;
using FoodHealthChecker.SemanticKernel.Plugins;
using Microsoft.SemanticKernel;

namespace FoodHealthChecker
{
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

        public void UpdateTemporaryKernel(TemporaryConfig config)
        {
            var kernelBuilder = Kernel.CreateBuilder();

            if (config.isAzureOpenAIConfigValid())
            {
                kernelBuilder.AddAzureOpenAIChatCompletion(config.AzureOpenAI_DeploymentName, config.AzureOpenAI_Endpoint, config.AzureOpenAI_ApiKey);
                isValid = true;
            }
            else if (config.isOpenAIConfigValid())
            {
                kernelBuilder.AddOpenAIChatCompletion(config.OpenAI_ModelId, config.OpenAI_ApiKey);
                isValid = true;
            }
            var kernel = kernelBuilder.Build();
            kernel.Plugins.AddFromObject(_foodCheckerPlugin);
            _kernel = kernel;
        }
        public bool IsValid()
        {
            return isValid;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ingredientResponse"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public IAsyncEnumerable<string> CheckFoodHealthAsync(string ingredientResponse, CancellationToken cancellationToken = default)
        {
            return _foodCheckerPlugin.CheckFoodHealthAsync(ingredientResponse, _kernel, cancellationToken);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageData"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// Inconsistant behaviour https://github.com/microsoft/semantic-kernel/pull/6319
        /// Related to https://github.com/dotnet/runtime/issues/96544
        public IAsyncEnumerable<string> GetIngredirentsAsync(ReadOnlyMemory<byte> imageData, string fileName, CancellationToken cancellationToken = default)
        {

            var imageDataUrl = new ImageContent(imageData) { MimeType = GetMimeType(fileName) }.ToString();

            return _foodCheckerPlugin.GetIngredientsAsync(imageDataUrl, _kernel, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public IAsyncEnumerable<string> GetIngredirentsAsync(string imageUrl, CancellationToken cancellationToken = default)
        {
            return _foodCheckerPlugin.GetIngredientsAsync(imageUrl, _kernel, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>  
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
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
