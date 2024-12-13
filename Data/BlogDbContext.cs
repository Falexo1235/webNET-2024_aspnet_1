using Microsoft.EntityFrameworkCore;
using BlogApi.Models;
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
        public DbSet<AddrObj> AddrObjs { get; set; }
        public DbSet<AdmHierarchy> AdmHierarchies { get; set; }
        public DbSet<House> Houses { get; set; }
        public DbSet<ObjectLevel> ObjectLevels { get; set; }
        public DbSet<RevokedToken> RevokedTokens { get; set; }
        public DbSet<PostTag> PostTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Post>()
                .Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Comment>()
                .Property(c => c.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .HasDefaultValueSql("gen_random_uuid()");
             modelBuilder.Entity<CommunityUser>()
                .HasKey(cu => new { cu.UserId, cu.CommunityId });
            modelBuilder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostId, pt.TagId });
            modelBuilder.Entity<AddrObj>().ToTable("as_addr_obj");
            modelBuilder.Entity<AdmHierarchy>().ToTable("as_adm_hierarchy");
            modelBuilder.Entity<House>().ToTable("as_houses");
            
            modelBuilder.Entity<ObjectLevel>().ToTable("as_object_levels");
            modelBuilder.Entity<ObjectLevel>()
                .HasKey(o => o.level);
        }
    }
}
