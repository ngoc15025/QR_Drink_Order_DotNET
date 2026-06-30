using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.API.Services.Interfaces;
using QRDrinkOrder.Shared.DTOs.Requests;
using QRDrinkOrder.Shared.DTOs.Responses;
using QRDrinkOrder.API.Models;
using System.Text.RegularExpressions;

namespace QRDrinkOrder.API.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly QrdrinkOrderDbContext _context;
    private readonly IImageService _imageService;

    public ReviewService(QrdrinkOrderDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<bool> SubmitReviewAsync(Guid sessionId, SubmitReviewRequest request)
    {
        // 1. Kiểm tra đơn hàng tồn tại
        var order = await _context.Orders.FindAsync(request.OrderId);
        if (order == null)
            throw new Exception("Không tìm thấy đơn hàng cần đánh giá.");

        // 2. Ràng buộc bảo mật: Đúng Session ID đặt đơn mới được đánh giá
        if (order.SessionId != sessionId)
            throw new Exception("Bạn không có quyền đánh giá đơn hàng này.");

        // 3. Kiểm tra đã đánh giá chưa (Giới hạn đánh giá 1 lần duy nhất)
        var existingReview = await _context.Reviews.AnyAsync(r => r.OrderId == request.OrderId);
        if (existingReview)
            throw new Exception("Đơn hàng này đã được đánh giá trước đó.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var review = new Review
            {
                OrderId = request.OrderId,
                SessionId = sessionId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // 4. Xử lý lưu ảnh tải lên (nếu có) vào thư mục tĩnh wwwroot
            if (request.Base64Images != null && request.Base64Images.Count > 0)
            {
                foreach (var base64Str in request.Base64Images)
                {
                    if (string.IsNullOrEmpty(base64Str)) continue;

                    string extension = "jpg";
                    string rawBase64 = base64Str;

                    // Tách phần đầu base64 data:image/png;base64,... (nếu có)
                    var match = Regex.Match(base64Str, @"data:image/(?<type>.+?);base64,(?<data>.+)");
                    if (match.Success)
                    {
                        extension = match.Groups["type"].Value;
                        rawBase64 = match.Groups["data"].Value;
                    }

                    // Đề phòng phần mở rộng jpeg chuyển thành jpg
                    if (extension == "jpeg") extension = "jpg";

                    byte[] imageBytes = Convert.FromBase64String(rawBase64);
                    string fileName = $"{Guid.NewGuid()}.{extension}";
                    
                    using var stream = new MemoryStream(imageBytes);
                    string imageUrl = await _imageService.UploadImageAsync(stream, fileName, "Reviews");

                    var reviewImage = new ReviewImage
                    {
                        ReviewId = review.ReviewId,
                        ImageUrl = imageUrl,
                        UploadedAt = DateTime.Now
                    };
                    _context.ReviewImages.Add(reviewImage);
                }

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<ReviewDto>> GetReviewsForDrinkAsync(int drinkId)
    {
        // Lấy tất cả đánh giá của đơn hàng chứa món nước này
        var reviews = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.ReviewImages)
            .Include(r => r.Order)
                .ThenInclude(o => o.OrderItems)
            .AsSplitQuery()
            .Where(r => r.Order.OrderItems.Any(oi => oi.DrinkId == drinkId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(MapToReviewDto).ToList();
    }

    public async Task<List<ReviewDto>> GetAllReviewsAsync()
    {
        var reviews = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.ReviewImages)
            .Include(r => r.Order)
                .ThenInclude(o => o.Session)
            .AsSplitQuery()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(MapToReviewDto).ToList();
    }

    private static ReviewDto MapToReviewDto(Review review)
    {
        return new ReviewDto
        {
            ReviewId = review.ReviewId,
            OrderId = review.OrderId,
            SessionId = review.SessionId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt ?? DateTime.Now,
            ImageUrls = review.ReviewImages.Select(ri => ri.ImageUrl).ToList(),
            CustomerPhone = review.Order?.Session?.Phone,
            TableNumber = review.Order?.TableNumber
        };
    }
}
