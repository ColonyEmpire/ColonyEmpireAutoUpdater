using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

namespace AutoUpdater
{
    internal class Program
    {
        private static void Main()
        {
            //Check that SharpZipLib is available
            if (!File.Exists("ICSharpCode.SharpZipLib.dll"))
                DownloadSharpZipLib();

            //Check that the file is in the right place
            CheckFileLocation();

            //GetNewestVersion existing mod version
            var version = GetCurrentVersion();

            //Send request to update web service to check newest release version
            Task<string> newestVersion = GetNewestVersion();


            //Update if installed version is old
            var currentVersion = int.Parse(version);
            var newVersion = int.Parse(newestVersion.Result.Replace("\"", ""));

            if (currentVersion < newVersion)
            {
                PerformUpdate(newVersion);
            }
            else if (currentVersion == newVersion)
            {
                Console.WriteLine("You have the newest version installed");
            }
            else
            {
                Console.WriteLine(
                    $"You have version {version} installed and the newest public release is {newVersion}.");
                Console.WriteLine(
                    $"If you wish to replace your current version then delete the {VersionFileName} file and then run the updater again.");
                Console.ReadLine();
            }
            End(0);
        }

        private static void DownloadSharpZipLib()
        {
            var client = new WebClient();
            client.DownloadFile(new Uri("https://github.com/icsharpcode/SharpZipLib/releases/download/0.86.0.518/ICSharpCode.SharpZipLib.dll"), "ICSharpCode.SharpZipLib.dll");
        }

        private static void PerformUpdate(int newVersion)
        {
            Console.WriteLine("Updating Colony Empire");
            DownloadFile();
            Console.WriteLine("Updating files");
            UpdateMod();
            Console.WriteLine("Update complete");
            UpdateVersionFile(newVersion);
            DeleteCeUpdateFile();
        }

        private static Task<string> GetNewestVersion()
        {
            var newestVersion = GetNewestVersion(TestWebServiceSiteVersionCheck);
            var watch = new Stopwatch();
            watch.Start();
            Console.Write("Checking newest version available");
            while (!newestVersion.IsCompleted && watch.ElapsedMilliseconds < 20000)
            {
                Console.Write(".");
                Thread.Sleep(50);
            }
            Console.WriteLine();
            watch.Stop();

            if (newestVersion.Status == TaskStatus.RanToCompletion)
            {
                Console.WriteLine($"Version fetched in {watch.Elapsed.TotalMilliseconds}ms");
                Console.WriteLine($"Newest version available: {newestVersion.Result.Replace("\"", "")}");
            }

            return newestVersion;
        }

        private static string GetCurrentVersion()
        {
            if (File.Exists(VersionFileName))
                try
                {
                    var version = File.ReadAllText(VersionFileName);
                    Console.WriteLine($"Current version: {version}");
                    return version;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    End(0xA0);
                }
            else
                Console.WriteLine("Unable to find version file, forcing update");
            return "0";
        }

        private static void CheckFileLocation()
        {
            if (File.Exists(GameFileName))
            {
                Console.WriteLine("Found game files");
            }
            else
            {
                Console.WriteLine(
                    "Unable to find colonysurvival.exe, make sure the AutoUpdate.exe is in the same folder as your colonysurvival.exe file.");
                End(0);
            }
        }

        private static void DeleteCeUpdateFile()
        {
            File.Delete("ceupdate.zip");
        }

        private static void UpdateVersionFile(int newVersion)
        {
            File.WriteAllText(VersionFileName, newVersion.ToString());
        }

        private static void UpdateMod()
        {
            var zipHandler = new ZipHandler();
            zipHandler.ExtractZipFile("ceupdate.zip", Directory.GetCurrentDirectory());
        }

        private static void DownloadFile()
        {
            Console.Write("Downloading update: ");
            var client = new WebClient();

            var progress = new ProgressBar();
            client.DownloadProgressChanged += (sender, e) => ProgressChanged(e.ProgressPercentage, progress);
            client.DownloadFileCompleted += (sender, e) => progress.Dispose();


            try
            {
                client.DownloadFileAsync(new Uri(TestWebServiceSiteDownload), "ceupdate.zip");

                while (client.IsBusy)
                    Thread.Sleep(20);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            Console.WriteLine("Download complete");
        }

        private static void ProgressChanged(int progressPercentage, ProgressBar progress)
        {
            progress.Report((double) progressPercentage / 100);
        }

        private static async Task<string> GetNewestVersion(string path)
        {
            var client = new WebClient();
            return client.DownloadString(path);
        }

        private static void End(int code)
        {
            for (var i = 5; i > 0; i--)
            {
                Console.WriteLine($"Closing in {i} seconds");
                Thread.Sleep(1000);
            }
            Environment.Exit(code);
        }

        #region Constants

        private const string GameFileName = "colonyclient.exe";
        private const string VersionFileName = "ceupdate.version";
        private const string TestWebServiceSiteVersionCheck = "http://cronee-001-site1.btempurl.com/api/Version";
        private const string TestWebServiceSiteDownload = "http://cronee-001-site1.btempurl.com/api/Version/Download";

        #endregion
    }
}