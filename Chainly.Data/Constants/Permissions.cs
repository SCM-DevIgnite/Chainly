namespace Chainly.Data.Constants;

public static class Permissions
{
    public static List<string> GeneratePermissionsList(string module)
    {
        return new List<string>
        {
            $"Permissions.{module}.View",
            $"Permissions.{module}.Delete",
            $"Permissions.{module}.Create",
            $"Permissions.{module}.Update",
        };
    }
    
    public static List<string> GenerateAllPermissions()
    {
        var allPermissions = new List<string>();
        var modules = Enum.GetNames(typeof(Modules));
        foreach (var module in modules)
        {
            allPermissions.AddRange(GeneratePermissionsList(module));
        }
        return allPermissions;
    }
    
    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Delete = "Permissions.Users.Delete";
        public const string Create = "Permissions.Users.Create";
        public const string Update = "Permissions.Users.Update";
    }

    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Delete = "Permissions.Roles.Delete";
        public const string Create = "Permissions.Roles.Create";
        public const string Update = "Permissions.Roles.Update";
    }
}