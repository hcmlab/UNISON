using UnityEngine;

namespace Stations.Zones.Mall
{
    public enum Product
    {
        Disinfectant,
        HealthPoints,
        HealthCheck
    }

    public static class ProductExtensions
    {
        public static string GetDisplayName(this Product product)
        {
            return product switch
            {
                Product.Disinfectant => LocalizationUtility.GetLocalizedString("disinfectant"),
                Product.HealthPoints => LocalizationUtility.GetLocalizedString("healthPoints"),
                Product.HealthCheck => LocalizationUtility.GetLocalizedString("healthCheck"),
                _ => throw new UnityException($"Unknown product {product}!")
            };
        }
    }
}