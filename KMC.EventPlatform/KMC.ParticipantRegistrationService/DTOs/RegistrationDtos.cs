using System.ComponentModel.DataAnnotations;

namespace KMC.ParticipantRegistrationService.DTOs
{
    public class CreateRegistrationRequest
    {
        [Required]
        public int EventId { get; set; }

        [Required, MaxLength(100)]
        public string ParticipantName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string ParticipantEmail { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ParticipantPhone { get; set; }
    }

    public class RegistrationResponse
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public string? ParticipantPhone { get; set; }
        public DateTime RegisteredAt { get; set; }
    }
}
