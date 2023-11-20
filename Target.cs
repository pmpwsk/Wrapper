using System.Diagnostics;
using System.Security.AccessControl;

/// <summary>
/// The watched process and 
/// </summary>
static class Target
{
    ///<summary>
    ///The process for the target. It stays the same, only the file and arguments change.
    ///</summary>
    static readonly Process TargetProcess = new();

    ///<summary>
    ///Indicates whether to continue listening to target output or abort.
    ///</summary>
    public static bool Continue = true;

    ///<summary>
    ///Sets up the process. This should only be called once.
    ///</summary>
    public static void ConfigureProcess()
    {
        TargetProcess.StartInfo.UseShellExecute = false;
        TargetProcess.StartInfo.RedirectStandardOutput = true;
        TargetProcess.StartInfo.WorkingDirectory = "../Live";
        TargetProcess.EnableRaisingEvents = true;
        TargetProcess.Exited += Exited;
    }

    ///<summary>
    ///Attempts to set the execute permission for the target file for Windows (untested, not included!) and Unix (using chmod).
    ///</summary>
    public static void TrySetPermission()
    {
        switch (Environment.OSVersion.Platform)
        {
            /* THIS NEEDS TESTING!
            case PlatformID.Win32NT:
            {
                var file = new FileInfo(Config.Target);
                var access = file.GetAccessControl();
                var rules = access.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                var acceptableRights = new FileSystemRights[]
                {
                    FileSystemRights.ExecuteFile, FileSystemRights.FullControl, FileSystemRights.ReadAndExecute
                };
                bool allowed = false;
                try
                {
                    foreach (System.Security.AccessControl.FileSystemAccessRule rule in rules)
                    {
                        allowed = allowed || acceptableRights.Contains(rule.FileSystemRights);
                    }
                }
                catch (Exception ex)
                {
                    Log.Wrapper($"Error getting access rules for target, skipping... ({ex.Message})");
                }
                
                if (!allowed)
                {
                    try
                    {
                        rules.AddRule(new FileSystemAccessRule(Environment.UserName, FileSystemRights.ExecuteFile, AccessControlType.Allow));
                        file.SetAccessControl(access);
                    }
                    catch (Exception ex)
                    {
                        Log.Wrapper($"Error setting execute rule for the target, skipping... ({ex.Message})");
                    }
                }
            } break;*/
            case PlatformID.Unix:
                try
                {
                    Process chmod = new();
                    chmod.StartInfo.FileName = "chmod";
                    chmod.StartInfo.UseShellExecute = false;
                    chmod.StartInfo.Arguments = $"+x ../Live/{Config.TargetFile}";
                    chmod.Start();
                    chmod.WaitForExit();
                }
                catch (Exception ex)
                {
                    Log.Wrapper($"Error setting execute rule for the target, skipping... ({ex.Message})");
                }
                break;
        }
    }

    ///<summary>
    ///Repeatedly reads output lines from the target process and forwards them or handles them as commands, until Continue=false.
    ///</summary>
    public static void HandleOutput()
    {
        while (Continue)
        {
            try
            {
                string? text = TargetProcess.StandardOutput.ReadLine();
                if (text != null)
                {
                    Log.General(text);
                    if (!Config.DisableCommands) Commands.Handle(text);
                }
                else Task.Delay(100).GetAwaiter().GetResult();
            }
            catch
            {
                Task.Delay(100).GetAwaiter().GetResult();
            }
        }
    }

    ///<summary>
    ///Restarts the target if Config.AutoRestart=true, otherwise sets Continue=false. This is being called when the target process exits.
    ///</summary>
    static void Exited(object? sender, EventArgs e)
    {
        Log.Wrapper("Target exited.");
        if (Config.AutoRestart)
        {
            try
            {
                UpdateAndStart();
            }
            catch (Exception ex)
            {
                Log.Wrapper($"Error restarting the target process, exiting... ({ex.Message})");
                Continue = false;
            }
        }
        else
        {
            Log.Wrapper("Exiting...");
            Continue = false;
        }
    }

    ///<summary>
    ///Attempts to rollback or update the target, then sets the path and arguments of the process to the current config and starts the process.
    ///</summary>
    public static void UpdateAndStart()
    {
        try
        {
            Versions.RollbackOrUpdate();
        }
        catch (Exception ex)
        {
            Log.Wrapper($"Error updating the target, skipping... ({ex.Message})");
        }
        TargetProcess.StartInfo.FileName = $"../Live/{Config.TargetFile}";
        TargetProcess.StartInfo.Arguments = Config.Arguments;
        Log.Wrapper("Starting the target process...");
        TargetProcess.Start();
    }
}