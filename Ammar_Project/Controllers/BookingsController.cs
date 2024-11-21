using Ammar_Project.Data;
using Ammar_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ammar_Project.Controllers
{
    [Authorize]
    [Route("api/[controller]/{action}")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.Bookings);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int? id)
        {
            var bookings = _context.Bookings.FirstOrDefault(e => e.BookingID == id);
            if (bookings == null)
                return Problem(detail:"Booking with id " + id + " is not found.", statusCode:404);

            return Ok(bookings);
        }

        [HttpGet("{status}")]
        public IActionResult GetByStatus(string? status = "All")
        {
            switch (status.ToLower())
            {
                case "all":
                    return Ok(_context.Bookings);
                case "pending":
                    return Ok(_context.Bookings.Where(e => e.BookingStatus.ToLower() == "pending"));
                case "approved":
                    return Ok(_context.Bookings.Where(e => e.BookingStatus.ToLower() == "approved"));
                case "rejected":
                    return Ok(_context.Bookings.Where(e => e.BookingStatus.ToLower() == "rejected"));
                default:
                    return Problem(detail: "Booking with status " + status + " is not found.", statusCode: 404);
            }
        }

        [HttpPost]
        public IActionResult Post(Booking booking)
        {
            _context.Bookings.Add(booking);
            _context.SaveChanges();

            return CreatedAtAction("GetAll", new { id = booking.BookingID }, booking);
        }

        [HttpPut]
        public IActionResult Put(int? id, Booking booking)
        {
            var entity = _context.Bookings.FirstOrDefault(e => e.BookingID == id);
            if (entity == null)
                return Problem(detail: "Booking with id " + id + " is not found.", statusCode: 404);

            entity.FacilityDescription = booking.FacilityDescription;
            entity.BookingDateFrom = booking.BookingDateFrom;
            entity.BookingDateTo = booking.BookingDateTo;
            entity.BookedBy = booking.BookedBy;
            entity.BookingStatus = booking.BookingStatus;

            _context.SaveChanges();

            return Ok(entity);
        }

        [HttpDelete]
        public IActionResult Delete(int? id, Booking booking)
        {
            var entity = _context.Bookings.FirstOrDefault(e => e.BookingID == id);
            if (entity == null)
                return Problem(detail: "Booking with id " + id + " is not found.", statusCode: 404);

            _context.Bookings.Remove(entity);
            _context.SaveChanges();

            return Ok(entity);
        }
    }
}
