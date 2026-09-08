using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KMC.ParticipantRegistrationService.Data;
using KMC.ParticipantRegistrationService.DTOs;
using KMC.ParticipantRegistrationService.Models;
using KMC.ParticipantRegistrationService.Services;

namespace KMC.ParticipantRegistrationService.Controllers
{
    // Participant Registration Service
    // Accepts registration requests from the public for a given event and
    // associates participants with events.
    [ApiController]
    [Route("api/registrations")]
    public class RegistrationsController : ControllerBase
    {
        private readonly RegistrationDbContext _db;
        private readonly EventServiceClient _eventClient;

        public RegistrationsController(RegistrationDbContext db, EventServiceClient eventClient)
        {
            _db = db;
            _eventClient = eventClient;
        }

        // POST api/registrations
        // Open to the public - no account required to register for an event.
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<RegistrationResponse>> Register(CreateRegistrationRequest request)
        {
            // Confirm the event actually exists by calling the Event Management Service.
            var ev = await _eventClient.GetEventAsync(request.EventId);
            if (ev is null)
                return BadRequest("The specified event does not exist.");

            var alreadyRegistered = await _db.Registrations
                .AnyAsync(r => r.EventId == request.EventId && r.ParticipantEmail == request.ParticipantEmail);
            if (alreadyRegistered)
                return Conflict("This email is already registered for the event.");

            if (ev.Capacity > 0)
            {
                var currentCount = await _db.Registrations.CountAsync(r => r.EventId == request.EventId);
                if (currentCount >= ev.Capacity)
                    return BadRequest("This event has reached its registration capacity.");
            }

            var registration = new Registration
            {
                EventId = request.EventId,
                EventTitle = ev.Title,
                ParticipantName = request.ParticipantName,
                ParticipantEmail = request.ParticipantEmail,
                ParticipantPhone = request.ParticipantPhone
            };

            _db.Registrations.Add(registration);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = registration.Id }, ToResponse(registration));
        }

        // GET api/registrations/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RegistrationResponse>> GetById(int id)
        {
            var reg = await _db.Registrations.FindAsync(id);
            if (reg is null) return NotFound();
            return Ok(ToResponse(reg));
        }

        // GET api/registrations/event/5
        // Used by an event organizer to see who has registered for their event.
        [HttpGet("event/{eventId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult<IEnumerable<RegistrationResponse>>> GetByEvent(int eventId)
        {
            var ev = await _eventClient.GetEventAsync(eventId);
            if (ev is null)
                return NotFound("The specified event does not exist.");

            var callerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(callerIdClaim, out var callerId) && ev.OrganizerId != callerId)
                return Forbid();

            var registrations = await _db.Registrations
                .Where(r => r.EventId == eventId)
                .OrderBy(r => r.RegisteredAt)
                .ToListAsync();

            return Ok(registrations.Select(ToResponse));
        }

        // GET api/registrations/my-registrations?email=user@example.com
        [HttpGet("my-registrations")]
        public async Task<ActionResult<IEnumerable<RegistrationResponse>>> GetMyRegistrations([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email parameter is required.");

            var registrations = await _db.Registrations
                .Where(r => r.ParticipantEmail.ToLower() == email.ToLower())
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            return Ok(registrations.Select(ToResponse));
        }

        // DELETE api/registrations/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelRegistration(int id, [FromQuery] string email)
        {
            var reg = await _db.Registrations.FindAsync(id);
            if (reg is null) return NotFound("Registration not found.");

            if (!string.IsNullOrWhiteSpace(email) && !string.Equals(reg.ParticipantEmail, email, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            _db.Registrations.Remove(reg);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private static RegistrationResponse ToResponse(Registration r) => new()
        {
            Id = r.Id,
            EventId = r.EventId,
            EventTitle = r.EventTitle,
            ParticipantName = r.ParticipantName,
            ParticipantEmail = r.ParticipantEmail,
            ParticipantPhone = r.ParticipantPhone,
            RegisteredAt = r.RegisteredAt
        };
    }
}
