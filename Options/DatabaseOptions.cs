namespace TokenManagerApi.Options;

public class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string UsersTableName { get; set; } = "Users";
}
