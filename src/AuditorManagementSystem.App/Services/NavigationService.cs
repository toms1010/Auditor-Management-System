namespace AuditorManagementSystem.App.Services;

/// <summary>
/// Simple navigation service for the main window Shell.
/// Registry of named pages (UserControls) mapped to their type.
/// </summary>
public static class NavigationService
{
    public static readonly Dictionary<string, Type> Registry = new(StringComparer.OrdinalIgnoreCase);

    public static void Register(string name, Type pageType)
    {
        if (Registry.ContainsKey(name))
            Registry[name] = pageType;
        else
            Registry.Add(name, pageType);
    }

    public static bool TryGetType(string name, out Type? type)
        => Registry.TryGetValue(name, out type);

    public static object? CreateInstance(string name)
    {
        if (TryGetType(name, out var type))
            return Activator.CreateInstance(type);
        return null;
    }
}
