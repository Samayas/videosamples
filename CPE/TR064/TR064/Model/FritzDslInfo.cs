namespace TR064.Model
{
    public sealed class FritzDslInfo
    {
        public FritzDslInfo(int downstreamCurrRate, int upstreamCurrRate, int downstreamMaxRate, int upstreamMaxRate, int downstreamNoiseMargin, int upstreamNoiseMargin)
        {
            this.DownstreamCurrRate = downstreamCurrRate;
            this.UpstreamCurrRate = upstreamCurrRate;
            this.DownstreamMaxRate = downstreamMaxRate;
            this.UpstreamMaxRate = upstreamMaxRate;
            this.DownstreamNoiseMargin = downstreamNoiseMargin;
            this.UpstreamNoiseMargin = upstreamNoiseMargin;
        }

        public int DownstreamCurrRate { get; }
        public int UpstreamCurrRate { get; }
        public int DownstreamMaxRate { get; }
        public int UpstreamMaxRate { get; }
        public int DownstreamNoiseMargin { get; }
        public int UpstreamNoiseMargin { get; }

        public override string ToString()
        {
            return $"DownstreamCurrRate : {DownstreamCurrRate} - UpstreamCurrRate : {UpstreamCurrRate} - DownstreamMaxRate : {DownstreamMaxRate} - UpstreamMaxRate : {UpstreamMaxRate} - DownstreamNoiseMargin : {DownstreamNoiseMargin} - UpstreamNoiseMargin : {UpstreamNoiseMargin}";
        }
    }
}
