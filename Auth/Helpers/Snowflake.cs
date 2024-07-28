using Microsoft.Extensions.Configuration;

namespace Auth.Helpers;

public static class Snowflake
{
    private static readonly object _lock = new object();
    private static long _lastTimestamp = -1L;
    private static long _sequence = 0L;
    private static long _workerId;
    private static long _datacenterId;
    private static long _twepoch;

    private const int WorkerIdBits = 5;
    private const int DatacenterIdBits = 5;
    private const int SequenceBits = 12;
    private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
    private const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);
    private const int WorkerIdShift = SequenceBits;
    private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
    private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;
    private const long SequenceMask = -1L ^ (-1L << SequenceBits);

    static Snowflake()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var workerIdString = configuration["Snowflake:WorkerId"];
        if (string.IsNullOrEmpty(workerIdString))
        {
            throw new ArgumentException("Snowflake:WorkerId is not set in the configuration.");
        }
        _workerId = long.Parse(workerIdString);

        var datacenterIdString = configuration["Snowflake:DatacenterId"];
        if (string.IsNullOrEmpty(datacenterIdString))
        {
            throw new ArgumentException("Snowflake:DatacenterId is not set in the configuration.");
        }
        _datacenterId = long.Parse(datacenterIdString);

        _twepoch = DateTimeOffset.Parse(configuration["Snowflake:Date"]).ToUnixTimeMilliseconds();
    }

    public static string Next()
    {
        lock (_lock)
        {
            var timestamp = TimeGen();

            if (timestamp < _lastTimestamp)
            {
                throw new InvalidOperationException("Clock moved backwards. Refusing to generate id for " + (_lastTimestamp - timestamp) + " milliseconds");
            }

            if (_lastTimestamp == timestamp)
            {
                _sequence = (_sequence + 1) & SequenceMask;
                if (_sequence == 0)
                {
                    timestamp = TilNextMillis(_lastTimestamp);
                }
            }
            else
            {
                _sequence = 0L;
            }

            _lastTimestamp = timestamp;

            var id = ((timestamp - _twepoch) << TimestampLeftShift) |
                     (_datacenterId << DatacenterIdShift) |
                     (_workerId << WorkerIdShift) |
                     _sequence;

            return id.ToString();
        }
    }

    public static bool Validate(string id)
    {
        return long.TryParse(id, out _);
    }

    public static (long Timestamp, long DatacenterId, long WorkerId, long Sequence) Parse(string id)
    {
        if (!Validate(id))
        {
            throw new ArgumentException("Invalid Snowflake ID");
        }

        var longId = long.Parse(id);
        var timestamp = (longId >> TimestampLeftShift) + _twepoch;
        var datacenterId = (longId >> DatacenterIdShift) & MaxDatacenterId;
        var workerId = (longId >> WorkerIdShift) & MaxWorkerId;
        var sequence = longId & SequenceMask;

        return (timestamp, datacenterId, workerId, sequence);
    }

    public static long GetTimestamp(string id)
    {
        if (!Validate(id))
        {
            throw new ArgumentException("Invalid Snowflake ID");
        }

        var longId = long.Parse(id);
        return (longId >> TimestampLeftShift) + _twepoch;
    }

    private static long TilNextMillis(long lastTimestamp)
    {
        var timestamp = TimeGen();
        while (timestamp <= lastTimestamp)
        {
            timestamp = TimeGen();
        }
        return timestamp;
    }

    private static long TimeGen()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
