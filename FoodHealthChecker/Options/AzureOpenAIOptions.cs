namespace FoodHealthChecker.Options
{
    internal class AzureOpenAIOptions
    {
        public string DeploymentName { get; set; }
        public string ApiKey { get; set; }
        public string Endpoint { get; set; }

        public bool isValid()
        {
            return !string.IsNullOrEmpty(DeploymentName) && !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(Endpoint);
        }
    }
}
