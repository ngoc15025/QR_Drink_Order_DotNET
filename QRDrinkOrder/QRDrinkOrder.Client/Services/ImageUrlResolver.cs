namespace QRDrinkOrder.Client.Services;

public static class ImageUrlResolver
{
    public static string Resolve(string imageUrl, string? baseAddress)
    {
        if (string.IsNullOrEmpty(imageUrl)) return imageUrl;
        if (string.IsNullOrEmpty(baseAddress)) return imageUrl;

        if (imageUrl.Contains("/uploads/"))
        {
            int index = imageUrl.IndexOf("/uploads/");
            var relativePath = imageUrl.Substring(index);
            var baseUri = baseAddress.TrimEnd('/');
            return $"{baseUri}{relativePath}";
        }
        else if (imageUrl.StartsWith("uploads/"))
        {
            var baseUri = baseAddress.TrimEnd('/');
            return $"{baseUri}/{imageUrl}";
        }

        return imageUrl;
    }
}
