/// <summary>
/// The wrapper's configuration and methods to load a configuration from a file.
/// </summary>
static class Config
{
    ///<summary>
    ///Indicates whether to include UTC timestamps in the log.<br/>
    ///Default: true
    ///</summary>
    public static bool Timestamps = true;

    ///<summary>
    ///Indicates whether to disable commands from the target output.<br/>
    ///Default: false
    ///</summary>
    public static bool DisableCommands = false;

    ///<summary>
    ///The target file path, relative to ../Live.<br/>
    ///Default: empty string (not acceptable)
    ///</summary>
    public static string TargetFile = "";

    ///<summary>
    ///The arguments to launch the target with or empty if none are set.<br/>
    ///Default: empty string (no arguments)
    ///</summary>
    public static string Arguments = "";

    ///<summary>
    ///The log file path, either relative to the parent directory or as an absolute path, or null if no log file is set.<br/>
    ///Default: null
    ///</summary>
    public static string? LogFile = null;

    ///<summary>
    ///The highest character count the log file should contain. If exceeded, the first 2/3 of the log file will be cleared.<br/>
    ///Default: 50000
    ///</summary>
    public static long LogLimit = 50000;

    ///<summary>
    ///Indicates whether to save the previous version of the target as a backup when updating or rolling back.<br/>
    ///Default: false
    ///</summary>
    public static bool CreateBackup = false;

    ///<summary>
    ///Indicates whether to automatically update and restart the target process or exit after it exited.<br/>
    ///Default: false
    ///</summary>
    public static bool AutoRestart = false;

    ///<summary>
    ///Parses all lines of the config file.
    ///</summary>
    public static void Load()
    {
        string[] lines = File.ReadAllLines("../Wrapper.config");
        foreach (string line in lines)
        {
            if (line != "" && !line.StartsWith('#'))
            {
                ParseLine(line);
            }
        }

        if (TargetFile == "") throw new Exception("No target was specified.");
        
    }

    ///<summary>
    ///Parses a given config line as part of the config file or as a temporary change.
    ///</summary>
    public static void ParseLine(string line)
    {
        int index = line.IndexOf('=');
        try
        {
            if (index == -1) throw new Exception("Invalid format.");

            string key = line.Remove(index);
            string value = line.Remove(0, index+1);
            switch (key)
            {
                case "Timestamps":
                    Timestamps = bool.Parse(value); break;
                case "DisableCommands":
                    DisableCommands = bool.Parse(value); break;
                case "Target":
                    if (!File.Exists("../Live/" + value)) throw new Exception("The target does not exist.");
                    else
                    {
                        TargetFile = value;
                        Target.TrySetPermission();
                    }
                    break;
                case "Arguments":
                    Arguments = value;
                    break;
                case "LogFile":
                    {
                        string path;
                        if (value.StartsWith('/') || value.StartsWith("\\\\") || (value.Length >= 2 && value[1] == ':'))
                            path = value;
                        else path = $"../{value}";
                        if (path != LogFile)
                        {
                            LogFile = path;
                            Log.Create();
                        }
                    } break;
                case "LogLimit":
                    LogLimit = long.Parse(value); break;
                case "CreateBackup":
                    CreateBackup = bool.Parse(value); break;
                case "AutoRestart":
                    AutoRestart = bool.Parse(value); break;
                default:
                    throw new Exception("Unknown identifier.");
            }
        }
        catch (Exception ex)
        {
            Log.Wrapper($"Invalid config line '{line}', ignoring... ({ex.Message})");
        }
    }
}