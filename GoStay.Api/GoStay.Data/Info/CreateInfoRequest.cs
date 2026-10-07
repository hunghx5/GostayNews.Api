using System.ComponentModel.DataAnnotations;

namespace GoStay.DataDto.Info
{
    public class CreateInfoRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Domain { get; set; }
    }
}
