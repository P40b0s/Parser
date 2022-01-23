namespace SettingsWorker;

[AttributeUsage(AttributeTargets.All)]
public class PropertyAttribute : Attribute
{
    public string About {get;set;}
    public PropertyAttribute(string about)
    {
        About = about;
    }
}