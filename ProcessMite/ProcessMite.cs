using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NanoMite
{
    public class ProcessMite : Mite
    {
        protected override IEnumerable<string> GetHelpParametersCore()
        {
            var helpParameters = new Collection<string>
            {
                "-listprocesses: Lists process names to monitor for drives-to-monitor:name in appsettings.json."
            };

            return helpParameters;
        }

        protected override IEnumerable<string> HeartbeatCore()
        {
            var heartbeatItems = new Collection<string>();

            foreach (var process in GetMonitoredProcesses())
            {
                try
                {
                    heartbeatItems.Add(Configuration["process-heartbeat-html"].Value<string>()
                        .Replace("[PROCESS-NAME]", process.ProcessName)
                        .Replace("[PROCESS-MEMORY]", MiteHelper.GetSize(process.WorkingSet64)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return heartbeatItems;
        }

        protected override IEnumerable<string> CheckupCore()
        {
            var checkupItems = new Collection<string>();

            var processesToMonitor = Configuration["processes-to-monitor"].Count();

            for (var i = 0; i < processesToMonitor; i++)
            {
                var processName = Configuration["processes-to-monitor"][i]["name"].Value<string>();
                var maxSizeMb = Configuration["processes-to-monitor"][i]["max-size-mb"].Value<string>();
                var processAboveMaxException = Configuration["process-above-max-exception-html"].Value<string>();
                var processNotFoundException = Configuration["process-not-found-exception-html"].Value<string>();

                var processes = Process.GetProcessesByName(processName);

                foreach (var process in processes)
                {
                    if ((process.WorkingSet64 / 1000000) > double.Parse(maxSizeMb))
                    {
                        checkupItems.Add(processAboveMaxException.Replace("[PROCESS-NAME]", process.ProcessName)
                            .Replace("[PROCESS-MEMORY]", MiteHelper.GetSize(Convert.ToInt64(process.WorkingSet64)))
                            .Replace("[PROCESS-MAX-MEMORY]", maxSizeMb));
                    }
                }

                if (processes.Length == 0)
                    checkupItems.Add(processNotFoundException.Replace("[PROCESS-NAME]", processName));
            }

            return checkupItems;
        }

        protected override bool CustomActionCore(string customAction)
        {
            var usedCustomAction = false;

            if (customAction == "-listprocesses")
            {
                usedCustomAction = true;
                var processes = Process.GetProcesses();

                foreach (var process in processes)
                {
                    Console.WriteLine(Configuration["list-process-text"].Value<string>()
                        .Replace("[PROCESS-NAME]", process.ProcessName)
                        .Replace("[PROCESS-MEMORY]", MiteHelper.GetSize(process.WorkingSet64)));
                }
            }

            return usedCustomAction;
        }

        private IEnumerable<Process> GetMonitoredProcesses()
        {
            var processesToMonitor = new Collection<Process>();

            try
            {
                var processesToMonitorCount = Configuration["processes-to-monitor"].Count();

                for (var i = 0; i < processesToMonitorCount; i++)
                {
                    var processes =
                        Process.GetProcessesByName(Configuration["processes-to-monitor"][i]["name"].Value<string>());
                    foreach (var process in processes)
                    {
                        processesToMonitor.Add(process);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return processesToMonitor;
        }
    }
}