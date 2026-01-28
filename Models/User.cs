using System.ComponentModel.DataAnnotations;

namespace TokenManagerApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Guid UserId { get; set; } = Guid.NewGuid();
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
    }
}