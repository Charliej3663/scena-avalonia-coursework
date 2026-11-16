using Microsoft.EntityFrameworkCore;
using ScenaAvalonia.Models;

namespace ScenaAvalonia.Data;

public sealed class StudioDbContext(DbContextOptions<StudioDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();
    public DbSet<StudioReview> Reviews => Set<StudioReview>();
    public DbSet<AppVisit> AppVisits => Set<AppVisit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasIndex(x => x.Phone);
        modelBuilder.Entity<Customer>().Property(x => x.Name).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Customer>().Property(x => x.Phone).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Customer>().Property(x => x.Email).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Enrollment>().Property(x => x.Status).HasMaxLength(30);
        modelBuilder.Entity<Enrollment>().HasOne(x => x.Customer).WithMany(x => x.Enrollments).HasForeignKey(x => x.CustomerId);
        modelBuilder.Entity<SupportMessage>().Property(x => x.Text).HasMaxLength(2000);
        modelBuilder.Entity<SupportMessage>().Property(x => x.Reply).HasMaxLength(2000);
        modelBuilder.Entity<StudioReview>().Property(x => x.Name).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<StudioReview>().Property(x => x.Text).HasMaxLength(2000).IsRequired();
        modelBuilder.Entity<AppVisit>().Property(x => x.VisitorName).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<AppVisit>().HasIndex(x => x.StartedAt);
    }
}

