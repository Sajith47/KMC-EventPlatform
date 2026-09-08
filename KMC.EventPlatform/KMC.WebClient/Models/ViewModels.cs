using System.ComponentModel.DataAnnotations;

namespace KMC.WebClient.Models
{
    public class LoginViewModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }

    public class RegisterViewModel
    {
        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Display(Name = "I am registering as")]
        public string Role { get; set; } = "Public";

        public string? ErrorMessage { get; set; }
    }

    public class EventFormViewModel
    {
        public int Id { get; set; }

        [Required, Display(Name = "Event Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;

        [Required, Display(Name = "Event Type")]
        public string EventType { get; set; } = string.Empty;

        [Required, Display(Name = "Event Date & Time")]
        [DataType(DataType.DateTime)]
        public DateTime EventDate { get; set; } = DateTime.Now.AddDays(7);

        [Required, Display(Name = "Venue")]
        public string Venue { get; set; } = string.Empty;

        [Range(0, 100000), Display(Name = "Capacity (0 = unlimited)")]
        public int Capacity { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class PublicRegistrationViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;

        [Required, Display(Name = "Full Name")]
        public string ParticipantName { get; set; } = string.Empty;

        [Required, EmailAddress, Display(Name = "Email")]
        public string ParticipantEmail { get; set; } = string.Empty;

        [Display(Name = "Phone (optional)")]
        public string? ParticipantPhone { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    public class SearchViewModel
    {
        public string? Keyword { get; set; }
        public string? EventType { get; set; }
        public string? Date { get; set; }
        public List<string> AvailableTypes { get; set; } = new();
        public List<EventDto> Results { get; set; } = new();
    }

    public class DashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<RegistrationDto> RegisteredEvents { get; set; } = new();
        public ProfileViewModel ProfileModel { get; set; } = new();
        public ChangePasswordViewModel PasswordModel { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ProfileViewModel
    {
        [Required, Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required, Display(Name = "Current Password"), DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(6), Display(Name = "New Password"), DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, Compare("NewPassword", ErrorMessage = "Passwords do not match."), Display(Name = "Confirm New Password"), DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
