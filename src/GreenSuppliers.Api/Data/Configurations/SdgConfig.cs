using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class SdgConfig : IEntityTypeConfiguration<Sdg>
{
    public void Configure(EntityTypeBuilder<Sdg> builder)
    {
        builder.ToTable("Sdgs");

        builder.HasKey(s => s.Id);

        // Int PK 1-17, not auto-generated — we seed explicit values
        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.Color)
            .IsRequired()
            .HasMaxLength(7);

        // Seed all 17 UN SDGs with official colors
        builder.HasData(
            new Sdg { Id = 1, Name = "No Poverty", Description = "End poverty in all its forms everywhere", Color = "#E5243B" },
            new Sdg { Id = 2, Name = "Zero Hunger", Description = "End hunger, achieve food security and improved nutrition and promote sustainable agriculture", Color = "#DDA63A" },
            new Sdg { Id = 3, Name = "Good Health and Well-Being", Description = "Ensure healthy lives and promote well-being for all at all ages", Color = "#4C9F38" },
            new Sdg { Id = 4, Name = "Quality Education", Description = "Ensure inclusive and equitable quality education and promote lifelong learning opportunities for all", Color = "#C5192D" },
            new Sdg { Id = 5, Name = "Gender Equality", Description = "Achieve gender equality and empower all women and girls", Color = "#FF3A21" },
            new Sdg { Id = 6, Name = "Clean Water and Sanitation", Description = "Ensure availability and sustainable management of water and sanitation for all", Color = "#26BDE2" },
            new Sdg { Id = 7, Name = "Affordable and Clean Energy", Description = "Ensure access to affordable, reliable, sustainable and modern energy for all", Color = "#FCC30B" },
            new Sdg { Id = 8, Name = "Decent Work and Economic Growth", Description = "Promote sustained, inclusive and sustainable economic growth, full and productive employment and decent work for all", Color = "#A21942" },
            new Sdg { Id = 9, Name = "Industry, Innovation and Infrastructure", Description = "Build resilient infrastructure, promote inclusive and sustainable industrialization and foster innovation", Color = "#FD6925" },
            new Sdg { Id = 10, Name = "Reduced Inequalities", Description = "Reduce inequality within and among countries", Color = "#DD1367" },
            new Sdg { Id = 11, Name = "Sustainable Cities and Communities", Description = "Make cities and human settlements inclusive, safe, resilient and sustainable", Color = "#FD9D24" },
            new Sdg { Id = 12, Name = "Responsible Consumption and Production", Description = "Ensure sustainable consumption and production patterns", Color = "#BF8B2E" },
            new Sdg { Id = 13, Name = "Climate Action", Description = "Take urgent action to combat climate change and its impacts", Color = "#3F7E44" },
            new Sdg { Id = 14, Name = "Life Below Water", Description = "Conserve and sustainably use the oceans, seas and marine resources for sustainable development", Color = "#0A97D9" },
            new Sdg { Id = 15, Name = "Life on Land", Description = "Protect, restore and promote sustainable use of terrestrial ecosystems, sustainably manage forests, combat desertification, and halt and reverse land degradation and halt biodiversity loss", Color = "#56C02B" },
            new Sdg { Id = 16, Name = "Peace, Justice and Strong Institutions", Description = "Promote peaceful and inclusive societies for sustainable development, provide access to justice for all and build effective, accountable and inclusive institutions at all levels", Color = "#00689D" },
            new Sdg { Id = 17, Name = "Partnerships for the Goals", Description = "Strengthen the means of implementation and revitalize the Global Partnership for Sustainable Development", Color = "#19486A" }
        );
    }
}
