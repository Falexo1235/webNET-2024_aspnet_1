using Microsoft.EntityFrameworkCore;
using BlogApi.Models;

using BlogApi.Middleware;

namespace BlogApi.Data
{
    public class BlogDbContext : DbContext
    {
        
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Community> Communities { get; set; }
        public DbSet<CommunityUser> CommunityUsers { get; set; }

        public DbSet<RevokedToken> RevokedTokens { get; set; }  // Добавляем DbSet для черных токенов
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Для всех сущностей, которые используют UUID в качестве идентификатора, указываем default value через gen_random_uuid
            modelBuilder.Entity<Post>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()");  // Указываем функцию для генерации UUID по умолчанию

            modelBuilder.Entity<Comment>()
                .Property(c => c.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .HasDefaultValueSql("gen_random_uuid()");
        }

    }
    
}
