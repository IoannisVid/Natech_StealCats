using Microsoft.EntityFrameworkCore;
using StealTheCats.Entities.Models;

namespace StealTheCats.Entities
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Cat> Cat { get; set; }
        public DbSet<Tag> Tag { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cat>()
                .HasMany(p => p.Tags)
                .WithMany(c => c.Cats)
                .UsingEntity<Dictionary<string, object>>(
                    "CatTags",
                    j => j.HasOne<Tag>().WithMany().HasForeignKey("TagsId"),
                    j => j.HasOne<Cat>().WithMany().HasForeignKey("CatsId"),
                    j => j.HasKey("CatsId", "TagsId")
                );
        }
    }
}
