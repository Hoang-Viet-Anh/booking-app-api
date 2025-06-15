using booking_api.Context;
using booking_api.Interfaces;
using booking_api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace booking_api.Controllers;

[ApiController]
[Route("[controller]")]
public class Bookings : ControllerBase
{
    private readonly IBookingsService _bookingsService;

    public Bookings(IBookingsService bookingsService)
    {
        _bookingsService = bookingsService;
    }

    /// <summary>
    /// Gets all bookings
    /// </summary>
    /// <returns>Array of Booking</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _bookingsService.GetAllBookingAsync());
    }

    /// <summary>
    /// Gets booking by its ID.
    /// </summary>
    /// <param name="bookingId">The unique ID of the item.</param>
    [HttpGet("{bookingId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid bookingId)
    {
        var booking = await _bookingsService.GetBookingByIdAsync(bookingId);
        if (booking == null) return NotFound();
        return Ok(booking);
    }

    /// <summary>
    /// Create booking
    /// </summary>
    /// <param name="booking">The booking data to create.</param>
    /// <returns>200 OK if successful, 400 Bad Request if input is invalid, 409 Conflict if duplicate.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] BookingModelDto booking)
    {
        var result = await _bookingsService.AddBookingAsync(booking);
        if (result == null) return BadRequest();
        if (result.Count == 0) return Conflict();
        return Ok();
    }

    /// <summary>
    /// Update booking by ID
    /// </summary>
    /// <param name="bookingId">ID of existing booking</param>
    /// <param name="booking">The booking data to create.</param>
    /// <returns>200 OK if successful, 404 Not found if booking is not found, 409 Conflict if duplicate.</returns>
    [HttpPatch("{bookingId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromBody] BookingModelDto booking, Guid bookingId)
    {
        var updatedBooking = await _bookingsService.EditBookingAsync(bookingId, booking);
        if (updatedBooking == null) return NotFound();
        if (updatedBooking.Count == 0) return Conflict();
        return Ok();
    }

    /// <summary>
    /// Delete booking by ID
    /// </summary>
    /// <param name="bookingId">ID of existing booking</param>
    /// <returns>200 OK</returns>
    [HttpDelete("{bookingId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid bookingId)
    {
        await _bookingsService.DeleteBookingByIdAsync(bookingId);
        return Ok();
    }

    /// <summary>
    /// Get fully booked days that unavailable
    /// </summary>
    /// <returns>Array of DateTime</returns>
    [HttpPost("booked-days")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookedDays([FromBody] AvailabilityPropertiesDto availabilityProperties)
    {
        var dates = await _bookingsService.GetAllBookingDatesAsync(availabilityProperties);
        return Ok(dates);
    }

    /// <summary>
    /// Get available hours for start and end dates
    /// </summary>
    /// <returns>Object with startTimes and endTimes array of DateTime</returns>
    [HttpPost("available-hours")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableHours([FromBody] AvailabilityPropertiesDto availabilityProperties)
    {
        var times = await _bookingsService.GetAvailableTimeAsync(availabilityProperties);
        return Ok(times);
    }
}