using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KMC.EventManagementService.Data;
using KMC.EventManagementService.DTOs;
using KMC.EventManagementService.Models;

namespace KMC.EventManagementService.Controllers
{
    // Event Management Service
    // Responsible for creation, retrieval, update and deletion of events.
    // Only the organizer who created an event may update or delete it.
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly EventDbContext _db;

        public EventsController(EventDbContext db)
        {
            _db = db;
        }

        // GET api/events
        // Public endpoint - used directly, or by the Event Search Service,
        // to list all events. Consumers can also filter here for convenience.
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventResponse>>> GetAll(
            [FromQuery] string? type, [FromQuery] DateTime? date, [FromQuery] string? keyword)
        {
            var query = _db.Events.AsQueryable();

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(e => e.EventType.ToLower() == type.ToLower());

            if (date.HasValue)
                query = query.Where(e => e.EventDate.Date == date.Value.Date);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(e => e.Title.Contains(keyword) || e.Description.Contains(keyword) || e.Venue.Contains(keyword));

            var events = await query.OrderBy(e => e.EventDate).ToListAsync();
            return Ok(events.Select(ToResponse));
        }

        // GET api/events/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<EventResponse>> GetById(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev is null) return NotFound();
            return Ok(ToResponse(ev));
        }

        // GET api/events/organizer/5
        // Lets an organizer's dashboard show only the events they created.
        [HttpGet("organizer/{organizerId:int}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<EventResponse>>> GetByOrganizer(int organizerId)
        {
            var callerId = GetUserId();
            if (callerId != organizerId)
                return Forbid();

            var events = await _db.Events
                .Where(e => e.OrganizerId == organizerId)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(events.Select(ToResponse));
        }

        // POST api/events
        // Only authenticated Organizers can create events.
        [HttpPost]
        [Authorize(Roles = "Organizer")]
        public async Task<ActionResult<EventResponse>> Create(CreateEventRequest request)
        {
            var ev = new Event
            {
                Title = request.Title,
                Description = request.Description,
                EventType = request.EventType,
                EventDate = request.EventDate,
                Venue = request.Venue,
                Capacity = request.Capacity,
                OrganizerId = GetUserId(),
                OrganizerName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown"
            };

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ev.Id }, ToResponse(ev));
        }

        // PUT api/events/5
        // Only the organizer who created the event may update it.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Update(int id, UpdateEventRequest request)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev is null) return NotFound();

            if (ev.OrganizerId != GetUserId())
                return StatusCode(403, "Only the creator of this event may update it.");

            ev.Title = request.Title;
            ev.Description = request.Description;
            ev.EventType = request.EventType;
            ev.EventDate = request.EventDate;
            ev.Venue = request.Venue;
            ev.Capacity = request.Capacity;
            ev.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(ToResponse(ev));
        }

        // DELETE api/events/5
        // Only the organizer who created the event may delete it.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev is null) return NotFound();

            if (ev.OrganizerId != GetUserId())
                return StatusCode(403, "Only the creator of this event may delete it.");

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private static EventResponse ToResponse(Event e) => new()
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            EventType = e.EventType,
            EventDate = e.EventDate,
            Venue = e.Venue,
            Capacity = e.Capacity,
            OrganizerId = e.OrganizerId,
            OrganizerName = e.OrganizerName,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}
