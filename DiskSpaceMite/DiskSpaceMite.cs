using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NanoMite
{
    public class DiskSpaceMite : Mite
    {
        protected override IEnumerable<string> GetHelpParametersCore()
        {
            var helpParameters = new Collection<string>
            {
                "-listdrives: Lists drive names to monitor for drives-to-monitor:name in appsettings.json."
            };

            return helpParameters;
        }

        protected override IEnumerable<string> HeartbeatCore()
        {
            var heartbeatItems = new Collection<string>();

            foreach (var drive in GetMonitoredDrives())
            {
                try
                {
                    if (drive.TotalSize > 0 && drive.TotalFreeSpace > 0 && drive.DriveType == DriveType.Fixed)
                    {
                        heartbeatItems.Add(Configuration["drive-heartbeat-html"].Value<string>()
                            .Replace("[DRIVE-NAME]", drive.Name).Replace("[DRIVE-FREE-SPACE]",
                                MiteHelper.GetSize(Convert.ToInt64(drive.TotalFreeSpace))));
                    }
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
            IEnumerable<string> checkupItems = new List<string>();

            checkupItems = GetInvalidDrives().Concat(GetDrivesWithLowDiskSpace());

            return checkupItems;
        }

        protected override bool CustomActionCore(string customAction)
        {
            var usedCustomAction = false;

            if (customAction == "-listdrives")
            {
                usedCustomAction = true;
                ListDrives();
            }

            return usedCustomAction;
        }

        private void ListDrives()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (drive.TotalSize > 0 && drive.TotalFreeSpace > 0 && drive.DriveType == DriveType.Fixed)
                    {
                        Console.WriteLine(Configuration["list-drive-console-text"].Value<string>()
                            .Replace("[DRIVE-NAME]", drive.Name).Replace("[DRIVE-FREE-SPACE]",
                                MiteHelper.GetSize(Convert.ToInt64(drive.TotalFreeSpace))));
                    }
                }
                catch
                {
                    //Eat Exception
                }
            }
        }

        private IEnumerable<DriveInfo> GetMonitoredDrives()
        {
            var drivesToMonitor = new Collection<DriveInfo>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                try
                {
                    var drivesToMonitorCount = Configuration["drives-to-monitor"].Count();

                    for (var i = 0; i < drivesToMonitorCount; i++)
                    {
                        if (drive.Name == Configuration["drives-to-monitor"][i]["name"].Value<string>())
                        {
                            drivesToMonitor.Add(drive);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return drivesToMonitor;
        }

        private IEnumerable<string> GetDrivesWithLowDiskSpace()
        {
            var lowSpaceDrives = new Collection<string>();

            // Drives Checkup
            foreach (var drive in GetMonitoredDrives())
            {
                try
                {
                    var drivesToMonitorCount = Configuration["drives-to-monitor"].Count();
                    for (var i = 0; i < drivesToMonitorCount; i++)
                    {
                        var driveName = Configuration[$"drives-to-monitor"][i]["name"].Value<string>();
                        var minSizeGb = Configuration[$"drives-to-monitor"][i]["min-size-gb"].Value<string>();
                        var minSizeGbException = Configuration["drive-below-min-exception-html"].Value<string>();

                        if (drive.Name == driveName)
                        {
                            if (drive.AvailableFreeSpace / 1000000000 < double.Parse(minSizeGb))
                            {
                                lowSpaceDrives.Add(minSizeGbException.Replace("[DRIVE-NAME]", driveName)
                                    .Replace("[DRIVE-FREE-SPACE]",
                                        MiteHelper.GetSize(Convert.ToInt64(drive.TotalFreeSpace)))
                                    .Replace("[DRIVE-MIN-FREE-SPACE]", minSizeGb));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return lowSpaceDrives;
        }

        private IEnumerable<string> GetInvalidDrives()
        {
            var invalidDrives = new Collection<string>();

            var drivesToMonitorCount = Configuration["drives-to-monitor"].Count();
            for (var i = 0; i < drivesToMonitorCount; i++)
            {
                var verified = false;
                
                var driveName = Configuration[$"drives-to-monitor"][i]["name"].Value<string>();
                
                foreach (var drive in DriveInfo.GetDrives())
                {
                    try
                    {
                        if (drive.Name == driveName)
                        {
                            verified = true;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                if (!verified)
                {
                    invalidDrives.Add(Configuration["drive-not-found-exception-html"].Value<string>()
                        .Replace("[DRIVE-NAME]", driveName));
                }
            }

            return invalidDrives;
        }
    }
}