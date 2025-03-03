using System;
using System.IO;
using Microsoft.Win32;
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
                if (process != null
                    && (process.Id > 8) // A process with an id of 8 or lower is a system process, we shouldn't be trying to access those.
                    && (process.MainWindowHandle != IntPtr.Zero))
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
            List<Process> returnList = new List<Process>();
            Process[] processList = Process.GetProcessesByName(filter);

            foreach (Process process in processList)
            {
                if (IsValidProcess(process))
                {
                    if (process.ProcessName.Contains(filter) || process.MainWindowTitle.Contains(filter))
                    {
                        returnList.Add(process);
                    }
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
                    Write("Found launcher running, attempting to close \"" + launcher.Id.ToString() + "\"...");

                    try
                    {
                        launcher.Kill();
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

            if (Directory.Exists(tempFolder))
            {
                string newLauncher = (tempFolder + "\\CodeRedLauncher.exe");
                string launcherPath = (tempFolder + "\\LauncherPath.txt");

                if (File.Exists(newLauncher))
                {
                    if (File.Exists(launcherPath))
                    {
                        string oldLauncher = File.ReadAllText(launcherPath);

                        if (File.Exists(oldLauncher))
                        {
                            if (CloseLauncher())
                            {
                                Write("Deleting old launcher \"" + oldLauncher + "\"...");

                                try
                                {
                                     File.Delete(oldLauncher);
                                }
                                catch (Exception ex)
                                {
                                    Write("Failed to delete old launcher, either missing permissions or being blocked by antivirus!");
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
                                    Console.ReadKey();
                                    return;
                                }

                                if (File.Exists(oldLauncher))
                                {
                                    Write("Successfully installed the new launcher, attempting to open...");
                                    Process.Start(new ProcessStartInfo(oldLauncher) { UseShellExecute = false });
                                    Environment.Exit(0);
                                }
                                else
                                {
                                    Write("Failed to overwrite old launcher with new one!");
                                    Console.ReadKey();
                                }
                            }
                            else
                            {
                                Write("Failed to close the launcher, cannot overwrite with the new one!");
                                Console.ReadKey();
                            }
                        }
                        else
                        {
                            Write("Failed to locate old launcher to replace, cannot install new one!");
                            Console.ReadKey();
                        }
                    }
                    else
                    {
                        Write("Failed to locate old launchers install path, cannot install new one!");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Write("Failed to find newly downloaded launcher, cannot install!");
                    Console.ReadKey();
                }
            }
            else
            {
                Write("Failed to find temporary folder, cannot install launcher!");
                Console.ReadKey();
            }
        }
    }
}