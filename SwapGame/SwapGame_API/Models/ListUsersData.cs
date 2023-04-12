namespace SwapGame_API.Models;

public struct UserData {
    public string? name { get; set; }
    public string? email { get; set; }
    public IEnumerable<string?> roles { get; set; }
}

