﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using ChecksumEncoder;

namespace LazyCopy
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Any()) {
                var argCount = 0;

                Console.WriteLine("Lazycopy!");

                while (!ShouldExit(args[argCount]))
                {
                    if (args[argCount] == "backup")
                    {
                        Console.WriteLine("backup initialized!");
                        Backup();
                    }
                    if (args.Length == ++argCount)
                    {
                        break;
                    }
                    
                }
            }

            ShouldExit("exit");
            Console.ReadKey();

        }

        private static bool ShouldExit(string arg = "")
        {
            if (arg.ToLower() == "exit" || arg == "")
            {
                Console.WriteLine("exiting!");
                return true;
            }

            return false;
        }

        private static void Backup()
        {
            //setup constants of locations to copy
            const string golang = @"C:\Users\journ\go\src";
            const string repos = @"C:\Users\journ\source\repos";
            //setup constant for base save location
            const string destination = @"C:\Users\journ\OneDrive\OneDrive - coderbox.co\.LazyCopy Backup";
            const string temp_destination = @"C:\temp\.LazyCopy";
            //for each location ZipFile.CreateFromDirectory(d,temp_destinary);
            //todo: if (Directory.Exists(golang) || Directory.Exists(repos))
            
            var goDirectories = Directory.EnumerateDirectories(golang);
            var repoDirectories = Directory.EnumerateDirectories(repos);
            var savedZips = Directory.EnumerateFiles(destination, "*.zip");
            
            //check if all the folders are present
            
            foreach (var d in goDirectories)
            {
                var directoryName = new DirectoryInfo(d);
                try
                {
                    if (savedZips.Any(z => z.Contains(directoryName.Name)))
                    {
                        //zip the folder into temp directory
                        ZipFile.CreateFromDirectory(d, temp_destination + "\\go_" + directoryName.Name + ".zip");
                        //todo:encrypt temp zip files of the folders

                        //copy the last zip saved for project from destination to backup
                        File.Copy(destination + "\\go_" + directoryName.Name + ".zip",
                            destination + "\\backup\\go_" + directoryName.Name + "_" + DateTime.UtcNow.GetHashCode() +
                            ".zip");

                        //copy the temp file to the destination, overwritting is allowed
                        File.Copy(temp_destination + "\\go_" + directoryName.Name + ".zip",
                            destination + "\\go_" + directoryName.Name + ".zip", true);

                        //delete the temp file
                        File.Delete(temp_destination + "\\go_" + directoryName.Name + ".zip");
                    }
                    else
                    {
                        ZipFile.CreateFromDirectory(d, destination + "\\go_" + directoryName.Name + ".zip");
                        //todo:encrypt
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Go file error: " + ex);
                }
            }

            foreach (var d in repoDirectories)
            {
                var directoryName = new DirectoryInfo(d);
                try
                {
                    var tempFileLocation = temp_destination + "\\repo_" + directoryName.Name + ".zip";

                    var backupFileLocation = destination + "\\backup\\repo_" + directoryName.Name + "-" + DateTime.Now.ToString("yyyyMMddHHmmssFFFF") +
                            ".zip";
                    var hasFileBeenHistoricalBackedupBefore = File.Exists(backupFileLocation);

                    var recentBackupFileLocation = destination + "\\repo_" + directoryName.Name + ".zip";
                    var hasFileBeenRecentBackedupBefore = File.Exists(recentBackupFileLocation);

                    if (savedZips.Any(z => z.Contains(directoryName.Name + ".zip")))
                    {
                        
                        ZipFile.CreateFromDirectory(d, tempFileLocation);
                        var tempFile = new FileInfo(tempFileLocation);
                        var format = "X2";
                        var sha256Temp = new SHA256Managed();
                        var shaHashForTempFile = "";
                        ChecksumEncoder.Encoder.EncodeFile<SHA256>(sha256Temp,tempFile,out shaHashForTempFile, format);

                        if (hasFileBeenRecentBackedupBefore)
                        {
                            var shaHashForRecent = "";
                            var sha256Recent = new SHA256Managed();
                            var recentFile = new FileInfo(recentBackupFileLocation);
                            ChecksumEncoder.Encoder.EncodeFile<SHA256>(sha256Recent, recentFile, out shaHashForRecent, format);
                            if ((shaHashForTempFile == shaHashForRecent) ? false : true)
                            {
                                File.Copy(recentBackupFileLocation, backupFileLocation);
                                File.Copy(tempFileLocation,recentBackupFileLocation, true);
                            }
                        }

                        File.Delete(tempFileLocation);
                    }
                    else
                    {
                        ZipFile.CreateFromDirectory(d, recentBackupFileLocation);

                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Repo file error: " + ex);
                }

            }
        }
    }
}
