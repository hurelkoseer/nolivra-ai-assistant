using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nolivra.Gateway.Domain.Entities;

namespace Nolivra.Gateway.Infrastructure.Configurations;

public sealed class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.ToTable("events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Details)
            .HasMaxLength(4000);

        builder.Property(x => x.StartAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.EndAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
