namespace Nolivra.Gateway.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nolivra.Gateway.Domain.Entities;

internal sealed class AssistantRequestLogConfiguration : IEntityTypeConfiguration<AssistantRequestLog>
{
    public void Configure(EntityTypeBuilder<AssistantRequestLog> builder)
    {
        builder.ToTable("assistant_request_logs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RawUserInput)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.RawAiResponse)
            .IsRequired()
            .HasMaxLength(8000);

        builder.Property(x => x.ParsedIntent)
            .HasMaxLength(50);

        builder.Property(x => x.ErrorDetail)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
