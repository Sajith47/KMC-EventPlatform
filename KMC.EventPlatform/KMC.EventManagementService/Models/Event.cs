using System.ComponentModel.DataAnnotations;

namespace KMC.EventManagementService.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        // e.g. "Concert", "Workshop", "Festival", "Sports", "Exhibition"
        [Required, MaxLength(50)]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(150)]
        public string Venue { get; set; } = string.Empty;

        public int Capacity { get; set; }

        // Id of the User (from UserAuthService) who created this event.
        [Required]
        public int OrganizerId { get; set; }

        [MaxLength(150)]
        public string OrganizerName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
