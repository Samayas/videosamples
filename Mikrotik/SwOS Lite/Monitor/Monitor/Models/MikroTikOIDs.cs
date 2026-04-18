namespace Mikrotik.SwOSLite.Monitor.Models
{
    public static class MikroTikOIDs
    {
        // System Information (RFC1213-MIB)
        public const string SysDescr = "1.3.6.1.2.1.1.1.0";
        public const string SysObjectID = "1.3.6.1.2.1.1.2.0";
        public const string SysUpTime = "1.3.6.1.2.1.1.3.0";
        public const string SysContact = "1.3.6.1.2.1.1.4.0";
        public const string SysName = "1.3.6.1.2.1.1.5.0";
        public const string SysLocation = "1.3.6.1.2.1.1.6.0";

        // MikroTik Specific (MIKROTIK-MIB)
        public const string MtxrLicVersion = "1.3.6.1.4.1.14988.1.1.4.4.0";
        public const string MtxrSerialNumber = "1.3.6.1.4.1.14988.1.1.7.3.0";
        public const string MtxrFirmwareVersion = "1.3.6.1.4.1.14988.1.1.7.4.0";

        // Interfaces (IF-MIB)
        public const string IfIndex = "1.3.6.1.2.1.2.2.1.1";
        public const string IfDescr = "1.3.6.1.2.1.2.2.1.2";
        public const string IfType = "1.3.6.1.2.1.2.2.1.3";
        public const string IfPhysAddress = "1.3.6.1.2.1.2.2.1.6";
        public const string IfAdminStatus = "1.3.6.1.2.1.2.2.1.7";
        public const string IfOperStatus = "1.3.6.1.2.1.2.2.1.8";
        public const string IfInOctets = "1.3.6.1.2.1.2.2.1.10";
        public const string IfInErrors = "1.3.6.1.2.1.2.2.1.14";
        public const string IfInDiscards = "1.3.6.1.2.1.2.2.1.13";
        public const string IfOutOctets = "1.3.6.1.2.1.2.2.1.16";
        public const string IfOutErrors = "1.3.6.1.2.1.2.2.1.20";
        public const string IfOutDiscards = "1.3.6.1.2.1.2.2.1.19";

        // Interface Extensions (IF-MIB)
        public const string IfName = "1.3.6.1.2.1.31.1.1.1.1";
        public const string IfAlias = "1.3.6.1.2.1.31.1.1.1.18";
        public const string IfHCInOctets = "1.3.6.1.2.1.31.1.1.1.6";
        public const string IfHCOutOctets = "1.3.6.1.2.1.31.1.1.1.10";
        public const string IfHighSpeed = "1.3.6.1.2.1.31.1.1.1.15";
        public const string IfInMulticastPkts = "1.3.6.1.2.1.31.1.1.1.2";
        public const string IfInBroadcastPkts = "1.3.6.1.2.1.31.1.1.1.3";
        public const string IfOutMulticastPkts = "1.3.6.1.2.1.31.1.1.1.4";
        public const string IfOutBroadcastPkts = "1.3.6.1.2.1.31.1.1.1.5";

        // MikroTik Hosts
        public const string HostsMacTableOid = ".1.3.6.1.2.1.17.4.3.1.1";
        public const string HostsPortTableOid = ".1.3.6.1.2.1.17.4.3.1.2";

        // MikroTik Health
        public const string MtxrHlTemperature = "1.3.6.1.4.1.14988.1.1.3.100.1.3.52.0";

        // Ethernet-like Statistics (EtherLike-MIB)
        public const string Dot3StatsAlignmentErrors = "1.3.6.1.2.1.10.7.2.1.2";
        public const string Dot3StatsFCSErrors = "1.3.6.1.2.1.10.7.2.1.3";
        public const string Dot3StatsSingleCollisionFrames = "1.3.6.1.2.1.10.7.2.1.4";
        public const string Dot3StatsMultipleCollisionFrames = "1.3.6.1.2.1.10.7.2.1.5";
        public const string Dot3StatsDeferredTransmissions = "1.3.6.1.2.1.10.7.2.1.7";
        public const string Dot3StatsLateCollisions = "1.3.6.1.2.1.10.7.2.1.8";
        public const string Dot3StatsExcessiveCollisions = "1.3.6.1.2.1.10.7.2.1.9";
        public const string Dot3StatsFrameTooLongs = "1.3.6.1.2.1.10.7.2.1.13";
        public const string Dot3StatsDuplexStatus = "1.3.6.1.2.1.10.7.2.1.19";

        // STP (Spanning Tree Protocol) - BRIDGE-MIB
        public const string Dot1dStpProtocolSpecification = "1.3.6.1.2.1.17.2.1.0";
        public const string Dot1dStpPriority = "1.3.6.1.2.1.17.2.2.0";
        public const string Dot1dStpTimeSinceTopologyChange = "1.3.6.1.2.1.17.2.3.0";
        public const string Dot1dStpTopChanges = "1.3.6.1.2.1.17.2.4.0";
        public const string Dot1dStpDesignatedRoot = "1.3.6.1.2.1.17.2.5.0";
        public const string Dot1dStpRootCost = "1.3.6.1.2.1.17.2.6.0";
        public const string Dot1dStpRootPort = "1.3.6.1.2.1.17.2.7.0";
        public const string Dot1dStpMaxAge = "1.3.6.1.2.1.17.2.8.0";
        public const string Dot1dStpHelloTime = "1.3.6.1.2.1.17.2.9.0";
        public const string Dot1dStpForwardDelay = "1.3.6.1.2.1.17.2.11.0";

        // MikroTik Optical Modules (SFP)
        public const string MtxrOpticalTable = "1.3.6.1.4.1.14988.1.1.19.1.1";
        public const string MtxrOpticalName = "1.3.6.1.4.1.14988.1.1.19.1.1.2";
        public const string MtxrOpticalRxLoss = "1.3.6.1.4.1.14988.1.1.19.1.1.3";
        public const string MtxrOpticalTxFault = "1.3.6.1.4.1.14988.1.1.19.1.1.4";
        public const string MtxrOpticalWavelength = "1.3.6.1.4.1.14988.1.1.19.1.1.5";
        public const string MtxrOpticalTemperature = "1.3.6.1.4.1.14988.1.1.19.1.1.6";
        public const string MtxrOpticalSupplyVoltage = "1.3.6.1.4.1.14988.1.1.19.1.1.7";
        public const string MtxrOpticalTxCurrent = "1.3.6.1.4.1.14988.1.1.19.1.1.8";
        public const string MtxrOpticalTxPower = "1.3.6.1.4.1.14988.1.1.19.1.1.9";
        public const string MtxrOpticalRxPower = "1.3.6.1.4.1.14988.1.1.19.1.1.10";
    }
}
