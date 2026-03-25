using Microsoft.EntityFrameworkCore;
using Skorbord.API.Models;

namespace Skorbord.API.Data
{
    public class SkorbordDbContext : DbContext
    {
        public SkorbordDbContext(DbContextOptions<SkorbordDbContext> options) : base(options)
        {
        }

        public DbSet<League> Leagues { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // League configuration
            modelBuilder.Entity<League>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Country).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Season).IsRequired().HasMaxLength(20);
                // Remove unique constraint on name since same league can exist in different seasons
                entity.HasIndex(e => new { e.Name, e.Season }).IsUnique();
                
                // Seed data since Sportmonks API doesn't provide bulk leagues on free tier
                var staticDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                entity.HasData(
                    new League { Id = 271, Name = "Superliga (Danimarka)", Country = "Denmark", Season = "2025/2026", CreatedAt = staticDate, UpdatedAt = staticDate },
                    new League { Id = 501, Name = "Premiership (İskoçya)", Country = "Scotland", Season = "2023/2024", CreatedAt = staticDate, UpdatedAt = staticDate }
                );
            });

            // Team configuration
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ShortName).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Stadium).HasMaxLength(100);
                entity.HasOne(e => e.League)
                      .WithMany(l => l.Teams)
                      .HasForeignKey(e => e.LeagueId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Player configuration
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Position).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Nationality).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.Team)
                      .WithMany(t => t.Players)
                      .HasForeignKey(e => e.TeamId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Match configuration
            modelBuilder.Entity<Match>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Venue).HasMaxLength(100);
                entity.HasOne(e => e.League)
                      .WithMany(l => l.Matches)
                      .HasForeignKey(e => e.LeagueId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.HomeTeam)
                      .WithMany(t => t.HomeMatches)
                      .HasForeignKey(e => e.HomeTeamId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.AwayTeam)
                      .WithMany(t => t.AwayMatches)
                      .HasForeignKey(e => e.AwayTeamId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
