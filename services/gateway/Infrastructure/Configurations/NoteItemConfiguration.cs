using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nolivra.Gateway.Domain.Entities;

namespace Nolivra.Gateway.Infrastructure.Configurations;

public sealed class NoteItemConfiguration : IEntityTypeConfiguration<NoteItem>
{
    public void Configure(EntityTypeBuilder<NoteItem> builder)
    {
        builder.ToTable("notes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Details)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
