using booking_api.Context;
using booking_api.Models;

namespace booking_api.Seed;

public static class DbSeeding
{
    public static void SeedData(BookingContext context)
    {
        if (!context.Workspace.Any())
        {
            var workspacesToSeed = new List<WorkspaceModel>
            {
                new()
                {
                    Title = "Open space",
                    Description =
                        "A vibrant shared area perfect for freelancers or small teams who enjoy a collaborative atmosphere. Choose any available desk and get to work with flexibility and ease.",
                    ImageUrls = new()
                    {
                        "https://s3.magicman.cc/crocondine-booking-app/open-space/open-space1.jpg",
                        "https://s3.magicman.cc/crocondine-booking-app/open-space/open-space2.png",
                        "https://s3.magicman.cc/crocondine-booking-app/open-space/open-space3.png",
                        "https://s3.magicman.cc/crocondine-booking-app/open-space/open-space4.png"
                    },
                    Amenities = new() { "a/c", "gaming", "wifi", "coffee" },
                    MaxBookingDays = 1,
                    AreaType = "desk"
                },
                new()
                {
                    Title = "Private rooms",
                    Description =
                        "Ideal for focused work, video calls, or small team huddles. These fully enclosed rooms offer privacy and come in a variety of sizes to fit your needs.",
                    ImageUrls = new()
                    {
                        "https://s3.magicman.cc/crocondine-booking-app/private-rooms/private-rooms1.jpg",
                        "https://s3.magicman.cc/crocondine-booking-app/private-rooms/private-rooms2.png",
                        "https://s3.magicman.cc/crocondine-booking-app/open-space/open-space4.png"
                    },
                    Amenities = new() { "wifi", "a/c", "headphones" },
                    MaxBookingDays = 31,
                    AreaType = "room"
                },
                new()
                {
                    Title = "Meeting rooms",
                    Description =
                        "Designed for productive meetings, workshops, or client presentations. Equipped with screens, whiteboards, and comfortable seating to keep your sessions running smoothly.",
                    ImageUrls = new()
                    {
                        "https://s3.magicman.cc/crocondine-booking-app/meeting-rooms/meeting-rooms1.jpg",
                        "https://s3.magicman.cc/crocondine-booking-app/meeting-rooms/meeting-rooms2.png",
                        "https://s3.magicman.cc/crocondine-booking-app/meeting-rooms/meeting-rooms3.png",
                        "https://s3.magicman.cc/crocondine-booking-app/meeting-rooms/meeting-rooms4.png",
                    },
                    Amenities = new() { "wifi", "a/c", "headphones", "microphone" },
                    MaxBookingDays = 31,
                    AreaType = "room"
                }
            };

            context.Workspace.AddRange(workspacesToSeed);
            context.SaveChanges();
        }

        if (!context.Coworking.Any())
        {
            var workspaces = context.Workspace.ToList();

            var openSpace = workspaces.Find(w => w.Title == "Open space");
            var privateRooms = workspaces.Find(w => w.Title == "Private rooms");
            var meetingRooms = workspaces.Find(w => w.Title == "Meeting rooms");

            if (openSpace == null || privateRooms == null || meetingRooms == null)
            {
                return;
            }

            var coworkingsToSeed = new List<CoworkingModel>
            {
                new()
                {
                    Title = "WorkClub Pechersk",
                    Description = "Modern coworking in the heart of Pechersk with quiet rooms and coffee on tap.",
                    Location = "123 Yaroslaviv Val St, Kyiv",
                    ImageUrls = new List<string>([]),
                    WorkspacesCapacity = new List<WorkspaceCapacity>([
                        new()
                        {
                            WorkspaceId = openSpace.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 20,
                                    Capacity = 1,
                                },
                                new()
                                {
                                    Amounts = 10,
                                    Capacity = 2,
                                },
                                new()
                                {
                                    Amounts = 5,
                                    Capacity = 4,
                                }
                            ])
                        },
                        new()
                        {
                            WorkspaceId = privateRooms.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 2,
                                    Capacity = 1,
                                },
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 2,
                                },
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 4,
                                }
                            ])
                        },
                        new()
                        {
                            WorkspaceId = meetingRooms.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 10,
                                },
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 20,
                                }
                            ])
                        }
                    ])
                },

                new()
                {
                    Title = "UrbanSpace Podil",
                    Description = "A creative riverside hub ideal for freelancers or small startups.",
                    Location = "78 Naberezhno-Khreshchatytska St, Kyiv",
                    ImageUrls = new List<string>([]),
                    WorkspacesCapacity = new List<WorkspaceCapacity>([
                        new()
                        {
                            WorkspaceId = openSpace.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 12,
                                    Capacity = 1,
                                },
                                new()
                                {
                                    Amounts = 6,
                                    Capacity = 2,
                                },
                                new()
                                {
                                    Amounts = 2,
                                    Capacity = 4,
                                }
                            ])
                        },
                        new()
                        {
                            WorkspaceId = privateRooms.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 1,
                                },
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 2,
                                }
                            ])
                        },
                        new()
                        {
                            WorkspaceId = meetingRooms.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 10,
                                }
                            ])
                        }
                    ])
                },

                new()
                {
                    Title = "Creative Hub Lvivska",
                    Description = "A compact, design-focused space with open desks and strong community vibes.",
                    Location = "12 Lvivska Square, Kyiv",
                    ImageUrls = new List<string>([]),
                    WorkspacesCapacity = new List<WorkspaceCapacity>([
                        new()
                        {
                            WorkspaceId = openSpace.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 10,
                                    Capacity = 1,
                                },
                                new()
                                {
                                    Amounts = 5,
                                    Capacity = 2,
                                }
                            ])
                        },
                        new()
                        {
                            WorkspaceId = meetingRooms.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 10,
                                },
                            ])
                        }
                    ])
                },

                new()
                {
                    Title = "TechNest Olimpiiska",
                    Description = "A high-tech space near Olimpiiska metro, perfect for team sprints and solo focus.",
                    Location = "45 Velyka Vasylkivska St, Kyiv",
                    ImageUrls = new List<string>([]),
                    WorkspacesCapacity = new List<WorkspaceCapacity>([
                        new()
                        {
                            WorkspaceId = openSpace.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 20,
                                    Capacity = 1,
                                },
                                new()
                                {
                                    Amounts = 15,
                                    Capacity = 2,
                                },
                                new()
                                {
                                    Amounts = 5,
                                    Capacity = 4,
                                }
                            ])
                        },
                        new()
                        {
                            WorkspaceId = privateRooms.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 2,
                                    Capacity = 2,
                                },
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 4,
                                }
                            ])
                        },
                        new()
                        {
                            WorkspaceId = meetingRooms.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 10,
                                },
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 20,
                                }
                            ])
                        }
                    ])
                },

                new()
                {
                    Title = "Hive Station Troieshchyna",
                    Description = "A quiet, affordable option in the city's northeast - great for remote workers.",
                    Location = "102 Zakrevskogo St, Kyiv",
                    ImageUrls = new List<string>([]),
                    WorkspacesCapacity = new List<WorkspaceCapacity>([
                        new()
                        {
                            WorkspaceId = openSpace.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 20,
                                    Capacity = 1,
                                },
                                new()
                                {
                                    Amounts = 5,
                                    Capacity = 2,
                                },
                            ])
                        },
                        new()
                        {
                            WorkspaceId = privateRooms.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 2,
                                },
                            ])
                        },
                        new()
                        {
                            WorkspaceId = meetingRooms.Id,
                            Availability = new List<Availability>([
                                new()
                                {
                                    Amounts = 1,
                                    Capacity = 10,
                                }
                            ])
                        }
                    ])
                },
            };

            context.Coworking.AddRange(coworkingsToSeed);
            context.SaveChanges();
        }
    }
}