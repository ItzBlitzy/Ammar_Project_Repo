using Ammar_Project.Data;
using Ammar_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ammar_Project.Controllers
{
    [Authorize]
    [Route("api/[controller]/{action}")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(ApplicationDbContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
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
                return Problem(detail: "Booking with id " + id + " is not found.", statusCode: 404);

            return Ok(bookings);
        }

        [HttpGet("{status}")]
        public IActionResult GetByStatus(string? status = "All")
        {
            var userName = User.Identity.Name; // Get the logged-in user's name

            switch (status.ToLower())
            {
                case "all":
                    return Ok(_context.Bookings.Where(e => e.BookedBy == userName || User.IsInRole(UserRoles.Admin))); // Filter by user's bookings, or allow Admin to see all
                case "pending":
                    return Ok(_context.Bookings.Where(e => e.BookingStatus.ToLower() == "pending" && (e.BookedBy == userName || User.IsInRole(UserRoles.Admin))));
                case "approved":
                    return Ok(_context.Bookings.Where(e => e.BookingStatus.ToLower() == "approved" && (e.BookedBy == userName || User.IsInRole(UserRoles.Admin))));
                case "rejected":
                    return Ok(_context.Bookings.Where(e => e.BookingStatus.ToLower() == "rejected" && (e.BookedBy == userName || User.IsInRole(UserRoles.Admin))));
                default:
                    return Problem(detail: "Booking with status " + status + " is not found.", statusCode: 404);
            }
        }


        [HttpPost]
        public IActionResult Post(Booking booking)
        {
            try
            {
                // Set default values for BookedBy and BookingStatus
                booking.BookedBy = User.Identity.Name; // Set the logged-in user's name
                booking.BookingStatus = "Pending"; // Set the status to "Pending" by default

                // Add the booking to the database
                _context.Bookings.Add(booking);
                _context.SaveChanges();

                // Log successful booking creation
                _logger.LogInformation("Booking created successfully: {BookingId}", booking.BookingID);

                var successResponse = new { success = true, message = "Booking created successfully." };

                return CreatedAtAction("GetAll", new { id = booking.BookingID }, successResponse);
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError("Error creating booking: {ErrorMessage}", ex.Message);

                var errorResponse = new { success = false, message = ex.Message };
                return BadRequest(errorResponse);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int? id, Booking booking)
        {
            var entity = _context.Bookings.FirstOrDefault(e => e.BookingID == id);
            if (entity == null)
                return Problem(detail: "Booking with id " + id + " is not found.", statusCode: 404);

            // Update the booking properties
            entity.FacilityDescription = booking.FacilityDescription;
            entity.BookingDateFrom = booking.BookingDateFrom;
            entity.BookingDateTo = booking.BookingDateTo;
            entity.BookedBy = booking.BookedBy;
            entity.BookingStatus = booking.BookingStatus;

            // Save the changes to the database
            _context.SaveChanges();

            return Ok(entity);
        }


        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
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
