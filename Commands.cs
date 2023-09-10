/// <summary>
/// Commands that the watched program can run to manage the wrapper.
/// </summary>
static class Commands
{
    ///<summary>
    ///Attempts to handle the provided command or ignores it if no command has been recognized.
    ///</summary>
    public static void Handle(string text)
    {
        try
        {
            if (!text.StartsWith("wrapper ")) return;
            string command = text.Remove(0, 8);
            string argument;
            int index = command.IndexOf(' ');
            if (index != -1)
            {
                argument = command.Remove(0, index+1);
                command = command.Remove(index);
            }
            else argument = "";

            switch (command)
            {
                case "set":
                    Config.ParseLine(argument);
                    break;
                case "log-backup":
                    Log.Backup();
                    break;
                case "log-clear":
                    Log.Clear();
                    break;
                case "rollback":
                    File.WriteAllText("../RollbackRequested", "true");
                    break;
                case "reload-config":
                    Config.Load();
                    break;
                default:
                    return;
            }

            if (command != "log-clear") Log.Wrapper("Executed the command.");
        }
        catch (Exception ex)
        {
            Log.Wrapper($"Error executing the command, skipping... ({ex.Message})");
        }
    }
}