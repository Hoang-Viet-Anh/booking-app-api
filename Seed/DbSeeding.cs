using booking_api.Context;
using booking_api.Models;

namespace booking_api.Seed;

public static class DbSeeding
{
    public static void SeedData(BookingContext context)
    {
        if (context.Workspace.Any())
        {
            return;
        }

        var workspacesToSeed = new List<WorkspaceModel>
        {
            new()
            {
                Title = "Open space",
                Description = "A vibrant shared area perfect for freelancers or small teams who enjoy a collaborative atmosphere. Choose any available desk and get to work with flexibility and ease.",
                ImageUrls = new()
                {
                    "https://s3.magicman.cc/crocondine-booking-app/open-space/open-space1.jpg",
                    "https://s3.magicman.cc/crocondine-booking-app/open-space/open-space2.png",
                    "https://s3.magicman.cc/crocondine-booking-app/open-space/open-space3.png",
                    "https://s3.magicman.cc/crocondine-booking-app/open-space/open-space4.png"
                },
                Amenities = new() { "a/c", "gaming", "wifi", "coffee" },
                MaxBookingDays = 1,
                Availability = new Availability
                {
                    Type = "desk",
                    Rooms = new()
                    {
                        new Room { RoomsAmount = 12, Capacity = 1 },
                        new Room { RoomsAmount = 8, Capacity = 2 },
                        new Room { RoomsAmount = 4, Capacity = 4 }
                    }
                }
            },
            new()
            {
                Title = "Private rooms",
                Description = "Ideal for focused work, video calls, or small team huddles. These fully enclosed rooms offer privacy and come in a variety of sizes to fit your needs.",
                ImageUrls = new()
                {
                    "https://s3.magicman.cc/crocondine-booking-app/private-rooms/private-rooms1.jpg",
                    "https://s3.magicman.cc/crocondine-booking-app/private-rooms/private-rooms2.png",
                    "https://s3.magicman.cc/crocondine-booking-app/open-space/open-space4.png"
                },
                Amenities = new() { "wifi", "a/c", "headphones" },
                MaxBookingDays = 31,
                Availability = new Availability
                {
                    Type = "room",
                    Rooms = new()
                    {
                        new Room { RoomsAmount = 7, Capacity = 1 },
                        new Room { RoomsAmount = 4, Capacity = 2 },
                        new Room { RoomsAmount = 3, Capacity = 5 },
                        new Room { RoomsAmount = 1, Capacity = 10 }
                    }
                }
            },
            new()
            {
                Title = "Meeting rooms",
                Description = "Designed for productive meetings, workshops, or client presentations. Equipped with screens, whiteboards, and comfortable seating to keep your sessions running smoothly.",
                ImageUrls = new()
                {
                    "https://s3.magicman.cc/crocondine-booking-app/meeting-rooms/meeting-rooms1.jpg",
                    "https://s3.magicman.cc/crocondine-booking-app/meeting-rooms/meeting-rooms2.png",
                    "https://s3.magicman.cc/crocondine-booking-app/meeting-rooms/meeting-rooms3.png",
                    "https://s3.magicman.cc/crocondine-booking-app/meeting-rooms/meeting-rooms4.png",
                },
                Amenities = new() { "wifi", "a/c", "headphones", "microphone" },
                MaxBookingDays = 31,
                Availability = new Availability
                {
                    Type = "room",
                    Rooms = new()
                    {
                        new Room { RoomsAmount = 4, Capacity = 10 },
                        new Room { RoomsAmount = 1, Capacity = 20 }
                    }
                }
            }
        };

        context.Workspace.AddRange(workspacesToSeed);
        context.SaveChanges();
    }
}