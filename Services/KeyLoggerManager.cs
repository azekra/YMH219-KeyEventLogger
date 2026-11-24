using System.Windows.Forms;
using YMH219_KeyEventLogger.Models;

namespace YMH219_KeyEventLogger.Services
{
    public class KeyLoggerManager
    {
        private readonly List<KeyLogEntry> _entries = new();

        public bool IsRecording { get; private set; }
        public IReadOnlyList<KeyLogEntry> Entries => _entries.AsReadOnly();
        public int TotalKeyCount => _entries.Count;

        public void Start()
        {
            IsRecording = true;
        }

        public void Stop()
        {
            IsRecording = false;
        }

        public void Clear()
        {
            _entries.Clear();
        }

        public KeyLogEntry? RecordKey(Keys keyCode, Keys modifiers, string eventType = "KeyDown")
        {
            if (!IsRecording)
                return null;

            var entry = new KeyLogEntry(keyCode, modifiers, eventType);
            _entries.Add(entry);
            return entry;
        }

        public List<KeyValuePair<string, int>> GetTopKeys(int topCount = 5)
        {
            return _entries
                .GroupBy(entry => entry.KeyName)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key)
                .Take(topCount)
                .Select(group => new KeyValuePair<string, int>(group.Key, group.Count()))
                .ToList();
        }

        public List<KeyLogEntry> FilterByTimeRange(DateTime start, DateTime end)
        {
            if (start > end)
                throw new ArgumentException("Başlangıç zamanı, bitiş zamanından büyük olamaz.");

            return _entries
                .Where(entry => entry.Timestamp >= start && entry.Timestamp <= end)
                .ToList();
        }
    }
}
