using System.ComponentModel.DataAnnotations;

namespace Bucketlist.Database.Entities
{
    public class BucketListItem
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
    }
}