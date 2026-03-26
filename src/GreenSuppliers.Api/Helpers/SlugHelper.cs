using System.Text.RegularExpressions;

namespace GreenSuppliers.Api.Helpers;

public static class SlugHelper
{
    public static string Slugify(string input)
    {
        var slug = input.ToLowerInvariant().Trim();
        // Replace spaces and underscores with hyphens
        slug = Regex.Replace(slug, @"[\s_]+", "-");
        // Remove special characters (keep letters, digits, hyphens)
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        // Collapse multiple hyphens
        slug = Regex.Replace(slug, @"-{2,}", "-");
        // Trim leading/trailing hyphens
        slug = slug.Trim('-');
        return slug;
    }
}
