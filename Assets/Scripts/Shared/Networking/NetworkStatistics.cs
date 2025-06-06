using System;

namespace Shared.Networking
{
    public readonly struct NetworkStatistics
    {
        public static readonly NetworkStatistics Empty = new NetworkStatistics(0, 0, minReportedTimeElapsed);

        public readonly long BytesSent;
        public readonly long BytesReceived;
        public readonly float TimeElapsed;

        const float minReportedTimeElapsed = 0.001f;  //avoid divide by zero or huge numbers for first few frames

        public NetworkStatistics(long bytesSent, long bytesReceived, float timeElapsed)
        {
            BytesSent = bytesSent;
            BytesReceived = bytesReceived;
            TimeElapsed = MathF.Max(timeElapsed, minReportedTimeElapsed);
        }

        public NetworkStatistics(NetworkStatistics previous, NetworkStatistics current)
        {
            BytesSent = Math.Max(0, current.BytesSent - previous.BytesSent);
            BytesReceived = Math.Max(0, current.BytesReceived - previous.BytesReceived);
            TimeElapsed = MathF.Max(current.TimeElapsed - previous.TimeElapsed, minReportedTimeElapsed);
        }

        public float AvgBandwidthUp => BytesSent / TimeElapsed;
        public float AvgBandwidthDown => BytesReceived / TimeElapsed;

        public string ToBandwidthString()
        {
            return
                $"Up: {ToBandwidthString(AvgBandwidthUp)}\n" +
                $"Dn: {ToBandwidthString(AvgBandwidthDown)}";
        }

        public static string ToBandwidthString(float bandwidth)
        {
            if (bandwidth < 10)
            {
                return $"  {bandwidth:0.00} B/s";
            }
            if (bandwidth < 100)
            {
                return $" {bandwidth:00.00} B/s";
            }
            if (bandwidth < 1000)
            {
                return $"{bandwidth:000.00} B/s";
            }
            if (bandwidth < 10000)
            {
                return $"  {(bandwidth / 1000):0.0} KB/s";
            }
            if (bandwidth < 100000)
            {
                return $" {(bandwidth / 1000):00.0} KB/s";
            }
            if (bandwidth < 1000000)
            {
                return $"{(bandwidth / 1000):000.0} KB/s";
            }

            return $"{(bandwidth / 1000000):000.0} MB/s";
        }
    }
}