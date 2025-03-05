using Microsoft.Extensions.Options;
using API.Shared.Configuration;

namespace API.Shared.Utilities;

/// <summary>
/// Static class for generating Snowflake IDs.
/// </summary>
public static class Snowflake
{
    private static readonly object _lock = new();
    private static bool _isInitialized;
    private static long _lastTimestamp = -1L;
    private static long _sequence;

    // Configuration access
    private static IOptions<SnowflakeSettings>? _options;

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

    /// <summary>
    /// Sets the options to be used when the generator is first accessed.
    /// </summary>
    /// <param name="options">The Snowflake settings options.</param>
    public static void SetOptions(IOptions<SnowflakeSettings> options)
    {
        _options = options;
    }

    /// <summary>
    /// Manual initialization with options
    /// </summary>
    public static void Initialize(IOptions<SnowflakeSettings> options)
    {
        if (_isInitialized) return;

        var settings = options.Value;
        _datacenterId = settings.DatacenterId;
        _workerId = settings.WorkerId;
        _epoch = DateTimeOffset.Parse(settings.Epoch);

        _isInitialized = true;
    }

    /// <summary>
    /// Generates a new Snowflake ID.
    /// </summary>
    /// <returns>A unique Snowflake ID.</returns>
    public static long Generate()
    {
        EnsureInitialized();

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

    private static void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            if (_options == null)
            {
                throw new InvalidOperationException("Snowflake has not been configured. Call SetOptions() first.");
            }

            Initialize(_options);
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
