namespace ProductService.DTOs
{
    public class CreateReviewDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
        public string? Comment { get; set; }
    }
}
