using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace CEUpdaterMod
{

    [ModLoader.ModManager]
    public static class ModEntries
    {

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, "ceupdater.assemblyload")]
        public static void OnAssemblyLoaded(string path)
        {
            try
            {

                //Check that SharpZipLib is available
                if (!File.Exists("ICSharpCode.SharpZipLib.dll"))
                    DownloadSharpZipLib();

                //GetNewestVersion existing mod version
                var version = GetCurrentVersion();
                //Send request to update web service to check newest release version
                string newestVersion = GetNewestVersion();

                //Update if installed version is old


                int currentVersion = 0;
                int.TryParse(version, out currentVersion);
                var newVersion = 9999;
                int.TryParse(newestVersion.Replace("\"", ""), out newVersion);

                if (currentVersion < newVersion)
                {
                    PerformUpdate(newVersion);
                }
                else if (currentVersion == newVersion)
                {
                    Pipliz.Log.Write("ColonyEmpire is up to date");
                }
                else
                {
                    Pipliz.Log.Write(
                        $"You have version {version} installed and the newest public release is {newVersion}.");
                    Pipliz.Log.Write(
                        $"If you wish to replace your current version then delete the {VersionFileName} file and then run the updater again.");
                }

            }
            catch (Exception e)
            {
                Pipliz.Log.Write(e.Message);
                Pipliz.Log.WriteException(e);
                throw e;
            }
        }

        private static void DownloadSharpZipLib()
        {
            var client = new WebClient();
            client.DownloadFile(new Uri("https://github.com/icsharpcode/SharpZipLib/releases/download/0.86.0.518/ICSharpCode.SharpZipLib.dll"), "ICSharpCode.SharpZipLib.dll");
        }

        private static void PerformUpdate(int newVersion)
        {
            Pipliz.Log.Write("Updating Colony Empire");
            DownloadFile();
            UpdateMod();
            Pipliz.Log.Write("Update complete");
            UpdateVersionFile(newVersion);
            DeleteCeUpdateFile();
        }

        private static string GetNewestVersion()
        {
            var newestVersion = GetNewestVersion(TestWebServiceSiteVersionCheck);
            var watch = new Stopwatch();
            watch.Start();
            while (string.IsNullOrEmpty(newestVersion) && watch.ElapsedMilliseconds < 20000)
            {
                Thread.Sleep(50);
            }
            watch.Stop();
            Pipliz.Log.Write($"Newest version available: {newestVersion.Replace("\"", "")}");
            

            return newestVersion;
        }

        private static string GetCurrentVersion()
        {
            if (File.Exists(VersionFileName))
                try
                {
                    var version = File.ReadAllText(VersionFileName);
                    return version;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return "0";
                }
            else
                Pipliz.Log.Write("Unable to find version file, forcing update");
            return "0";
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
            Pipliz.Log.Write("Downloading update... ");
            var client = new WebClient();


            try
            {
                var watch = new Stopwatch();
                watch.Start();
                client.DownloadFileAsync(new Uri(TestWebServiceSiteDownload), "ceupdate.zip");

                while (client.IsBusy)
                    Thread.Sleep(20);
                watch.Stop();
                Pipliz.Log.Write($"Download finished in {watch.Elapsed.ToString()}");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static string GetNewestVersion(string path)
        {
            var client = new WebClient();
            return client.DownloadString(path);
        }

        #region Constants

        private const string GameFileName = "colonyclient.exe";
        private const string VersionFileName = "ceupdate.version";
        private const string TestWebServiceSiteVersionCheck = "http://cronee-001-site1.btempurl.com/api/Version";
        private const string TestWebServiceSiteDownload = "http://cronee-001-site1.btempurl.com/api/Version/Download";

        #endregion

    }
}
