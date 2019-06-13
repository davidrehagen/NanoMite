using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using NickStrupat;

namespace NanoMite
{
    public class SystemMemoryMite: Mite
    {
        protected override IEnumerable<string> CheckupCore()
        {
            var checkupItems = new Collection<string>();
            
            var computerInfo = new ComputerInfo();
            
            if (computerInfo.AvailablePhysicalMemory / 1000000000 <
                double.Parse(Configuration[$"system-memory"]["min-size-gb"].Value<string>()))
            {
                checkupItems.Add(Configuration["system-memory-below-min-exception-html"].Value<string>()
                    .Replace("[COMPUTER-NAME]", Environment.MachineName).Replace(
                        "[COMPUTER-MEMORY-FREE]",
                        MiteHelper.GetSize((long)computerInfo.AvailablePhysicalMemory))
                    .Replace("[COMPUTER-MIN-MEMORY-FREE]",
                        Configuration[$"system-memory"]["min-size-gb"].Value<string>()));                                                       
            }

            return checkupItems;
        }

        protected override IEnumerable<string> HeartbeatCore()
        {
            var heartbeatItems = new Collection<string>();
            
            var computerInfo = new ComputerInfo();

            heartbeatItems.Add(Configuration["system-memory-heartbeat-html"].Value<string>()
                .Replace("[COMPUTER-NAME]", Environment.MachineName).Replace(
                    "[COMPUTER-MEMORY-FREE]",
                    MiteHelper.GetSize((long)computerInfo.AvailablePhysicalMemory)));                                                   

            return heartbeatItems;
        }
    }
}