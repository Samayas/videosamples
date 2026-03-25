using Lextm.SharpSnmpLib;
using VigorSNMP.Model;
using VigorSNMP.Services;

namespace VigorSNMP
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            //   VigorSnmpService vigorSNMPService = new vigorSNMPService("192.168.149.1","public", 161, VersionCode.V1);
               VigorSnmpService vigorSNMPService = new VigorSnmpService("192.168.149.1", "public", 161, VersionCode.V2);
            // VigorSnmpService vigorSNMPService = new VigorSnmpService("192.168.149.1", new SnmpV3Credentials("user", "12345678", SnmpV3AuthProtocol.SHA1, "12345678", SnmpV3PrivProtocol.AES128), 161);

            try
            {
            //    Console.WriteLine(vigorSNMPService.DiagnoseV3Combinations());
            //    Console.Write(vigorSNMPService.DiagnoseV3Connection());

                SystemDeviceInfo sys = await vigorSNMPService.GetSystemInfoAsync();

                Dictionary<int, string> ifMap = await vigorSNMPService.GetInterfaceMapAsync();

                int lanIfIndex = await vigorSNMPService.FindLanBridgeIfIndexAsync();
                int vdsl2IfIndex = await vigorSNMPService.DiscoverVdsl2IfIndexAsync();

                Console.WriteLine($" VDSL2 MIB ifIndex    = {vdsl2IfIndex}");

                Vdsl2LineStatusInfo line;
                Vdsl2ChannelInfo ch;
                Vdsl2Performance15MinInfo perf = new Vdsl2Performance15MinInfo();

                if (vdsl2IfIndex != -1)
                {
                    line = await vigorSNMPService.GetVdsl2LineStatusAsync(vdsl2IfIndex);
                    ch = await vigorSNMPService.GetVdsl2ChannelAsync(vdsl2IfIndex);
                    perf = await vigorSNMPService.GetVdsl2PerformanceAsync(vdsl2IfIndex);
                }
                else
                {
                    Console.WriteLine(" [INFO] RFC 5650 absent — using RFC 2662 ADSL-LINE-MIB.");
                    line = await vigorSNMPService.GetAdslLineStatusAsync();
                    ch = await vigorSNMPService.GetAdslChannelAsync();
                }

                LanInterfaceInfo lan = await vigorSNMPService.GetLanInterfaceAsync(lanIfIndex);

                PrintSection("SYSTEM & DEVICE");
                PrintRow("Model", sys.RouterModel);
                PrintRow("Firmware", sys.FirmwareRevision);
                PrintRow("Build date", sys.FirmwareBuildDate);
                PrintRow("DSL chipset", sys.DslChipsetVersion);
                PrintRow("System name", sys.SystemName);
                PrintRow("System description", sys.SystemDescription);
                PrintRow("Contact", sys.SystemContact);
                PrintRow("Location", sys.SystemLocation);
                PrintRow("Uptime", sys.UptimeFormatted);
                PrintRow("Memory usage", $"{sys.MemoryUsagePercent} %");
                PrintRow("LAN MAC", sys.LanMacAddress);

                PrintSection("VDSL2 LINE STATUS");
                if (!string.IsNullOrEmpty(line.LineStatusCo))
                    PrintRow("CO  line status", line.LineStatusCo);
                if (!string.IsNullOrEmpty(line.LineStatusCpe))
                    PrintRow("CPE line status", line.LineStatusCpe);
                PrintRow("SNR margin DS", $"{line.SnrMarginDsDb:F1} dB");
                PrintRow("SNR margin US", $"{line.SnrMarginUsDb:F1} dB");
                PrintRow("Attenuation DS", $"{line.AttenuationDsDb:F1} dB");
                PrintRow("Attenuation US", $"{line.AttenuationUsDb:F1} dB");
                if (line.AttainableDsRateBps > 0)
                    PrintRow("Attainable DS", $"{line.AttainableDsRateMbps:F2} Mbps");
                if (line.AttainableUsRateBps > 0)
                    PrintRow("Attainable US", $"{line.AttainableUsRateMbps:F2} Mbps");
                PrintRow("Active profile", line.DisplayProfile);
                if (!string.IsNullOrEmpty(line.AdslAnnex))
                    PrintRow("Annex", line.AdslAnnex);
                PrintRow("Init result", line.InitResultText);
                PrintRow("Power state", line.PowerManagementStateText);
                PrintRow("Last state DS", line.LastStateDs.ToString());
                PrintRow("Last state US", line.LastStateUs.ToString());
                PrintRow("Act trans. system", line.ActTransmissionSystem.ToString());
                PrintRow("Act mode", line.ActMode.ToString());
                PrintRow("CO  defects", FormatDefects(line.CoHasLos, line.CoHasLof, line.CoHasLpr, line.CoDefects));
                PrintRow("CPE defects", FormatDefects(line.CpeHasLos, line.CpeHasLof, line.CpeHasLpr, line.CpeDefects));

                PrintSection("VDSL2 CHANNEL");
                PrintRow("Current DS rate", $"{ch.CurrentDsRateMbps:F2} Mbps  ({ch.CurrentDsRateBps:N0} bps)");
                PrintRow("Current US rate", $"{ch.CurrentUsRateMbps:F2} Mbps  ({ch.CurrentUsRateBps:N0} bps)");
                PrintRow("Prev DS rate", $"{ch.PrevDsRateMbps:F2} Mbps  ({ch.PrevDsRateBps:N0} bps)");
                PrintRow("Prev US rate", $"{ch.PrevUsRateMbps:F2} Mbps  ({ch.PrevUsRateBps:N0} bps)");
                PrintRow("DS rate delta", $"{ch.DsRateDeltaMbps:+0.00;-0.00;+0.00} Mbps since last retrain");
                PrintRow("US rate delta", $"{ch.UsRateDeltaMbps:+0.00;-0.00;+0.00} Mbps since last retrain");
                PrintRow("INP DS", $"{ch.ImpulseNoiseProtDs:F1} symbols");
                PrintRow("INP US", $"{ch.ImpulseNoiseProtUs:F1} symbols");
                PrintRow("Interleave delay", $"{ch.InterleaveDelayMs} ms");

                PrintSection("VDSL2 PERFORMANCE — last 15 min");
                PrintRow("Line stable", perf.IsLineStable ? "Yes" : "No  ← issues detected");
                PrintRow("CO  has errors", perf.HasCoErrors ? "Yes" : "No");
                PrintRow("CPE has errors", perf.HasCpeErrors ? "Yes" : "No");
                PrintSeparator();
                PrintRow("CO  errored secs", perf.CoErroredSecs.ToString());
                PrintRow("CO  severely errored secs", perf.CoSeverelyErroredSecs.ToString());
                PrintRow("CO  loss-of-signal secs", perf.CoLossOfSignalSecs.ToString());
                PrintRow("CO  unavailable secs", perf.CoUnavailableSecs.ToString());
                PrintSeparator();
                PrintRow("CPE errored secs", perf.CpeErroredSecs.ToString());
                PrintRow("CPE severely errored secs", perf.CpeSeverelyErroredSecs.ToString());
                PrintRow("CPE loss-of-signal secs", perf.CpeLossOfSignalSecs.ToString());
                PrintRow("CPE unavailable secs", perf.CpeUnavailableSecs.ToString());

                PrintSection($"LAN INTERFACE  (ifIndex={lan.IfIndex})");
                PrintRow("Description", lan.Description);
                PrintRow("Status", lan.OperStatusText);
                PrintRow("Speed", $"{lan.SpeedMbps:F0} Mbps");
                PrintRow("In  octets", $"{lan.InOctets:N0} bytes  ({lan.TotalInGb:F3} GB)");
                PrintRow("Out octets", $"{lan.OutOctets:N0} bytes  ({lan.TotalOutGb:F3} GB)");
                PrintRow("In  errors", lan.InErrors.ToString());
                PrintRow("Out errors", lan.OutErrors.ToString());
                PrintRow("In  discards", lan.InDiscards.ToString());
                PrintRow("Out discards", lan.OutDiscards.ToString());

                PrintFooter();
                }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
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

        private static string FormatDefects(bool los, bool lof, bool lpr, int raw)
        {
            if (raw == 0)
                return "None";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (los) sb.Append("LOS ");   // Loss of Signal
            if (lof) sb.Append("LOF ");   // Loss of Frame
            if (lpr) sb.Append("LPR ");   // Loss of Power
            sb.Append($"(0x{raw:X2})");
            return sb.ToString().Trim();
        }
    } 
}