using BestMusPortal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace BestMusPortal.Data
{

    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Song> Songs { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Song>(entity =>
            {
                entity.HasKey(e => e.SongId);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Artist)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.MusicFilePath)
                    .HasMaxLength(255);

                entity.Property(e => e.VideoFilePath)
                    .HasMaxLength(255);

                entity.Property(e => e.Mood)
                    .HasMaxLength(255);

                entity.HasOne<Genre>()
                    .WithMany(g => g.Songs)
                    .HasForeignKey(e => e.GenreId);


            });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.HasKey(e => e.GenreId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(255);

            });
            modelBuilder.Entity<Song>().Property(s => s.VideoUrl).HasMaxLength(500);
        }
    }
}