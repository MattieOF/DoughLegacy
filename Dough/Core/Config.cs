using System.Reflection;
using System.Diagnostics;
using Tomlet;
using Tomlet.Models;

namespace Dough.Core;

internal static class ConfigFiles
{
    public const string EngineCore = "EngineCore.cfg";
    public const string EngineVideo = "EngineVideo.cfg";
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
class ConfigValue : Attribute
{
    /// <summary>
    /// <para>Name of the config value</para>
    /// </summary>
    // Todo:
    // Make it function like this for organisation purposes
    // <para>Use / to define a hierarchy of objects for it to be stored in\n</para>
    // E.g: <i>"Settings/Graphics/Fullscreen"</i> would result in it being stored in a a value called Fullscreen, 
    // which is inside an object called Graphics, which is inside an object called Settings
    public string Name { get; set; }

    /// <summary>
    /// File to store the value in. <b>Include</b> the extension
    /// </summary>
    public string File { get; set; }
    
    /// <summary>
    /// Comment to be written to the TOML. Useful for describing what a value does.
    /// <para>Persists between sessions if edited by user.</para>
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Default value to assign to the variable if its not found in the config file already
    /// <para>If <see cref="Default"/> is null, the value will remain set to what it has already been defined to.
    /// However, if the value has not been defined and is still null, it will be set to the result of <see cref="Activator.CreateInstance(Type)"/></para>
    /// </summary>
    // Todo: Add the ability to disable the Activator.CreateInstance behaviour
    // This would cause problems with the Toml serialiser though, as it doesn't support null
    public object Default { get; set; }

    public ConfigValue(string name, string file, string comment = null, object defaultValue = null)
    {
        Name = name;
        File = file;
        Comment = comment;
        Default = defaultValue;
    }
}

public class ConfigManager
{
    // Todo: Integrate with logging, include delegates for giving custom log functions
    // Todo: Add support for backups - copy all the cfgs when we load them. Configurable backup history length.

    public string ConfigDirectory => _configDirectory;

    /// <summary>
    /// File extensions that should be recognised as config files by <see cref="LoadConfigFiles"/>
    /// <para>Each entry should <b>include</b> the dot</para>
    /// </summary>
    public static readonly List<string> ConfigFileExtensions = new(new string[] { ".cfg" });

    // Property/field caches
    // ConfigValues discovered by InitConfig() are stored here for use by RefreshConfigValues()
    private static readonly List<FieldInfo>    CachedFields     = new();
    private static readonly List<PropertyInfo> CachedProperties = new();

    // Key is the filename of the config file (including ext), value is the TomlDoc representation
    private static readonly Dictionary<string, TomlDocument> ConfigFiles = new();
    // Root directory of config files. 
    private static string _configDirectory = string.Empty;

    /// <summary>
    /// Loads config files from the directory provided as an argument to <see cref="InitConfig(string)"/>
    /// <para>Use <see cref="ConfigFileExtensions"/> to define which file extensions should be recognised as config files</para>
    /// </summary>
    public static void LoadConfigFiles()
    {
        // Clear config files
        // Check given config dir exists
        //    If it doesn't, create it
        //    If creating it fails, log it and return
        // Get all files in the directory
        // Loop through all of the files found
        //    If the file extension is in configFileExtensions, parse it into a TomlDocument and add it to the dictionary

        ConfigFiles.Clear();

        if (!Directory.Exists(_configDirectory))
        {
            try
            {
                Directory.CreateDirectory(_configDirectory);
            } 
            catch (Exception e)
            {
                Log.EngineFatal("Failed to create directory for config files!");
                Log.EngineFatal(e.Message);
                return;
            }
        }

        var files = Directory.GetFiles(_configDirectory);
        foreach (string file in files)
        {
            if (ConfigFileExtensions.Contains(Path.GetExtension(file)))
                ConfigFiles.Add(Path.GetFileName(file), TomlParser.ParseFile(file));
        }

        // Console.WriteLine($"Loaded {configFiles.Count} config files!");
    }

    public static void SaveConfigFiles()
    {
        // For every config file/tomldoc, save its serialised value to a file
        foreach (string file in ConfigFiles.Keys)
            File.WriteAllText(Path.Combine(_configDirectory, file), ConfigFiles[file].SerializedValue);
    }

    /// <summary>
    /// Loads already existing config files, and then scans the calling assembly for fields and properties with the <see cref="ConfigValue"/>
    /// attribute. Found variables have their value set to the one found in the config files, or their default value if the config files did
    /// not contain a value.
    /// </summary>
    /// <param name="configDir">Root directory of config files.</param>
    /// <param name="assembly">Assembly to look for ConfigValues in. If null, the calling assembly.</param>
    public static void InitConfig(string configDir = "Config/", Assembly assembly = null)
    {
        // Load all files under config file path into TomlDocuments (dictionary with file name as key and tomldoc as value)
        // Look for configvalues in the assembly that called this function
        // Check for their values inside loaded config files:
        //   If it exists in the document, load the value into the field/property
        //   If it doesn't exist yet, add it to the document and use the provided default value

        Stopwatch timer = new();
        timer.Start();

        _configDirectory = configDir.Last() == '/' ? configDir : $"{configDir}/";
        LoadConfigFiles();
        CachedFields.Clear();
        CachedProperties.Clear();

        assembly = (assembly ?? Assembly.GetCallingAssembly());
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            // Fields:
            // For each field, check if it has a configvalue attrib
            // If it does, check it's a suitable value (is static, default is of the correct type)
            // Check if the config file exists. If it doesn't, create it
            // Check if the value exists in a loaded config file. If it does, set the field to the loaded value
            // If it doesn't, add it to the TomlDoc with either the defaultvalue, current value, or an empty instance
            // Add the field to the cached field list

            List<FieldInfo> fields = type.GetRuntimeFields().ToList();

            foreach (FieldInfo info in fields)
            {
                var configValue = info.GetCustomAttribute<ConfigValue>();
                if (configValue != null)
                {
                    if (configValue.Default != null && configValue.Default.GetType() != info.FieldType)
                    {
                        Log.EngineError($"Error: Field type and default value type do not match! Field \"{info.Name}\" in type \"{type.Name}\"");
                        continue;
                    }

                    if (!info.IsStatic)
                    {
                        Log.EngineError($"Error: Field must be static! Field \"{info.Name}\" in type \"{type.Name}\"");
                        continue;
                    }

                    if (!ConfigFiles.ContainsKey(configValue.File))
                        ConfigFiles.Add(configValue.File, TomlDocument.CreateEmpty());

                    if (ConfigFiles[configValue.File].ContainsKey(configValue.Name))
                        info.SetValue(null, TomletMain.To(info.FieldType, ConfigFiles[configValue.File].GetValue(configValue.Name)));
                    else
                    {
                        if (configValue.Default != null)
                            info.SetValue(null, configValue.Default);
                        else
                        {
                            if (info.GetValue(null) == null)
                                info.SetValue(null, Activator.CreateInstance(info.FieldType));
                        }

                        var val = TomletMain.ValueFrom(info.FieldType, info.GetValue(null));
                        if (!string.IsNullOrEmpty(configValue.Comment))
                            val.Comments.InlineComment = configValue.Comment;
                        ConfigFiles[configValue.File].PutValue(configValue.Name, val);
                    }

                    CachedFields.Add(info);
                }
            }

            // Properties: same as fields but with some different types
            // There may be a way to do this without repeating half of the function, but I don't know what it is so

            List<PropertyInfo> properties = type.GetProperties().ToList();

            foreach (PropertyInfo info in properties)
            {
                var configValue = info.GetCustomAttribute<ConfigValue>();
                if (configValue != null)
                {
                    if (configValue.Default != null && configValue.Default.GetType() != info.PropertyType)
                    {
                        Log.EngineError($"Error: Property type and default value type do not match! Property \"{info.Name}\" in type \"{type.Name}\"");
                        continue;
                    }

                    if (!info.GetGetMethod()!.IsStatic)
                    {
                        Log.EngineError("Error: Property must be static! Property \"{info.Name}\" in type \"{type.Name}\"");
                        continue;
                    }

                    if (!ConfigFiles.ContainsKey(configValue.File))
                        ConfigFiles.Add(configValue.File, TomlDocument.CreateEmpty());

                    if (ConfigFiles[configValue.File].ContainsKey(configValue.Name))
                        info.SetValue(null, TomletMain.To(info.PropertyType, ConfigFiles[configValue.File].GetValue(configValue.Name)));
                    else
                    {
                        if (configValue.Default != null)
                            info.SetValue(null, configValue.Default);
                        else
                        {
                            if (info.GetValue(null) == null)
                                info.SetValue(null, Activator.CreateInstance(info.PropertyType));
                        }
                        
                        var val = TomletMain.ValueFrom(info.PropertyType, info.GetValue(null));
                        if (!string.IsNullOrEmpty(configValue.Comment))
                            val.Comments.InlineComment = configValue.Comment;
                        ConfigFiles[configValue.File].PutValue(configValue.Name, val);
                    }

                    CachedProperties.Add(info);
                }
            }
        }

        SaveConfigFiles();

        timer.Stop();
        Log.EngineInfo($"Took {timer.ElapsedMilliseconds}ms to load config in {assembly.GetName().Name}");
    }

    /// <summary>
    /// Goes through every detected ConfigValue and set the value in the internal TomlDocument to its current value.
    /// <para>This function also saves config files to disk.</para>
    /// </summary>
    // Todo: Parameter to disable saving to disk
    public static void RefreshConfigValues()
    {
        // Stopwatch timer = new Stopwatch();
        // timer.Start();

        // For all cached fields and properties, put their current values into the respective TomlDoc.
        // There's no error/value checking here as we can assume all is correct since they must've gone through
        // the checks in InitConfig() to be added to the lists.

        foreach (FieldInfo info in CachedFields)
        {
            var configValue = info.GetCustomAttribute<ConfigValue>();
            ConfigFiles[configValue.File].PutValue(configValue.Name, TomletMain.ValueFrom(info.FieldType, info.GetValue(null)));
        }

        foreach (PropertyInfo info in CachedProperties)
        {
            var configValue = info.GetCustomAttribute<ConfigValue>();
            ConfigFiles[configValue.File].PutValue(configValue.Name, TomletMain.ValueFrom(info.PropertyType, info.GetValue(null)));
        }

        SaveConfigFiles();

        // timer.Stop();
        // Console.WriteLine($"Refreshing config values took {timer.ElapsedMilliseconds}ms");
    }
}