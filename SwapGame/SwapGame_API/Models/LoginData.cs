namespace SwapGame_API.Models;

public struct LoginData {
    public string Name { get; set; }
    public string Password { get; set; }
}
public struct LoginResponseData { 
    public string jwt_token { get; set; }
}
