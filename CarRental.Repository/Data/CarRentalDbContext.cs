using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Repository.Data;

public class CarRentalDbContext : DbContext
{
    public CarRentalDbContext(DbContextOptions<CarRentalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Car> Cars { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<TariffCalculation> TariffCalculations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LicensePlate)
                .IsRequired()
                .HasMaxLength(20);
            entity.HasIndex(e => e.LicensePlate)
                .IsUnique();
            entity.Property(e => e.Model)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.ManufacturingYear)
                .IsRequired();
            entity.Property(e => e.DailyRate)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            entity.Property(e => e.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);
        });
        
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasIndex(e => e.Email)
                .IsUnique();
            entity.Property(e => e.LicenseNumber)
                .IsRequired()
                .HasMaxLength(50);
        });
        
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalCost)
                .HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Car)
                .WithMany()
                .HasForeignKey(e => e.CarId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<TariffCalculation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BaseRate)
                .HasColumnType("decimal(18,2)");
            entity.Property(e => e.LateFee)
                .HasColumnType("decimal(18,2)");
            entity.Property(e => e.RoadTax)
                .HasColumnType("decimal(18,2)");
            entity.Property(e => e.CurrencyConversionRate)
                .HasColumnType("decimal(18,6)");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18,2)");
            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3);
        });
    }
}
