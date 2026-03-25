namespace VigorSNMP.Model
{
    public sealed class Vdsl2Performance15MinInfo
    {
        public long CoErroredSecs { get; set; }
        public long CoSeverelyErroredSecs { get; set; }
        public long CoLossOfSignalSecs { get; set; }
        public long CoUnavailableSecs { get; set; }

        public long CpeErroredSecs { get; set; }
        public long CpeSeverelyErroredSecs { get; set; }
        public long CpeLossOfSignalSecs { get; set; }
        public long CpeUnavailableSecs { get; set; }

        public bool HasCoErrors => CoErroredSecs > 0 || CoSeverelyErroredSecs > 0;
        public bool HasCpeErrors => CpeErroredSecs > 0 || CpeSeverelyErroredSecs > 0;
        public bool IsLineStable => CoUnavailableSecs == 0 && CpeUnavailableSecs == 0 && CoLossOfSignalSecs == 0 && CpeLossOfSignalSecs == 0;
    }
}
