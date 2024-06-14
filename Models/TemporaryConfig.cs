using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FoodHealthChecker.Models
{
    public class TemporaryConfig
    {
        public string AzureOpenAI_DeploymentName { get; set; }
        public string AzureOpenAI_Endpoint { get; set; }
        public string AzureOpenAI_ApiKey { get; set; }
        public string OpenAI_ModelId { get; set; }
        public string OpenAI_ApiKey { get; set; }
        public DateTime expiresOn { get; set; } = DateTime.UtcNow.AddDays(1);
        private bool isExpired { get; set; } = false;
        /// <summary>
        /// Checks if the Azure OpenAI configuration is valid.
        /// </summary>
        /// <returns>
        /// Returns true if valid Otherwise, returns false.
        /// </returns>
        public bool IsAzureOpenAIConfigValid()
        {
            return !string.IsNullOrEmpty(AzureOpenAI_DeploymentName) && !string.IsNullOrEmpty(AzureOpenAI_ApiKey) && !string.IsNullOrEmpty(AzureOpenAI_Endpoint);
        }

        /// <summary>
        /// Checks if the OpenAI configuration is valid.
        /// </summary>
        /// <returns>
        /// Returns true if valid Otherwise, returns false.
        /// </returns>
        public bool IsOpenAIConfigValid()
        {
            if (DateTime.UtcNow > expiresOn)
            {
                isExpired = true;
            }
            return !string.IsNullOrEmpty(OpenAI_ApiKey) && !string.IsNullOrEmpty(OpenAI_ModelId);
        }

        public async Task<bool> TryDeleteLocalStorage(ProtectedLocalStorage localStorage)
        {
            if (DateTime.UtcNow > expiresOn)
            {
                isExpired = true;
                await localStorage.DeleteAsync("TemporaryConfig");
            }
            return isExpired;
        }
    }
}
