using System;
using System.IO;
using System.Threading;

namespace FolderSync
{
    class Program
    {
        static string sourceFolder = "";
        static string replicaFolder = "";
        static int syncInterval = 0;
        static string logFile = "";

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: FolderSync.exe <source_folder> <replica_folder> <sync_interval> <log_file>");
                return;
            }

            sourceFolder = args[0];
            replicaFolder = args[1];
            syncInterval = int.Parse(args[2]);
            logFile = args[3];

            if (!Directory.Exists(sourceFolder) || !Directory.Exists(replicaFolder))
            {
                Console.WriteLine("One or both of the specified folders do not exist.");
                return;
            }

            while (true)
            {
                SyncFolders();
                Thread.Sleep(syncInterval * 1000);
            }
        }

        static void SyncFolders()
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourceFolder);
            DirectoryInfo replicaDir = new DirectoryInfo(replicaFolder);

            if (!replicaDir.Exists)
            {
                replicaDir.Create();
            }

            FileInfo[] sourceFiles = sourceDir.GetFiles();
            FileInfo[] replicaFiles = replicaDir.GetFiles();

            // Copy new/updated files from source to replica
            foreach (FileInfo file in sourceFiles)
            {
                bool found = false;

                foreach (FileInfo replicaFile in replicaFiles)
                {
                    if (file.Name == replicaFile.Name && file.Length == replicaFile.Length)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    file.CopyTo(Path.Combine(replicaFolder, file.Name), true);
                    LogEvent($"Copied {file.Name} from {sourceFolder} to {replicaFolder}");
                }
                else
                {
                    FileInfo replicaFile = replicaDir.GetFiles(file.Name)[0];

                    if (file.LastWriteTime > replicaFile.LastWriteTime)
                    {
                        file.CopyTo(Path.Combine(replicaFolder, file.Name), true);
                        LogEvent($"Updated {file.Name} in {replicaFolder} with new version from {sourceFolder}");
                    }
                }
            }

            // Delete files from replica that don't exist in source
            foreach (FileInfo file in replicaFiles)
            {
                bool found = false;

                foreach (FileInfo sourceFile in sourceFiles)
                {
                    if (file.Name == sourceFile.Name && file.Length == sourceFile.Length)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    file.Delete();
                    LogEvent($"Deleted {file.Name} from {replicaFolder}");
                }
            }
        }

        static void LogEvent(string message)
        {
            Console.WriteLine(message);
            File.AppendAllText(logFile, message + Environment.NewLine);
        }
    }
}