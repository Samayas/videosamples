using DefenderStats.Models;
using System.Management;

namespace DefenderStats.Services
{
    public class DefenderWmiService
    {
        private const string DefenderNamespace = @"root\Microsoft\Windows\Defender";

        public DefenderComputerStatusModel? GetComputerStatus()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(DefenderNamespace, "SELECT * FROM MSFT_MpComputerStatus"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        DefenderComputerStatusModel result = new DefenderComputerStatusModel();

                        result.AMEngineVersion = ToStr(obj, "AMEngineVersion");
                        result.AMProductVersion = ToStr(obj, "AMProductVersion");
                        result.AMServiceVersion = ToStr(obj, "AMServiceVersion");
                        result.NISEngineVersion = ToStr(obj, "NISEngineVersion");
                        result.ComputerID = ToStr(obj, "ComputerID");
                        result.ComputerState = ToUInt32(obj, "ComputerState");

                        result.AMServiceEnabled = ToBool(obj, "AMServiceEnabled");
                        result.AntivirusEnabled = ToBool(obj, "AntivirusEnabled");
                        result.AntispywareEnabled = ToBool(obj, "AntispywareEnabled");
                        result.RealTimeProtectionEnabled = ToBool(obj, "RealTimeProtectionEnabled");
                        result.BehaviorMonitorEnabled = ToBool(obj, "BehaviorMonitorEnabled");
                        result.IoavProtectionEnabled = ToBool(obj, "IoavProtectionEnabled");
                        result.OnAccessProtectionEnabled = ToBool(obj, "OnAccessProtectionEnabled");
                        result.NISEnabled = ToBool(obj, "NISEnabled");

                        result.AntivirusSignatureVersion = ToStr(obj, "AntivirusSignatureVersion");
                        result.AntivirusSignatureAge = ToUInt32(obj, "AntivirusSignatureAge");
                        result.AntivirusSignatureLastUpdated = ToDateTime(obj, "AntivirusSignatureLastUpdated");

                        result.AntispywareSignatureVersion = ToStr(obj, "AntispywareSignatureVersion");
                        result.AntispywareSignatureAge = ToUInt32(obj, "AntispywareSignatureAge");
                        result.AntispywareSignatureLastUpdated = ToDateTime(obj, "AntispywareSignatureLastUpdated");

                        result.NISSignatureVersion = ToStr(obj, "NISSignatureVersion");
                        result.NISSignatureAge = ToUInt32(obj, "NISSignatureAge");
                        result.NISSignatureLastUpdated = ToDateTime(obj, "NISSignatureLastUpdated");

                        result.FullScanStartTime = ToDateTime(obj, "FullScanStartTime");
                        result.FullScanEndTime = ToDateTime(obj, "FullScanEndTime");
                        result.FullScanAge = ToUInt32(obj, "FullScanAge");
                        result.LastFullScanSource = ToByte(obj, "LastFullScanSource");

                        result.QuickScanStartTime = ToDateTime(obj, "QuickScanStartTime");
                        result.QuickScanEndTime = ToDateTime(obj, "QuickScanEndTime");
                        result.QuickScanAge = ToUInt32(obj, "QuickScanAge");
                        result.LastQuickScanSource = ToByte(obj, "LastQuickScanSource");

                        result.RealTimeScanDirection = ToByte(obj, "RealTimeScanDirection");

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"GetComputerStatus failed: {ex.Message}");
            }

            return null;
        }

        public DefenderPreferenceModel? GetPreferences()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(DefenderNamespace, "SELECT * FROM MSFT_MpPreference"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        DefenderPreferenceModel result = new DefenderPreferenceModel();

                        result.ExclusionPath = ToStringArray(obj, "ExclusionPath");
                        result.ExclusionExtension = ToStringArray(obj, "ExclusionExtension");
                        result.ExclusionProcess = ToStringArray(obj, "ExclusionProcess");

                        result.ScanParameters = ToByte(obj, "ScanParameters");
                        result.ScanScheduleDay = ToByte(obj, "ScanScheduleDay");
                        result.ScanScheduleTime = ToUInt32(obj, "ScanScheduleTime");
                        result.RealTimeScanDirection = ToByte(obj, "RealTimeScanDirection");
                        result.ScanAvgCPULoadFactor = ToUInt32(obj, "ScanAvgCPULoadFactor");

                        result.DisableRealtimeMonitoring = ToBool(obj, "DisableRealtimeMonitoring");
                        result.DisableBehaviorMonitoring = ToBool(obj, "DisableBehaviorMonitoring");
                        result.DisableBlockAtFirstSeen = ToBool(obj, "DisableBlockAtFirstSeen");
                        result.DisableIOAVProtection = ToBool(obj, "DisableIOAVProtection");
                        result.DisableArchiveScanning = ToBool(obj, "DisableArchiveScanning");
                        result.DisableIntrusionPreventionSystem = ToBool(obj, "DisableIntrusionPreventionSystem");
                        result.DisableScriptScanning = ToBool(obj, "DisableScriptScanning");
                        result.DisablePrivacyMode = ToBool(obj, "DisablePrivacyMode");
                        result.DisableCatchupFullScan = ToBool(obj, "DisableCatchupFullScan");
                        result.DisableCatchupQuickScan = ToBool(obj, "DisableCatchupQuickScan");
                        result.SignatureDisableUpdateOnStartupWithoutEngine = ToBool(obj, "SignatureDisableUpdateOnStartupWithoutEngine");
                        result.CheckForSignaturesBeforeRunningScan = ToBool(obj, "CheckForSignaturesBeforeRunningScan");

                        result.SubmitSamplesConsent = ToUInt32(obj, "SubmitSamplesConsent");
                        result.MAPSReporting = ToByte(obj, "MAPSReporting");
                        result.SignatureUpdateInterval = ToUInt32(obj, "SignatureUpdateInterval");

                        result.HighThreatDefaultAction = ToUInt32(obj, "HighThreatDefaultAction");
                        result.ModerateThreatDefaultAction = ToUInt32(obj, "ModerateThreatDefaultAction");
                        result.LowThreatDefaultAction = ToUInt32(obj, "LowThreatDefaultAction");
                        result.SevereThreatDefaultAction = ToUInt32(obj, "SevereThreatDefaultAction");

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"GetPreferences failed: {ex.Message}");
            }

            return null;
        }

        public List<DefenderThreatModel> GetThreats()
        {
            List<DefenderThreatModel> threats = new List<DefenderThreatModel>();

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(DefenderNamespace, "SELECT * FROM MSFT_MpThreat"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        DefenderThreatModel threat = new DefenderThreatModel();

                        threat.ThreatID = ToInt64(obj, "ThreatID");
                        threat.ThreatName = ToStr(obj, "ThreatName");
                        threat.SeverityID = ToByte(obj, "SeverityID");
                        threat.CategoryID = ToByte(obj, "CategoryID");
                        threat.TypeID = ToByte(obj, "TypeID");
                        threat.IsActive = ToBool(obj, "IsActive");
                        threat.RollupStatus = ToBool(obj, "RollupStatus");
                        threat.IsServiceStopping = ToBool(obj, "IsServiceStopping");
                        threat.DidThreatExecute = ToInt64(obj, "DidThreatExecute");
                        threat.DefaultActionID = ToUInt32(obj, "DefaultActionID");

                        threats.Add(threat);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"GetThreats failed: {ex.Message}");
            }

            return threats;
        }

        public List<DefenderThreatDetectionModel> GetThreatDetections()
        {
            List<DefenderThreatDetectionModel> detections = new List<DefenderThreatDetectionModel>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(DefenderNamespace, "SELECT * FROM MSFT_MpThreatDetection"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        DefenderThreatDetectionModel detection = new DefenderThreatDetectionModel();

                        detection.DetectionID = ToStr(obj, "DetectionID");
                        detection.ThreatID = ToInt64(obj, "ThreatID");
                        detection.InitialDetectionTime = ToDateTime(obj, "InitialDetectionTime");
                        detection.LastThreatStatusChangeTime = ToDateTime(obj, "LastThreatStatusChangeTime");
                        detection.RemediationTime = ToDateTime(obj, "RemediationTime");
                        detection.DomainUser = ToStr(obj, "DomainUser");
                        detection.ProcessName = ToStr(obj, "ProcessName");
                        detection.Resources = ToStringArray(obj, "Resources");
                        detection.DetectionSourceTypeID = ToByte(obj, "DetectionSourceTypeID");
                        detection.ThreatStatusID = ToUInt32(obj, "ThreatStatusID");
                        detection.ThreatStatusErrorCode = ToUInt32(obj, "ThreatStatusErrorCode");
                        detection.CleaningActionID = ToUInt32(obj, "CleaningActionID");
                        detection.ActionSuccess = ToBool(obj, "ActionSuccess");
                        detection.AdditionalActionsBitMask = ToUInt32(obj, "AdditionalActionsBitMask");
                        detection.CurrentThreatExecutionStatusID = ToByte(obj, "CurrentThreatExecutionStatusID");
                        detection.AMProductVersion = ToStr(obj, "AMProductVersion");

                        detections.Add(detection);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"GetThreatDetections failed: {ex.Message}");
            }
            return detections;
        }

        public void StartQuickScan()
        {
            try
            {
                using (ManagementClass mpScan = new ManagementClass(DefenderNamespace, "MSFT_MpScan", null))
                {
                    ManagementBaseObject inParams = mpScan.GetMethodParameters("Start");
                    inParams["ScanType"] = 1; // 1=Quick 2=Full 3=Custom
                    mpScan.InvokeMethod("Start", inParams, null);
                }
            }
            catch (Exception ex)
            {
                Log($"StartQuickScan failed: {ex.Message}");
            }
        }

        private uint ToUInt32(ManagementObject obj, string property)
        {
            try
            {
                object value = obj[property];
                if (value == null) return 0;
                string stringValue = value?.ToString() ?? string.Empty;
                if (stringValue.Contains('.') && stringValue.Contains(':'))
                    return (uint)ManagementDateTimeConverter.ToTimeSpan(stringValue).TotalMinutes;
                return Convert.ToUInt32(stringValue);
            }
            catch (Exception ex)
            {
                Log($"ToUInt32 failed on [{property}]: {ex.Message}");
                return 0;
            }
        }

        private byte ToByte(ManagementObject obj, string property)
        {
            try
            {
                object value = obj[property];
                if (value == null) return 0;
                string stringValue = value?.ToString() ?? string.Empty;
                if (stringValue.Contains('.') && stringValue.Contains(':'))
                    return (byte)ManagementDateTimeConverter.ToTimeSpan(stringValue).TotalMinutes;
                return Convert.ToByte(stringValue);
            }
            catch (Exception ex)
            {
                Log($"ToByte failed on [{property}]: {ex.Message}");
                return 0;
            }
        }

        private long ToInt64(ManagementObject obj, string property)
        {
            try
            {
                object value = obj[property];
                if (value == null) return 0;
                return Convert.ToInt64(value);
            }
            catch (Exception ex)
            {
                Log($"ToInt64 failed on [{property}]: {ex.Message}");
                return 0;
            }
        }

        private bool ToBool(ManagementObject obj, string property)
        {
            try
            {
                object value = obj[property];
                if (value == null) return false;
                return Convert.ToBoolean(value);
            }
            catch (Exception ex)
            {
                Log($"ToBool failed on [{property}]: {ex.Message}");
                return false;
            }
        }

        private string ToStr(ManagementObject obj, string property)
        {
            try
            {
                object value = obj[property];
                return value?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Log($"ToStr failed on [{property}]: {ex.Message}");
                return string.Empty;
            }
        }

        private DateTime? ToDateTime(ManagementObject obj, string property)
        {
            try
            {
                object value = obj[property];
                if (value == null) return null;
                return ManagementDateTimeConverter.ToDateTime(value.ToString());
            }
            catch (Exception ex)
            {
                Log($"ToDateTime failed on [{property}]: {ex.Message}");
                return null;
            }
        }

        private string[] ToStringArray(ManagementObject obj, string property)
        {
            try
            {
                object value = obj[property];
                if (value == null) return Array.Empty<string>();
                return (string[])value;
            }
            catch (Exception ex)
            {
                Log($"ToStringArray failed on [{property}]: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        private void Log(string message)
        {
            Console.WriteLine($"[DefenderWmiService] {message}");
            // swap Console for your logger, e.g.:
            // _logger.LogWarning("[DefenderWmiService] {Message}", message);
        }
    }
}
