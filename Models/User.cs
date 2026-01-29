using System.ComponentModel.DataAnnotations;

namespace TokenManagerApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }
}