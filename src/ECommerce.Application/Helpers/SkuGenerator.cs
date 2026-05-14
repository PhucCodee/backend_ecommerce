using System;
using System.Security.Cryptography;
using System.Text;

namespace ECommerce.Application.Helpers
{
    public static class SkuGenerator
    {
        private const string BaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int BaseSkuLength = 8;

        public static string GenerateBaseSku()
        {
            var randomBytes = new byte[BaseSkuLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var chars = new char[BaseSkuLength];
            for (int i = 0; i < BaseSkuLength; i++)
            {
                chars[i] = BaseChars[randomBytes[i] % BaseChars.Length];
            }

            return new string(chars);
        }

        public static string GenerateVariantSku(string baseSku, int variantId)
        {
            return $"{baseSku}-{variantId}";
        }
    }
}
