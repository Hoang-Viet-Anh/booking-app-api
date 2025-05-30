using booking_api.Models;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Context;

public class BookingContext(DbContextOptions<BookingContext> options) : DbContext(options)
{
    public DbSet<BookingModel> Booking { get; set; }
    public DbSet<WorkspaceModel> Workspace { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkspaceModel>(entity =>
        {
            entity.OwnsOne(w => w.Availability, availability =>
            {
                availability.Property(a => a.Type);
                availability.OwnsMany(a => a.Rooms);
            });
        });

        modelBuilder.Entity<BookingModel>(entity => { entity.OwnsOne(b => b.DateSlot); });
    }
}