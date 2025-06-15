using booking_api.Models;
using Microsoft.EntityFrameworkCore;

namespace booking_api.Context;

public class BookingContext(DbContextOptions<BookingContext> options) : DbContext(options)
{
    public DbSet<BookingModel> Booking { get; set; }
    public DbSet<WorkspaceModel> Workspace { get; set; }

    public DbSet<CoworkingModel> Coworking { get; set; }

    public DbSet<BookingTimeSlotModel> BookingTimeSlot { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookingModel>(entity =>
        {
            entity
                .HasOne(b => b.Coworking)
                .WithMany()
                .HasForeignKey(b => b.CoworkingId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(b => b.Workspace)
                .WithMany()
                .HasForeignKey(b => b.WorkspaceId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(b => b.TimeSlots)
                .WithOne(ts => ts.Booking)
                .HasForeignKey(ts => ts.BookingId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CoworkingModel>(entity =>
            entity.OwnsMany(c => c.WorkspacesCapacity,
                wc =>
                {
                    wc.WithOwner();
                    wc.OwnsMany(w => w.Availability);
                }));
    }
}