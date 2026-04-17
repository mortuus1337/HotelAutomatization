using Hotel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Persistence;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<GuestIdentity> GuestIdentities => Set<GuestIdentity>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationRoom> ReservationRooms => Set<ReservationRoom>();
    public DbSet<Stay> Stays => Set<Stay>();
    public DbSet<StayGuest> StayGuests => Set<StayGuest>();
    public DbSet<StayOperation> StayOperations => Set<StayOperation>();
    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();
    public DbSet<GeneratedDocument> GeneratedDocuments => Set<GeneratedDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
