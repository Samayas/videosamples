namespace DefenderStats.Models
{
    public class DefenderPreferenceModel
    {
        public string[] ExclusionPath { get; set; } = new string[0];
        public string[] ExclusionExtension { get; set; } = new string[0];
        public string[] ExclusionProcess { get; set; } = new string[0];
        public byte ScanParameters { get; set; }          // 1=QuickScan 2=FullScan
        public byte ScanScheduleDay { get; set; }         // 0=Everyday ... 8=Never
        public uint ScanScheduleTime { get; set; }
        public byte RealTimeScanDirection { get; set; }
        public uint ScanAvgCPULoadFactor { get; set; }
        public bool DisableRealtimeMonitoring { get; set; }
        public bool DisableBehaviorMonitoring { get; set; }
        public bool DisableBlockAtFirstSeen { get; set; }
        public bool DisableIOAVProtection { get; set; }
        public bool DisablePrivacyMode { get; set; }
        public bool SignatureDisableUpdateOnStartupWithoutEngine { get; set; }
        public bool DisableArchiveScanning { get; set; }
        public bool DisableIntrusionPreventionSystem { get; set; }
        public bool DisableScriptScanning { get; set; }
        public uint SubmitSamplesConsent { get; set; }
        public byte MAPSReporting { get; set; }           // 0=Disabled 1=Basic 2=Advanced
        public uint HighThreatDefaultAction { get; set; }
        public uint ModerateThreatDefaultAction { get; set; }
        public uint LowThreatDefaultAction { get; set; }
        public uint SevereThreatDefaultAction { get; set; }
        public bool CheckForSignaturesBeforeRunningScan { get; set; }
        public uint SignatureUpdateInterval { get; set; }
        public bool DisableCatchupFullScan { get; set; }
        public bool DisableCatchupQuickScan { get; set; }
    }
}
