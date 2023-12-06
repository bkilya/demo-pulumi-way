using Pulumi.AzureNative;

namespace Bk.Demo.Pulumi
{
    public static class ProviderFactory
    {
        public static Provider Create(string name, string subscriptionId, string clientId, string clientSecret)
        {
            return new Provider($"subscriptionProvider-{name}", new ProviderArgs()
            {
                SubscriptionId = subscriptionId,
                ClientId = clientId,
                ClientSecret = clientSecret
            }); ;
        }

        public static Provider Create(string name, string subscriptionId, bool useManagedIdentiry)
        {
            return new Provider($"subscriptionProvider-{name}", new ProviderArgs()
            {
                SubscriptionId = subscriptionId,
                UseMsi = useManagedIdentiry
            }); ;
        }
    }
}
