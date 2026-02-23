using Microsoft.EntityFrameworkCore;

namespace DAL.Models
{
    public class RealEstateListingPlatformContext : DbContext
    {
        public RealEstateListingPlatformContext(DbContextOptions<RealEstateListingPlatformContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Listing> Listings { get; set; } = default!;
        public DbSet<ListingMedia> ListingMedia { get; set; } = default!;
        public DbSet<ListingPriceHistory> ListingPriceHistories { get; set; } = default!;
        public DbSet<ListingTour360> ListingTour360s { get; set; } = default!;
        public DbSet<Favorite> Favorites { get; set; } = default!;
        public DbSet<Lead> Leads { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<Report> Reports { get; set; } = default!;
        public DbSet<AuditLog> AuditLogs { get; set; } = default!;
        
        // Payment and Package system
        public DbSet<ListingPackage> ListingPackages { get; set; } = default!;
        public DbSet<UserPackage> UserPackages { get; set; } = default!;
        public DbSet<Transaction> Transactions { get; set; } = default!;
        public DbSet<ListingBoost> ListingBoosts { get; set; } = default!;

        // View tracking
        public DbSet<ListingView> ListingViews { get; set; } = default!;
        
        // Listing snapshots for approval tracking
        public DbSet<ListingSnapshot> ListingSnapshots { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Listing <-> ListingSnapshot relationships
            modelBuilder.Entity<Listing>()
                .HasMany(l => l.ListingSnapshots)
                .WithOne(s => s.Listing)
                .HasForeignKey(s => s.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Listing>()
                .HasOne(l => l.PendingSnapshot)
                .WithMany()
                .HasForeignKey(l => l.PendingSnapshotId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}