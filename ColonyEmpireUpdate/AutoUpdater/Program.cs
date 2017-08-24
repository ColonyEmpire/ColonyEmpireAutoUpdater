using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace AutoUpdater
{
    class Program
    {
        #region Constants

        private const string GameFileName = "colonyclient.exe";
        private const string VersionFileName = "ceupdate.version";
        private const string TestWebServiceSiteVersionCheck = "http://cronee-001-site1.btempurl.com/api/Version";
        private const string TestWebServiceSiteDownload = "http://cronee-001-site1.btempurl.com/api/Version/Download";

        #endregion
        static void Main(string[] args)
        {
            string version = "0";
            //Check that the file is in the right place
            if (File.Exists(GameFileName))
            {
                Console.WriteLine("Found game files");
            }
            else
            {
                Console.WriteLine("Unable to find colonysurvival.exe, make sure the AutoUpdate.exe is in the same folder as your colonysurvival.exe file.");
                End(0);
            }

            //Get existing mod version
            if (File.Exists(VersionFileName))
            {
                try
                {
                    version = File.ReadAllText(VersionFileName);
                    Console.WriteLine($"Current version: {version}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    End(0xA0);
                }

            }
            else
            {
                Console.WriteLine("Unable to find version file, forcing update");
            }

            //Send request to update web service to check newest release version

            var newestVersion = GetNewestVersion(TestWebServiceSiteVersionCheck);
            int count = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Console.Write("Checking newest version available");
            while (!newestVersion.IsCompleted && watch.ElapsedMilliseconds < 20000)
            {
                Console.Write(".");
                Thread.Sleep(50);
                count++;
            }
            Console.WriteLine();
            watch.Stop();

            if (newestVersion.Status == TaskStatus.RanToCompletion)
            {
                Console.WriteLine($"Version fetched in {watch.Elapsed.TotalMilliseconds}ms");
                Console.WriteLine($"Newest version available: {newestVersion.Result.Replace("\"", "")}");
            }


            //Update if installed version is old
            int currentVersion = int.Parse(version);
            int newVersion = int.Parse(newestVersion.Result.Replace("\"", ""));

            if (currentVersion < newVersion)
            {
                //Todo: Update
                Console.WriteLine("Updating Colony Empire");
                DownloadFile();
                Console.WriteLine("Updating files");
                UpdateMod();
                Console.WriteLine("Update complete");
                UpdateVersionFile(newVersion);
                DeleteCEUpdateFile();
            }
            else if (currentVersion == newVersion)
            {
                Console.WriteLine("You have the newest version installed");
            }
            else
            {
                Console.WriteLine($"You have version {version} installed and the newest public release is {newVersion}.");
                Console.WriteLine($"If you wish to replace your current version then delete the {VersionFileName} file and then run the updater again.");
                Console.ReadLine();
            }
            End(0);
        }

        private static void DeleteCEUpdateFile()
        {
            File.Delete("ceupdate.zip");
        }

        private static void UpdateVersionFile(int newVersion)
        {
                File.WriteAllText(VersionFileName,newVersion.ToString());
        }

        private static void UpdateMod()
        {
            var zipHandler = new ZipHandler();
            zipHandler.ExtractZipFile("ceupdate.zip", Directory.GetCurrentDirectory());
        }

        private static void DownloadFile()
        {
            Console.Write("Downloading update: ");
            WebClient Client = new WebClient();

            ProgressBar progress = new ProgressBar();
            Client.DownloadProgressChanged += (sender, e) =>
            {
                progressChanged(e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage, progress);
            };


            try
            {

                Client.DownloadFileAsync(new Uri(TestWebServiceSiteDownload), "ceupdate.zip");

                while (Client.IsBusy)
                {
                    Thread.Sleep(20);
                }
                progress.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            Console.WriteLine("Download complete");

        }

        private static void progressChanged(long bytesReceived, long totalBytesToReceive, int progressPercentage, ProgressBar progress)
        {
            progress.Report((double)progressPercentage / 100);
        }

        private static async Task<string> GetNewestVersion(string path)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return "-1";
        }

        private static void End(int code)
        {
            for (int i = 5; i > 0; i--)
            {
                Console.WriteLine($"Closing in {i} seconds");
                Thread.Sleep(1000);
            }
            Environment.Exit(code);
        }
    }
}
