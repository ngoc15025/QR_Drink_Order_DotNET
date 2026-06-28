using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using QRDrinkOrder.API.Services.Interfaces;

namespace QRDrinkOrder.API.Services.Implementations;

public class CloudinaryImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryImageService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        // Automatic format (f_auto) and quality (q_auto:best) to reduce size without losing quality
        if (folder.Equals("Banner", StringComparison.OrdinalIgnoreCase))
        {
            // Banner: crop to 16:9 ratio, width 1200px, gravity auto to focus on the most interesting part
            uploadParams.Transformation = new Transformation()
                .Width(1200).Height(675).Crop("fill").Gravity("auto")
                .Quality("auto:best").FetchFormat("auto");
        }
        else if (folder.Equals("Menu", StringComparison.OrdinalIgnoreCase))
        {
            // Menu items: square ratio 1:1, e.g. 800x800
            uploadParams.Transformation = new Transformation()
                .Width(800).Height(800).Crop("fill").Gravity("auto")
                .Quality("auto:best").FetchFormat("auto");
        }
        else
        {
            // Default: just apply auto quality and format
            uploadParams.Transformation = new Transformation()
                .Quality("auto:best").FetchFormat("auto");
        }

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl.ToString();
    }
}
