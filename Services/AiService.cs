using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Betalgo.Ranul.OpenAI.Managers;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using booking_api.Context;
using booking_api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Services;

public class AiService : IAiService
{
    private readonly ILogger<AiService> _logger;
    private readonly BookingContext _context;
    private readonly OpenAIService _openAi;

    public AiService(ILogger<AiService> logger, BookingContext context, OpenAIService openAi)
    {
        _logger = logger;
        _context = context;
        _openAi = openAi;
    }

    public async Task<string> AskGroqAsync(string prompt)
    {
        var bookingList = await _context.Booking
            .Include(b => b.Coworking)
            .Include(b => b.Workspace)
            .Select(b => new
            {
                b.Name,
                b.Email,
                Coworking = new
                {
                    b.Coworking.Title,
                    b.Coworking.Description,
                    b.Coworking.Location,
                },
                Workspace = new
                {
                    b.Workspace.Title,
                    b.Workspace.Description,
                    b.Workspace.Amenities,
                    b.Workspace.AreaType,
                    b.Workspace.AreaTypeEmoji,
                    b.Workspace.MaxBookingDays
                },
                StartDate = b.StartDate.ToLongDateString(),
                EndDate = b.EndDate.ToLongDateString(),
                StartTime = b.StartDate.ToShortTimeString(),
                EndTime = b.EndDate.ToShortTimeString(),
                b.AreaCapacity,
            })
            .ToListAsync();
        var jsonBookingList = JsonSerializer.Serialize(bookingList, new JsonSerializerOptions { WriteIndented = true });

        var completionResult = await _openAi.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        {
            Messages = new List<ChatMessage>
            {
                ChatMessage.FromSystem(
                    "You are a helpful assistant in coworking bookings. Here is the user's bookings:\n" +
                    (bookingList.Count > 0
                        ? jsonBookingList
                        : "0 bookings")),
                ChatMessage.FromSystem("Even if bookings is booked for different people it is still current User's bookings."),
                ChatMessage.FromSystem(
                    "If you need to show booking information then show: “(start date - end date) - (workspace area type) for (area capacity) people/person at (coworking title) (start time) - (end time)“." +
                    "Only if user ask something specific then u can show other information."),
                ChatMessage.FromSystem(
                    "If you need to show booking date then show firstly date then information and then time."),
                ChatMessage.FromSystem("Today is " + DateTime.UtcNow.ToLongDateString() + " " +
                                       DateTime.UtcNow.ToShortTimeString()),
                ChatMessage.FromSystem("When the user asks about 'last week', interpret it as the 7 days before today."),
                ChatMessage.FromSystem("Return only answear without reasoning."),
                ChatMessage.FromSystem(
                    "If the question is non related , return a fallback message:\n “Sorry, I didn’t understand that. Please try rephrasing your question.“"),
                ChatMessage.FromUser(prompt)
            },
            Model = "llama-3.1-8b-instant"
        });

        return completionResult.Successful ? completionResult.Choices.First().Message.Content : "Something went wrong";
    }
}