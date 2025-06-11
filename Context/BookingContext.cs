using booking_api.Models;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Context;

public class BookingContext(DbContextOptions<BookingContext> options) : DbContext(options)
{
    public DbSet<BookingModel> Booking { get; set; }
    public DbSet<WorkspaceModel> Workspace { get; set; }

    public DbSet<CoworkingModel> Coworking { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkspaceModel>();

        modelBuilder.Entity<BookingModel>(entity => entity.OwnsOne(b => b.DateSlot));

        modelBuilder.Entity<CoworkingModel>(entity =>
            entity.OwnsMany(c => c.WorkspacesCapacity,
                wc =>
                {
                    wc.WithOwner();
                    wc.OwnsMany(w => w.Availability);
                }));
    }
}