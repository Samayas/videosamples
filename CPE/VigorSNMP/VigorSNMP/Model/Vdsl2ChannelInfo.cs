namespace VigorSNMP.Model
{
    public sealed class Vdsl2ChannelInfo
    {
        public long CurrentDsRateBps { get; set; }
        public long CurrentUsRateBps { get; set; }
        public long PrevDsRateBps { get; set; }
        public long PrevUsRateBps { get; set; }
        public double ImpulseNoiseProtDs { get; set; }
        public double ImpulseNoiseProtUs { get; set; }
        public int InterleaveDelayMs { get; set; }

        public double CurrentDsRateMbps => CurrentDsRateBps / 1_000_000.0;
        public double CurrentUsRateMbps => CurrentUsRateBps / 1_000_000.0;
        public double PrevDsRateMbps => PrevDsRateBps / 1_000_000.0;
        public double PrevUsRateMbps => PrevUsRateBps / 1_000_000.0;
        public double DsRateDeltaMbps => CurrentDsRateMbps - PrevDsRateMbps;
        public double UsRateDeltaMbps => CurrentUsRateMbps - PrevUsRateMbps;
    }
}
