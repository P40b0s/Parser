namespace SettingsWorker;
public class Paths
{
    public string RootDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
    public string DocumentsDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Documents");
    public string FilesUploadingDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/UploadedFiles");
    public string XmlDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Xml");
    public string ConfigurationsDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Configuration");

    private const string cfgDir = "configs";
    private const string tokensDefinitionsDir = "tokens";
    private const string dictionariesDir = "dictionaries";
    private const string fileName = "settings.json";
    public static string TokensDirPath => Path.Combine(cfgDir, tokensDefinitionsDir);
    public static  string ChangersDirPath => cfgDir;
    public static  string CustomRulesDirPath => cfgDir;
    public static  string DictionariesDirPath => Path.Combine(cfgDir, dictionariesDir);
    
    public static void createPaths()
    {
        System.IO.Directory.CreateDirectory(TokensDirPath);
        System.IO.Directory.CreateDirectory(ChangersDirPath);
        System.IO.Directory.CreateDirectory(DictionariesDirPath);
    }
}