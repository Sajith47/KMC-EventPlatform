using System.ComponentModel.DataAnnotations;

namespace KMC.UserAuthService.Models
{
    // Roles supported by the KMC Event Platform
    public static class UserRoles
    {
        public const string Organizer = "Organizer";
        public const string Public = "Public";
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Role { get; set; } = UserRoles.Public;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
