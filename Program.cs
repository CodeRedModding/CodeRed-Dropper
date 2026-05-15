using System;
using System.IO;
using System.Diagnostics;

namespace CodeRedDropper
{
    internal class Program
    {
        static void Write(string str)
        {
            Console.WriteLine("[" + DateTime.Now.ToString() + "] " + str);
        }

        static bool IsValidProcess(Process process)
        {
            try
            {
                if ((process != null) && (process.Id > 8)) // A process with an id of 8 or lower is a system process, we shouldn't be trying to access those.
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Write("(IsValidProcess) Exception: " + ex.ToString());
            }

            return false;
        }

        static List<Process> GetFilteredProcesses(string filter)
        {
            List<Process> returnList = new();

            foreach (Process process in Process.GetProcessesByName(filter))
            {
                if (IsValidProcess(process))
                {
                    returnList.Add(process);
                }
            }

            return returnList;
        }

        // Launcher should already be closed by this point, so this is just for the sake of sanity checking.
        static bool CloseLauncher()
        {
            List<Process> launchers = GetFilteredProcesses("CodeRedLauncher");

            foreach (Process launcher in launchers)
            {
                if (IsValidProcess(launcher))
                {
                    Write("(CloseLauncher) Found launcher running, attempting to close \"" + launcher.Id.ToString() + "\"...");

                    try
                    {
                        launcher.Kill();

                        if (!launcher.WaitForExit(5000))
                        {
                            Write("(CloseLauncher) Failed to close launcher, timeout treshhold reached!");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Write("(CloseLauncher) Exception: " + ex.ToString());
                        return false;
                    }
                }
            }

            return true;
        }

        static void Main(string[] args)
        {
            string tempFolder = (Path.GetTempPath() + "\\CodeRedLauncher");
            string newLauncher = (tempFolder + "\\CodeRedLauncher.exe");
            string launcherPath = (tempFolder + "\\LauncherPath.txt");

            if (!Directory.Exists(tempFolder))
            {
                Write("Failed to find temporary folder, cannot install launcher!");
                Console.ReadKey();
                return;
            }

            if (!File.Exists(newLauncher))
            {
                Write("Failed to find newly downloaded launcher, cannot install!");
                Console.ReadKey();
                return;
            }

            if (!File.Exists(launcherPath))
            {
                Write("Failed to find old launchers install path, cannot install new one!");
                Console.ReadKey();
                return;
            }

            string oldLauncher = File.ReadAllText(launcherPath).Trim();

            if (!File.Exists(oldLauncher))
            {
                Write("Failed to locate old launcher to replace, cannot install new one!");
                Console.ReadKey();
                return;
            }

            if (!CloseLauncher())
            {
                Write("Failed to close the launcher, cannot overwrite with the new one!");
                Console.ReadKey();
                return;
            }

            Write("Deleting old launcher \"" + oldLauncher + "\"...");

            try
            {
                File.Delete(oldLauncher);
            }
            catch (Exception ex)
            {
                Write("Failed to delete old launcher, either missing permissions or being blocked by antivirus!");
                Write("Exception: " + ex.ToString());
                Console.ReadKey();
                return;
            }

            Write("Replacing with new launcher...");

            try
            {
                File.Move(newLauncher, oldLauncher, true);
            }
            catch (Exception ex)
            {
                Write("Failed to move extracted launcher, either missing permissions or being blocked by antivirus!");
                Write("Exception: " + ex.ToString());
                Console.ReadKey();
                return;
            }

            if (!File.Exists(oldLauncher))
            {
                Write("Failed to overwrite old launcher with new one, most likely being blocked by antivirus!");
                Console.ReadKey();
                return;
            }

            Write("Successfully installed the new launcher, attempting to open...");

            try
            {
                Process.Start(new ProcessStartInfo(oldLauncher) { UseShellExecute = false });
            }
            catch (Exception ex)
            {
                Write("Failed open the new launcher, most likely being blocked by antivirus!");
                Write("Exception: " + ex.ToString());
                Console.ReadKey();
            }
        }
    }
}