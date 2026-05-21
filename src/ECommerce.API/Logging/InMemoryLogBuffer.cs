using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ECommerce.API.Logging
{
    public class LogEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public DateTimeOffset Timestamp { get; set; }
        public string Level { get; set; } = "Information";
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public string? SourceContext { get; set; }
    }

    public class InMemoryLogBuffer
    {
        private const int MaxEntries = 300;
        private readonly ConcurrentQueue<LogEntry> _entries = new();

        public void Add(LogEntry entry)
        {
            _entries.Enqueue(entry);
            // Trim to max size
            while (_entries.Count > MaxEntries)
                _entries.TryDequeue(out _);
        }

        public IReadOnlyList<LogEntry> GetRecent(int limit = 100, string? level = null)
        {
            var all = _entries.ToArray();
            IEnumerable<LogEntry> query = all.Reverse();

            if (!string.IsNullOrEmpty(level) && level != "all")
                query = query.Where(e => e.Level.Equals(level, StringComparison.OrdinalIgnoreCase));

            return query.Take(limit).ToList();
        }

        public int TotalCount => _entries.Count;

        public Dictionary<string, int> LevelCounts()
        {
            var all = _entries.ToArray();
            return all
                .GroupBy(e => e.Level)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
