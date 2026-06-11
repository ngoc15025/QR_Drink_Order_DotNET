using System.ComponentModel.DataAnnotations;

namespace QRDrinkOrder.Shared.DTOs.Requests;

public class SubmitReviewRequest
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5 sao.")]
    public byte Rating { get; set; }

    [StringLength(1000, ErrorMessage = "Nội dung nhận xét không được vượt quá 1000 ký tự.")]
    public string? Comment { get; set; }

    // Hỗ trợ truyền ảnh dưới dạng chuỗi Base64 để lưu trữ ở thư mục tĩnh backend
    public List<string> Base64Images { get; set; } = new();
}
