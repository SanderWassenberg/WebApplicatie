namespace SwapGame_API.Models {
    public class User {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ProfilePic { get; set; } // fixed number referring to one of a few default images.
    }
}
