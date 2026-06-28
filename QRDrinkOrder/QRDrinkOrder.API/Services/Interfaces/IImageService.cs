namespace QRDrinkOrder.API.Services.Interfaces;

public interface IImageService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string folder);
}
