namespace TelonaiWebApi.Helpers.Configuration
{
    public static class Secret
    {
        public static void AddAmazonSecretsManager(
          this IConfigurationBuilder configurationBuilder,
          string region,
          string secretName)
        {
            var configurationSource = new AmazonSecretsManagerConfigurationSource(region, secretName);
            configurationBuilder.Add(configurationSource);
        }
    }
}
