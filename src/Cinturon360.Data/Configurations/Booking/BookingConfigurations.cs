using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Booking;
using Cinturon360.Domain.Enums.Booking;

namespace Cinturon360.Data.Configurations.Booking;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Domain.Entities.Booking.Booking>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Booking.Booking> builder)
    {
        builder.ToTable("bookings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.TravellerUserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.BookedByUserId).HasMaxLength(50);
        builder.Property(x => x.QuoteId).HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.TotalAmountGross).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(x => x.PnrCode).HasMaxLength(20);
        builder.Property(x => x.SupplierRef).HasMaxLength(100);
        builder.Property(x => x.ExternalRef).HasMaxLength(200);
        builder.Property(x => x.CancellationReason).HasMaxLength(1000);
        builder.HasIndex(x => x.OrgId);
        builder.HasIndex(x => x.TravellerUserId);
        builder.HasIndex(x => x.Status);
    }
}

public sealed class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
{
    public void Configure(EntityTypeBuilder<BookingItem> builder)
    {
        builder.ToTable("booking_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.BookingId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ItemType).HasConversion<int>();
        builder.Property(x => x.SupplierCode).HasMaxLength(50);
        builder.Property(x => x.SupplierRef).HasMaxLength(100);
        builder.Property(x => x.OriginCode).HasMaxLength(10);
        builder.Property(x => x.DestinationCode).HasMaxLength(10);
        builder.Property(x => x.FlightNumber).HasMaxLength(20);
        builder.Property(x => x.CabinClass).HasConversion<int>();
        builder.Property(x => x.SeatNumber).HasMaxLength(10);
        builder.Property(x => x.HotelName).HasMaxLength(300);
        builder.Property(x => x.GrossAmount).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.FeeAmount).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(x => x.TicketNumber).HasMaxLength(50);
        builder.HasIndex(x => x.BookingId);
    }
}

public sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("quotes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.RequestedByUserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.TotalAmountGross).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(x => x.ProviderRef).HasMaxLength(200);
        builder.HasIndex(x => x.OrgId);
        builder.HasIndex(x => x.Status);
    }
}
