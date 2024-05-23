using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace FoodHealthChecker.Models
{

    public class TemporaryConfig
    {
        public string AzureOpenAI_DeploymentName { get; set; }
        public string AzureOpenAI_Endpoint { get; set; }
        public string AzureOpenAI_ApiKey { get; set; }
        public string OpenAI_ModelId { get; set; }
        public string OpenAI_ApiKey { get; set; }

        public bool isAzureOpenAIConfigValid()
        {
            return !string.IsNullOrEmpty(AzureOpenAI_DeploymentName) && !string.IsNullOrEmpty(AzureOpenAI_ApiKey) && !string.IsNullOrEmpty(AzureOpenAI_Endpoint);
        }
        public bool isOpenAIConfigValid()
        {
            return !string.IsNullOrEmpty(OpenAI_ApiKey) && !string.IsNullOrEmpty(OpenAI_ModelId);
        }
    }
}
