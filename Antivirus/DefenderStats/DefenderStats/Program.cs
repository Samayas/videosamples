using DefenderStats.Models;
using DefenderStats.Services;

namespace DefenderStats
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DefenderWmiService service = new DefenderWmiService();

          //  service.StartQuickScan();


            PrintComputerStatus(service);
            PrintPreferences(service);
            PrintActiveThreats(service);
            PrintThreatDetections(service);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static void PrintComputerStatus(DefenderWmiService service)
        {
            Console.WriteLine("=== COMPUTER STATUS ===");
            DefenderComputerStatusModel? status = service.GetComputerStatus();
            if (status == null) return;

            Console.WriteLine($"AM Engine Version         : {status?.AMEngineVersion}");
            Console.WriteLine($"AM Product Version        : {status?.AMProductVersion}");
            Console.WriteLine($"AM Service Version        : {status?.AMServiceVersion}");
            Console.WriteLine($"NIS Engine Version        : {status?.NISEngineVersion}");
            Console.WriteLine($"Computer ID               : {status?.ComputerID}");
            Console.WriteLine($"Computer State            : {status?.ComputerState}");

            Console.WriteLine("\n--- Protection Flags ---");
            Console.WriteLine($"AM Service Enabled        : {status?.AMServiceEnabled}");
            Console.WriteLine($"Antivirus Enabled         : {status?.AntivirusEnabled}");
            Console.WriteLine($"Antispyware Enabled       : {status?.AntispywareEnabled}");
            Console.WriteLine($"Real-Time Protection      : {status?.RealTimeProtectionEnabled}");
            Console.WriteLine($"Behavior Monitor          : {status?.BehaviorMonitorEnabled}");
            Console.WriteLine($"IOAV Protection           : {status?.IoavProtectionEnabled}");
            Console.WriteLine($"On-Access Protection      : {status?.OnAccessProtectionEnabled}");
            Console.WriteLine($"NIS Enabled               : {status?.NISEnabled}");

            Console.WriteLine("\n--- Antivirus Signatures ---");
            Console.WriteLine($"Version                   : {status?.AntivirusSignatureVersion}");
            Console.WriteLine($"Age (days)                : {status?.AntivirusSignatureAge}");
            Console.WriteLine($"Last Updated              : {status?.AntivirusSignatureLastUpdated}");

            Console.WriteLine("\n--- Antispyware Signatures ---");
            Console.WriteLine($"Version                   : {status?.AntispywareSignatureVersion}");
            Console.WriteLine($"Age (days)                : {status?.AntispywareSignatureAge}");
            Console.WriteLine($"Last Updated              : {status?.AntispywareSignatureLastUpdated}");

            Console.WriteLine("\n--- NIS Signatures ---");
            Console.WriteLine($"Version                   : {status?.NISSignatureVersion}");
            Console.WriteLine($"Age (days)                : {status?.NISSignatureAge}");
            Console.WriteLine($"Last Updated              : {status?.NISSignatureLastUpdated}");

            Console.WriteLine("\n--- Quick Scan ---");
            Console.WriteLine($"Start Time                : {status?.QuickScanStartTime}");
            Console.WriteLine($"End Time                  : {status?.QuickScanEndTime}");
            Console.WriteLine($"Age (days)                : {status?.QuickScanAge}");
            Console.WriteLine($"Last Source               : {status?.LastQuickScanSource}");

            Console.WriteLine("\n--- Full Scan ---");
            Console.WriteLine($"Start Time                : {status?.FullScanStartTime}");
            Console.WriteLine($"End Time                  : {status?.FullScanEndTime}");
            Console.WriteLine($"Age (days)                : {status?.FullScanAge}");
            Console.WriteLine($"Last Source               : {status?.LastFullScanSource}");
        }

        private static void PrintPreferences(DefenderWmiService service)
        {
            Console.WriteLine("\n=== PREFERENCES ===");
            DefenderPreferenceModel? prefs = service.GetPreferences();
            if (prefs == null) return;

            Console.WriteLine($"Scan Type                 : {prefs?.ScanParameters} (1=Quick 2=Full)");
            Console.WriteLine($"Scheduled Day             : {prefs?.ScanScheduleDay}");
            Console.WriteLine($"Scheduled Time (min)      : {prefs?.ScanScheduleTime}");
            Console.WriteLine($"Avg CPU Load Factor       : {prefs?.ScanAvgCPULoadFactor}%");
            Console.WriteLine($"Signature Update Interval : {prefs?.SignatureUpdateInterval}h");
            Console.WriteLine($"MAPS Reporting            : {prefs?.MAPSReporting} (0=Off 1=Basic 2=Advanced)");
            Console.WriteLine($"Submit Samples Consent    : {prefs?.SubmitSamplesConsent}");

            Console.WriteLine("\n--- Disabled Features ---");
            Console.WriteLine($"Realtime Monitoring       : {prefs?.DisableRealtimeMonitoring}");
            Console.WriteLine($"Behavior Monitoring       : {prefs?.DisableBehaviorMonitoring}");
            Console.WriteLine($"Block At First Seen       : {prefs?.DisableBlockAtFirstSeen}");
            Console.WriteLine($"IOAV Protection           : {prefs?.DisableIOAVProtection}");
            Console.WriteLine($"Archive Scanning          : {prefs?.DisableArchiveScanning}");
            Console.WriteLine($"Script Scanning           : {prefs?.DisableScriptScanning}");
            Console.WriteLine($"Intrusion Prevention      : {prefs?.DisableIntrusionPreventionSystem}");
            Console.WriteLine($"Catchup Full Scan         : {prefs?.DisableCatchupFullScan}");
            Console.WriteLine($"Catchup Quick Scan        : {prefs?.DisableCatchupQuickScan}");

            Console.WriteLine("\n--- Default Threat Actions ---");
            Console.WriteLine($"Low                       : {prefs?.LowThreatDefaultAction}");
            Console.WriteLine($"Moderate                  : {prefs?.ModerateThreatDefaultAction}");
            Console.WriteLine($"High                      : {prefs?.HighThreatDefaultAction}");
            Console.WriteLine($"Severe                    : {prefs?.SevereThreatDefaultAction}");

            Console.WriteLine("\n--- Exclusions ---");
            PrintArray("Paths     ", prefs?.ExclusionPath);
            PrintArray("Extensions", prefs?.ExclusionExtension);
            PrintArray("Processes ", prefs?.ExclusionProcess);
        }

        private static void PrintActiveThreats(DefenderWmiService service)
        {
            Console.WriteLine("\n=== ACTIVE THREATS ===");
            List<DefenderThreatModel> threats = service.GetThreats();
            if (threats.Count == 0)
            {
                Console.WriteLine("No threats found.");
                return;
            }

            foreach (DefenderThreatModel threat in threats)
            {
                Console.WriteLine($"\n  Threat ID     : {threat.ThreatID}");
                Console.WriteLine($"  Name          : {threat.ThreatName}");
                Console.WriteLine($"  Severity      : {threat.SeverityID} (1=Low 2=Moderate 4=High 5=Severe)");
                Console.WriteLine($"  Category      : {threat.CategoryID}");
                Console.WriteLine($"  Is Active     : {threat.IsActive}");
                Console.WriteLine($"  Did Execute   : {threat.DidThreatExecute}");
                Console.WriteLine($"  Default Action: {threat.DefaultActionID}");
            }
        }

        private static void PrintThreatDetections(DefenderWmiService service)
        {
            Console.WriteLine("\n=== THREAT DETECTIONS ===");
            List<DefenderThreatDetectionModel> detections = service.GetThreatDetections();
            if (detections.Count == 0)
            {
                Console.WriteLine("No detections found.");
                return;
            }

            foreach (DefenderThreatDetectionModel detection in detections)
            {
                Console.WriteLine($"\n  Detection ID        : {detection.DetectionID}");
                Console.WriteLine($"  Threat ID           : {detection.ThreatID}");
                Console.WriteLine($"  Process             : {detection.ProcessName}");
//                Console.WriteLine($"  Domain\\User         : {detection.DomainUser}");
                Console.WriteLine($"  Initial Detection   : {detection.InitialDetectionTime}");
                Console.WriteLine($"  Last Status Change  : {detection.LastThreatStatusChangeTime}");
                Console.WriteLine($"  Remediation Time    : {detection.RemediationTime}");
                Console.WriteLine($"  Status ID           : {detection.ThreatStatusID}");
                Console.WriteLine($"  Action Success      : {detection.ActionSuccess}");
                Console.WriteLine($"  Source Type         : {detection.DetectionSourceTypeID}");
                Console.WriteLine($"  AM Product Version  : {detection.AMProductVersion}");
                PrintArray("  Resources", detection.Resources);
            }
        }

        private static void PrintArray(string label, string[]? values)
        {
            if (values == null || values.Length == 0)
            {
                Console.WriteLine($"  {label}          : (none)");
                return;
            }
            foreach (string value in values)
            {
                Console.WriteLine($"  {label}          : {value.Replace("stefan.casier", "stef")}");
            }
        }
    }
}