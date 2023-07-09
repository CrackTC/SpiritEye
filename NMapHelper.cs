using System.Diagnostics;

namespace SpiritEye
{
    static class NMapHelper
    {
        static bool IsUserPriviledged()
        {
            return Environment.UserName == "root";
        }

        public static Process? LaunchNMap(string target)
        {
            if (!IsUserPriviledged())
            {
                Utils.Error("please run this program as root");
                return null;
            }

            Process process = new();
            process.StartInfo.FileName = "nmap";
            process.StartInfo.Arguments = " --stats-every 3s -sV --open -oX - " + target;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            Utils.Info("nmap launched");
            return process;
        }
    }
}

