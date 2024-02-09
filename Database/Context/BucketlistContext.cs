using Bucketlist.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bucketlist.Database.Context
{
    public class BucketlistContext : DbContext
    {
        public BucketlistContext(DbContextOptions<BucketlistContext> options) : base(options)
        {
        }
        public DbSet<BucketListItem> BucketlistItems { get; set; }
    }
}