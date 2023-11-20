/// <summary>
/// Main program.
/// </summary>
static class Wrapper
{
    ///<summary>
    ///The main program logic.
    ///</summary>
    public static void Main(string[] _)
    {
        try
        {
            Config.Load();
        }
        catch (Exception ex)
        {
            Log.Wrapper($"Error reading the config file, exiting... ({ex.Message})");
            return;
        }

        Target.ConfigureProcess();

        Log.Wrapper("uwap.dev/wrapper 2.1.3");

        try
        {
            Target.UpdateAndStart();
        }
        catch (Exception ex)
        {
            Log.Wrapper($"Error starting the target process, exiting... ({ex.Message})");
            return;
        }

        Target.HandleOutput();
    }
}