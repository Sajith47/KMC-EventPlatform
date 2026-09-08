using System.ComponentModel.DataAnnotations;

namespace KMC.ParticipantRegistrationService.Models
{
    public class Registration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [MaxLength(150)]
        public string EventTitle { get; set; } = string.Empty;

        // The participant may or may not be a logged-in user of the platform,
        // so we capture their details directly rather than requiring an account.
        [Required, MaxLength(100)]
        public string ParticipantName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string ParticipantEmail { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ParticipantPhone { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
