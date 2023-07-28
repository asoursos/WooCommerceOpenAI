namespace WebApplication2.Models;

public class DatabaseSettings
{
    public DatabaseSettings()
    {
        DatabaseName = "vector-db";
        Server = "127.0.0.1";
        Port = 5433;
        UserId = "postgres";
        Password = "postgres";
    }

    public string UserId { get; set; }
    public string Password { get; set; }
    public string DatabaseName { get; set; }
    public string Server { get; set; }
    public int Port { get; set; }
}
