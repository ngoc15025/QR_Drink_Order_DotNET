using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;

namespace QRDrinkOrder.API.Services.Interfaces;

public interface IReviewService
{
    Task<bool> SubmitReviewAsync(Guid sessionId, SubmitReviewRequest request);
    Task<List<ReviewDto>> GetReviewsForDrinkAsync(int drinkId);
    Task<List<ReviewDto>> GetAllReviewsAsync();
}
