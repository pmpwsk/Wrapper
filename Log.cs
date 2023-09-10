/// <summary>
/// The log for the wrapper and the watched program.
/// </summary>
static partial class Log
{
    ///<summary>
    ///The lock that should be entered to write to the log file. Other locking methods have to wait until it is finished.
    ///</summary>
    static ReaderWriterLockSlim Lock = new();

    ///<summary>
    ///The count of characters that have been written to the log file so far.
    ///</summary>
    static long WrittenLines = 0;

    ///<summary>
    ///Writes the given text to the log (console and log file if enabled), adding [Wrapper] to the front and possibly a timestamp before that.
    ///</summary>
    public static void Wrapper(string text, bool lockLog = true)
    {
        General($"[Wrapper] {text}", lockLog);
    }

    ///<summary>
    ///Writes the given text to the log (console and log file if enabled), possibly adding a timestamp to the front.
    ///</summary>
    public static void General(string text, bool lockLog = true)
    {
        text = TimestampPrefix() + text;
        Console.WriteLine(text);

        if (Config.LogFile != null)
        {
            try
            {
                if (lockLog) Lock.EnterWriteLock();
                WriteToFile(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine(FormatWrapper($"Error writing to the log file, skipping... ({ex.Message})"));
            }
            finally
            {
                if (lockLog) Lock.ExitWriteLock();
            }
        }
    }

    ///<summary>
    ///Adds [Wrapper] and possibly a timestamp to the front of the provided text.
    ///</summary>
    static string FormatWrapper(string text)
        => $"{TimestampPrefix()}[Wrapper] {text}";

    ///<summary>
    ///Writes the given text to the log file and clears the first 2/3 of it if the character limit is exceeded.
    ///</summary>
    static void WriteToFile(string text)
    {
        if (Config.LogFile == null) return;

        if (WrittenLines + 1 > Config.LogLimit)
        {
            using StreamReader reader = new StreamReader(Config.LogFile);
            using StreamWriter writer = new StreamWriter($"{Config.LogFile}.temp", false);
            long newCount = 0;
            long toSkip = Config.LogLimit * 2 / 3;
            string? line;
            while (toSkip > 0 && (line = reader.ReadLine()) != null)
            {
                toSkip--;
            }
            while ((line = reader.ReadLine()) != null)
            {
                newCount++;
                writer.WriteLine(line);
            }

            string log = FormatWrapper("Cleared the first 2/3 of the log file.");
            Console.WriteLine(log);
            newCount++;
            writer.WriteLine(log);

            newCount++;
            writer.WriteLine(text);

            WrittenLines = newCount;

            writer.Flush();
            writer.Close();
            reader.Close();

            File.Delete(Config.LogFile);
            File.Move($"{Config.LogFile}.temp", Config.LogFile);
        }
        else
        {
            File.AppendAllLines(Config.LogFile, new[] {text});
            WrittenLines++;
        }
    }

    ///<summary>
    ///Creates a new log file at the configured location.
    ///</summary>
    public static void Create()
    {
        if (Config.LogFile == null) return;

        try
        {
            Lock.EnterWriteLock();
            WrittenLines = 0;
            File.WriteAllText(Config.LogFile, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine(FormatWrapper($"Error creating a new log file, skipping... ({ex.Message})"));
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    ///<summary>
    ///Creates a backup of the log file at [log file].backup that is safe to read.
    ///</summary>
    public static void Backup()
    {
        if (Config.LogFile == null) return;

        try
        {
            Lock.EnterWriteLock();

            File.Copy(Config.LogFile, $"{Config.LogFile}.backup", true);
        }
        catch (Exception ex)
        {
            Wrapper($"Error backing up the log file, skipping... ({ex.Message})", false);
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    ///<summary>
    ///Completely clears the log file.
    ///</summary>
    public static void Clear()
    {
        if (Config.LogFile == null) return;

        try
        {
            Lock.EnterWriteLock();

            string log = FormatWrapper("Cleared the log.");
            WrittenLines = 1;
            File.WriteAllText(Config.LogFile, log + '\n');
            Console.WriteLine(log);
        }
        catch (Exception ex)
        {
            Console.WriteLine(FormatWrapper($"Error clearing the log file, skipping... ({ex.Message})"));
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }

    ///<summary>
    ///Returns a prefix with the current timestamp if enabled, or an empty string otherwise.
    ///</summary>
    static string TimestampPrefix()
    {
        if (!Config.Timestamps) return "";

        static string ext(int x, int length)
        {
            string result = x.ToString();
            while (result.Length < length)
                result = "0" + result;
            return result;
        }

        DateTime d = DateTime.UtcNow;
        return $"{d.Year}/{ext(d.Month,2)}/{ext(d.Day,2)}-{ext(d.Hour,2)}:{ext(d.Minute,2)}:{ext(d.Second,2)}.{ext(d.Millisecond,3)}UTC: ";
    }
}