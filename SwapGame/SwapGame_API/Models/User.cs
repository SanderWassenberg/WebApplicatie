using System.ComponentModel.DataAnnotations;

namespace SwapGame_API.Models {
    public class User {
        [Key]        public int Id { get; set; }
        [Required]   public string Name { get; set; }

        // [Range] works like 0..2, the min and max value are INCLUSIVE.
        [Range(0,2)] public int ProfilePic { get; set; } // fixed number referring to one of a few default images.
        [Required]   public string HashedPassword { get; set; }
    }
}
