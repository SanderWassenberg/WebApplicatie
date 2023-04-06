namespace SwapGame_API;

interface IHasPath {
    public string Path { get; }
}

public class SwapGameOptions : IHasPath {
    public string Path => "SwapGame";
    public string Extreem_Veilige_DB_pls_no_hack { get; set; }
}

public class JwtOptions : IHasPath {
    public string Path => "SwapGame:Jwt";
    public string Issuer {get;set;}
    public string Audience {get;set;}
    public string Key { get; set; }
}

public class EmailOptions : IHasPath {
    public string Path => "SwapGame:Email";
    public string Api_Key { get; set; }
    public string Sender_Address { get; set; }
}
