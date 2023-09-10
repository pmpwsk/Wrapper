/// <summary>
/// Updates and reverts the watched program on restarts.
/// </summary>
static class Versions
{
    ///<summary>
    ///Checks if a rollback has been requested or an update exists and does the corresponding thing.
    ///</summary>
    public static void RollbackOrUpdate()
    {
        bool isRollback = false;

        //set the backup as an update if requested
        if (File.Exists("../RollbackRequested"))
        {
            File.Delete("../RollbackRequested");

            if (Directory.Exists("../Backup"))
            {
                if (Directory.Exists("../Update"))
                    Directory.Delete("../Update", true);
                
                Directory.Move("../Backup", "../Update");

                isRollback = true;
            }
            else Log.Wrapper("Rollback requested without a backup, skipping...");
        }

        //update if present (possibly after a version rollback)
        if (Directory.Exists("../Update"))
        {
            if (Config.CreateBackup)
            {
                if (Directory.Exists("../Backup"))
                    Directory.Delete("../Backup", true);
                
                Directory.Move("../Live", "../Backup");
            }
            else Directory.Delete("../Live", true);

            Directory.Move("../Update", "../Live");

            if (isRollback)
                Log.Wrapper("Reverted the target to its previous version.");
            else Log.Wrapper("Updated the target to a new version.");

            Target.TrySetPermission();
        }
    }
}