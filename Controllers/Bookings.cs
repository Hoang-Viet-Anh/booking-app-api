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

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _bookingsService.GetAllBookingAsync());
    }

    [HttpGet("{bookingId}")]
    public async Task<IActionResult> Get(Guid bookingId)
    {
        var booking = await _bookingsService.GetBookingByIdAsync(bookingId);
        if (booking == null) return NotFound();
        return Ok(booking);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BookingModelDto booking)
    {
        var result = await _bookingsService.AddBookingAsync(booking);
        if (result == null) return BadRequest();
        if (result.Count == 0) return Conflict();
        return Ok();
    }

    [HttpPatch("{bookingId}")]
    public async Task<IActionResult> Update([FromBody] BookingModelDto booking, Guid bookingId)
    {
        var updatedBooking = await _bookingsService.EditBookingAsync(bookingId, booking);
        if (updatedBooking == null) return NotFound();
        if (updatedBooking.Count == 0) return Conflict();
        return Ok();
    }

    [HttpDelete("{bookingId}")]
    public async Task<IActionResult> Delete(Guid bookingId)
    {
        await _bookingsService.DeleteBookingByIdAsync(bookingId);
        return Ok();
    }

    [HttpGet("booked-days")]
    public async Task<IActionResult> GetBookedDays([FromQuery] Guid workspaceId, [FromQuery] List<int> capacityList)
    {
        var dates = await _bookingsService.GetAllBookingDatesAsync(workspaceId, capacityList);
        return Ok(dates);
    }

    [HttpPost("available-hours")]
    public async Task<IActionResult> GetAvailableHours([FromQuery] Guid workspaceId, [FromQuery] List<int> capacityList,
        [FromBody] DateSlot dateSlot)
    {
        var times = await _bookingsService.GetAvailableTimeAsync(dateSlot, workspaceId, capacityList);
        return Ok(times);
    }
}