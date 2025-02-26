using Microsoft.Extensions.Options;
using API.Shared.Configuration;

namespace API.Shared.Security;

public static class Snowflake
{
    private static readonly object _lock = new();
    private static bool _isInitialized;
    private static long _lastTimestamp = -1L;
    private static long _sequence;

    // Component values
    private static long _datacenterId;
    private static long _workerId;
    private static DateTimeOffset _epoch;

    // Bit size allocation
    private const int SequenceBits = 12;
    private const int WorkerIdBits = 5;
    private const int DatacenterIdBits = 5;
    private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;
    private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
    private const int WorkerIdShift = SequenceBits;

    // Maximum values for each component
    private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
    private const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);
    private const long SequenceMask = -1L ^ (-1L << SequenceBits);

    public static void Initialize(IConfiguration configuration)
    {
        if (_isInitialized) return;

        var settings = configuration.GetSection("Snowflake").Get<SnowflakeSettings>()
            ?? throw new InvalidOperationException("Snowflake settings are not configured");

        InitializeWithSettings(settings);
    }
    
    public static void Initialize(IOptions<SnowflakeSettings> options)
    {
        if (_isInitialized) return;
        
        var settings = options.Value;
        InitializeWithSettings(settings);
    }
    
    private static void InitializeWithSettings(SnowflakeSettings settings)
    {
        _datacenterId = settings.DatacenterId;
        _workerId = settings.WorkerId;
        _epoch = DateTimeOffset.Parse(settings.Epoch);

        // Validation is now handled by DataAnnotations in the SnowflakeSettings class
        _isInitialized = true;
    }

    public static long Generate()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("Snowflake helper is not initialized. Call Initialize() first.");
        }

        lock (_lock)
        {
            var timestamp = GetTimestamp();

            if (timestamp < _lastTimestamp)
            {
                throw new InvalidOperationException("Clock moved backwards. Refusing to generate ID.");
            }

            if (_lastTimestamp == timestamp)
            {
                _sequence = (_sequence + 1) & SequenceMask;
                if (_sequence == 0)
                {
                    // Sequence exhausted, wait for next millisecond
                    timestamp = WaitNextMillis(_lastTimestamp);
                }
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;

            return (timestamp << TimestampLeftShift) |
                   (_datacenterId << DatacenterIdShift) |
                   (_workerId << WorkerIdShift) |
                   _sequence;
        }
    }

    private static long GetTimestamp()
    {
        return (long)(DateTimeOffset.UtcNow - _epoch).TotalMilliseconds;
    }

    private static long WaitNextMillis(long lastTimestamp)
    {
        var timestamp = GetTimestamp();
        while (timestamp <= lastTimestamp)
        {
            timestamp = GetTimestamp();
        }
        return timestamp;
    }
}
