using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoUpdater
{
    class Program
    {
        #region Constants

        private const string GameFileName = "colonyclient.exe";

        #endregion
        static void Main(string[] args)
        {
            //Todo: Check that the file is in the right place
            if (Directory.GetFiles(Directory.GetCurrentDirectory()).Any(x => x.ToLower() == GameFileName))
            {
                Console.WriteLine("Found game files");
            }
            else
            {
                Console.WriteLine("Unable to find colonysurvival.exe, make sure the AutoUpdate.exe is in the same folder as your colonysurvival.exe file.");
                End();
            }

            //Todo: Get existing mod version


            //Todo: Send request to update web service to check newest release version


            //Todo: Update if installed version is old

        }

        private static void End()
        {
            for (int i = 10; i > 0; i--)
            {
                Console.WriteLine($"Closing in {i} seconds");
                Thread.Sleep(1000);
            }
            Environment.Exit(0);
        }
    }
}
