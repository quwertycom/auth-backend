using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace API.Common.Helpers;

public static class Snowflake
{
    private static readonly object _lock = new();
    private static bool _isInitialized;
    private static long _lastTimestamp = -1L;
    private static long _sequence;

    // Configurable parameters
    private static long _datacenterId;
    private static long _workerId;
    private static DateTimeOffset _epoch;

    // Bit lengths for each component
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

        // Load configuration values directly
        _datacenterId = long.Parse(configuration["ENV__SNOWFLAKE__DATACENTER_ID"] ?? "1");
        _workerId = long.Parse(configuration["ENV__SNOWFLAKE__WORKER_ID"] ?? "1");
        _epoch = DateTimeOffset.Parse(configuration["ENV__SNOWFLAKE__EPOCH"] ?? "2024-01-01T00:00:00Z");

        // Validate parameters
        if (_datacenterId > MaxDatacenterId || _datacenterId < 0)
        {
            throw new ArgumentException($"Datacenter ID must be between 0 and {MaxDatacenterId}");
        }

        if (_workerId > MaxWorkerId || _workerId < 0)
        {
            throw new ArgumentException($"Worker ID must be between 0 and {MaxWorkerId}");
        }

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
