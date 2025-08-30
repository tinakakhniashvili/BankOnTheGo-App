namespace BankOnTheGo.API.Startup.Seed;

public static class SeedConstants
{
    public const string Password = "P@ssw0rd!";

    public const string AliceEmail = "alice@test.local";
    public const string BobEmail   = "bob@test.local";
    public const string CarolEmail = "carol@test.local";
    public const string DaveEmail  = "dave.admin@test.local";
    
    public static readonly string[] Currencies = { "USD", "EUR", "GEL" };

    public const string LinkPaid   = "LINKPAID1";
    public const string LinkOpen   = "LINKOPEN1";
    public const string LinkExpiry = "LINKEXPR1";
    public const string LinkPart   = "LINKPART1";
}