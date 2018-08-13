using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading;

namespace NewShell
{
    class WhoDis
    {
        static void Main(string[] args)
        {
            RunThisAsAdmin();
            new Thread(setupListener) { IsBackground = true, Name = "worker" }.Start();
            Console.WriteLine("New Shell. Who Dis?");
            do
            {
                Thread.Sleep(5000);
            } while (true);
        }

        static void setupListener()
        {
            var log = new EventLog("Security");
            log.EntryWritten += listener;
            log.EnableRaisingEvents = true;
        }

        static void listener(object sender, EntryWrittenEventArgs e)
        {
            try
            { 
                using (var reader = new StringReader(e.Entry.Message))
                {
                    var line = "";
                    var user = "";
                    var domain = "";
                    var summary = "";
                    do
                    {
                        line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        if (string.IsNullOrEmpty(summary))
                        {
                            summary = line;
                        }
                        var l = line.TrimStart().Trim();
                        if (string.IsNullOrEmpty(user) && l.ToLower().Contains("account name"))
                        {
                            var parts = l.Split('\t');
                            user = parts[parts.Length - 1];
                        }
                        if (string.IsNullOrEmpty(domain) && l.ToLower().Contains("account domain"))
                        {
                            var parts = l.Split('\t');
                            domain = parts[parts.Length - 1];
                        }

                    } while (line != null);
                    Console.WriteLine("+ {0} ({1}) {2}\\{3}", e.Entry.EventID, summary, domain, user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void RunThisAsAdmin()
        {
            if (!IsAdministrator())
            {
                var exe = Process.GetCurrentProcess().MainModule.FileName;
                var startInfo = new ProcessStartInfo(exe)
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Normal,
                    CreateNoWindow = false
                };
                Process.Start(startInfo);
                Process.GetCurrentProcess().Kill();
            }
        }
        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
