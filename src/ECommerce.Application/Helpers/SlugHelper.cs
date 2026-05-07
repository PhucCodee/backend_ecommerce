using System;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var sb = new StringBuilder(name.Length);
            var prevDash = false;

            foreach (var ch in name.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                    prevDash = false;
                }
                else if (!prevDash)
                {
                    sb.Append('-');
                    prevDash = true;
                }
            }

            return sb.ToString().Trim('-');
        }

        public static async Task<string> EnsureUniqueAsync(
            string baseSlug,
            Func<string, Task<bool>> existsAsync
        )
        {
            var slug = baseSlug;
            var counter = 1;

            while (await existsAsync(slug))
                slug = $"{baseSlug}-{counter++}";

            return slug;
        }
    }
}
