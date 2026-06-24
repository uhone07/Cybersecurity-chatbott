using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbot.Data
{
    public static class ActivityLog
    {
        private static readonly List<string> _entries = new List<string>();
        private const int MaxEntries = 50;

        public static void Add(string action)
        {
            _entries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {action}");
            if (_entries.Count > MaxEntries)
                _entries.RemoveAt(_entries.Count - 1);
        }

        public static List<string> GetRecent(int count = 10)
            => _entries.Take(count).ToList();

        public static List<string> GetAll()
            => new List<string>(_entries);

        public static int TotalCount => _entries.Count;
    }
}