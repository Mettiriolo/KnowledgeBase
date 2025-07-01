using Microsoft.EntityFrameworkCore;
using KnowledgeBase.API.Models.Entities;
namespace KnowledgeBase.API.Data
{
    public class KnowledgeBaseDbContext(DbContextOptions<KnowledgeBaseDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<NoteTag> NoteTags { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired();
                entity.HasIndex(e => e.Token).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Note configuration
            modelBuilder.Entity<Note>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Notes)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Tag configuration
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Color).HasMaxLength(7);
                entity.HasMany(u => u.NoteTags);
            });

            // NoteTag (many-to-many) configuration
            modelBuilder.Entity<NoteTag>(entity =>
            {
                entity.HasKey(e => new { e.NoteId, e.TagId });
                entity.HasOne(e => e.Note)
                    .WithMany(n => n.NoteTags)
                    .HasForeignKey(e => e.NoteId);
                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.NoteTags)
                    .HasForeignKey(e => e.TagId);
            });

            // Attachment configuration
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.HasOne(e => e.Note)
                    .WithMany(n => n.Attachments)
                    .HasForeignKey(e => e.NoteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}