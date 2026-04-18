using Mikrotik.SwOSLite.Monitor.Models;
using Mikrotik.SwOSLite.Monitor.Services;
using Newtonsoft.Json;

namespace Mikrotik.SwOSLite.Monitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== MikroTik SNMP Data Collector ===\n");

            string ipAddress = "192.168.150.94";
            string community = "public";

            if (args.Length > 0)
            {
                ipAddress = args[0];
            }
            if (args.Length > 1)
            {
                community = args[1];
            }

            Console.WriteLine($"Target Device: {ipAddress}");
            Console.WriteLine($"SNMP Community: {community}");
            Console.WriteLine($"Port: 161\n");

            MikroTikSNMPService mikroTikSNMPService = new MikroTikSNMPService(ipAddress, community);

            try
            {
                Dictionary<string, object> results = new Dictionary<string, object>();

                DeviceInfo deviceInfo = mikroTikSNMPService.GetDeviceInfo();
                PrintDeviceInfo(deviceInfo);

                IList<InterfaceInfo> interfaces = mikroTikSNMPService.GetInterfaces();
                PrintInterfaces(interfaces);

                IList<SwitchHost> swithHost = mikroTikSNMPService.GetSwitchHosts();
                PrintSwitchHost(swithHost);

                IList<EthernetStatsInfo> ethernetStats = mikroTikSNMPService.GetEthernetStats();
                PrintEthernetStats(ethernetStats);

                IList<SFPModuleInfo> sfpModuleInfos = mikroTikSNMPService.GetSFPModules();
                PrintSfp(sfpModuleInfos);

                STPInfo stpInfo = mikroTikSNMPService.GetSTPInfo();
                PrintSTPStatus(stpInfo);

                IDictionary<string, string> health = mikroTikSNMPService.GetHealthStatus();
                PrintHealthStatus(health);

                Console.WriteLine("\n=== Collection Complete ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static void PrintDeviceInfo(DeviceInfo device)
        {
            PrintSection("Device Information");

            PrintRow("System Name", device.SystemName ?? string.Empty);
            PrintRow("Description", device.SystemDescription ?? string.Empty);
            PrintRow("Location", device.SystemLocation ?? string.Empty);
            PrintRow("Contact", device.SystemContact ?? string.Empty);
            PrintRow("Uptime", device.SystemUpTime ?? string.Empty);
            PrintRow("Serial Number", device.SerialNumber ?? string.Empty);
            PrintRow("Firmware", device.FirmwareVersion ?? string.Empty);

            PrintFooter();
        }

        private static void PrintInterfaces(IList<InterfaceInfo> interfaces)
        {
            PrintSection("Network Interfaces");
            PrintRow("Total Interfaces", interfaces.Count.ToString());

            foreach (InterfaceInfo intf in interfaces)
            {
                PrintSeparator();
                PrintRow($"[{intf.Index}] {intf.Name}", "");
                PrintRow("Type", intf.Type ?? string.Empty);
                PrintRow("Speed", FormatSpeed(intf.Speed));
                PrintRow("MAC", intf.PhysAddress ?? string.Empty);
                PrintRow("Admin/Oper Status", $"{intf.AdminStatus} | {intf.OperStatus}");
                PrintRow("RX/TX", $"{FormatBytes(intf.InOctets)} | {FormatBytes(intf.OutOctets)}");
                PrintRow("Errors (In/Out)", $"{intf.InErrors}/{intf.OutErrors}");

                if (!string.IsNullOrEmpty(intf.Description))
                {
                    PrintRow("Description", intf.Description);
                }
            }

            PrintFooter();
        }

        private static void PrintSwitchHost(IList<SwitchHost> switchHosts)
        {
            PrintSection("Switch Hosts");
            PrintRow("Total Entries", switchHosts.Count.ToString());

            foreach (SwitchHost switchHost in switchHosts)
            {
                PrintSeparator();
                PrintRow(switchHost.Port.ToString(), switchHost.MACAddress ?? string.Empty);
            }

            PrintFooter();
        }

        private static void PrintEthernetStats(IList<EthernetStatsInfo> ethernetStatsInfos)
        {
            PrintSection("Ethernet Statistics");
            PrintRow("Total Entries", ethernetStatsInfos.Count.ToString());

            foreach (EthernetStatsInfo ethernetStatsInfo in ethernetStatsInfos)
            {
                PrintSeparator();
                PrintRow(ethernetStatsInfo.InterfaceName ?? string.Empty, "");

                string statsLine = $"BCst R/T {ethernetStatsInfo.InBroadcastPkts:N0}/{ethernetStatsInfo.OutBroadcastPkts:N0} | " +
                                  $"MCst R/T {ethernetStatsInfo.InMulticastPkts:N0}/{ethernetStatsInfo.OutMulticastPkts:N0} | " +
                                  $"Col SC/MC/LC/FSCE/AE/DT {ethernetStatsInfo.SingleCollisions}/{ethernetStatsInfo.MultipleCollisions}/" +
                                  $"{ethernetStatsInfo.LateCollisions}/{ethernetStatsInfo.FCSErrors}/{ethernetStatsInfo.AlignmentErrors}/{ethernetStatsInfo.DeferredTransmissions}";

                PrintRow("Stats", statsLine);
            }

            PrintFooter();
        }


        private static void PrintSfp(IList<SFPModuleInfo> sfpModuleInfos)
        {
            PrintSection("SFP Statistics");
            PrintRow("Total SFP", sfpModuleInfos.Count.ToString());

            foreach (SFPModuleInfo sfpModuleInfo in sfpModuleInfos)
            {
                PrintSeparator();
                PrintRow(sfpModuleInfo.Name ?? string.Empty, "");

                string sfpStats = $"RxL {sfpModuleInfo.RxLoss} | TxF {sfpModuleInfo.TxFault} | W {sfpModuleInfo.Wavelength} | " +
                                 $"T {sfpModuleInfo.Temperature} | SV {sfpModuleInfo.SupplyVoltage} | " +
                                 $"TxC {sfpModuleInfo.TxCurrent} | TxP {sfpModuleInfo.TxPower} | RxP {sfpModuleInfo.RxPower}";

                PrintRow("Stats", sfpStats);
            }

            PrintFooter();
        }

        private static void PrintHealthStatus(IDictionary<string, string> health)
        {
            PrintSection("Health Status");

            foreach (KeyValuePair<string, string> kvp in health)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    PrintRow(kvp.Key, kvp.Value);
                }
            }

            PrintFooter();
        }

        private static void PrintSTPStatus(STPInfo stpInfo)
        {
            PrintSection("STP Information");

            PrintRow("Max Age", stpInfo.MaxAge.ToString());
            PrintRow("Forward Delay", stpInfo.ForwardDelay.ToString());
            PrintRow("Topology Changes", stpInfo.TopologyChanges.ToString());
            PrintRow("Time Since TC", stpInfo.TimeSinceTopologyChange.ToString());
            PrintRow("Root Port", stpInfo.RootPort.ToString());
            PrintRow("Root Cost", stpInfo.RootCost.ToString());
            PrintRow("Designated Root", stpInfo.DesignatedRoot ?? string.Empty);
            PrintRow("Hello Time", stpInfo.HelloTime.ToString());
            PrintRow("Protocol Spec", stpInfo.ProtocolSpecification ?? string.Empty);
            PrintRow("Priority", stpInfo.Priority.ToString("X4"));

            PrintFooter();
        }

        private static void PrintSection(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  ┌─ {title} {"─".PadRight(62 - title.Length, '─')}┐");
            Console.ResetColor();
        }

        private static void PrintRow(string label, string value)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"  │  {label,-28}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{value}");
            Console.ResetColor();
        }

        private static void PrintSeparator()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  │  " + "─".PadRight(50, '─'));
            Console.ResetColor();
        }

        private static void PrintFooter()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine();
            Console.WriteLine($"  Polled at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.ResetColor();
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        private static string FormatSpeed(long bps)
        {
            if (bps == 0)
            {
                return "N/A";
            }

            if (bps >= 1000000000)
            {
                return $"{bps / 1000000000} Gbps";
            }

            if (bps >= 1000000)
            {
                return $"{bps / 1000000} Mbps";
            }

            if (bps >= 1000)
            {
                return $"{bps / 1000} Kbps";
            }

            return $"{bps} bps";
        }
    }
}
