namespace FoodHealthChecker
{
    internal class OpenAIOptions
    {
        public string ApiKey { get; set; }
        public string ModelID { get; set; }

        public bool isValid()
        {
            return !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ModelID);
        }
    }
}