using System.Diagnostics;

namespace SpiritEye
{
    static class NMapHelper
    {
        static bool IsUserPriviledged()
        {
            return Environment.UserName == "root";
        }

        public static List<Process>? LaunchNMap(List<string> target, string ports)
        {
            if (!IsUserPriviledged())
            {
                Utils.Error("please run this program as root");
                return null;
            }

            List<Process> processes = new();

            foreach (var t in target)
            {
                Process process = new();
                process.StartInfo.FileName = "nmap";
                process.StartInfo.Arguments = " --stats-every 1s -sV --min-rate 2500 --max-rtt-timeout 500ms --open -T4 -p" + ports + " -oX - " + t;
                // process.StartInfo.Arguments = " --stats-every 5s -sV --min-rate 500 --max-rtt-timeout 1s --open -T4 -p- -oX - " + t;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                processes.Add(process);
            }

            Utils.Info("nmap launched");
            return processes;
        }
    }
}

