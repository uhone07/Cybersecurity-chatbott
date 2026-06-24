using System;

namespace CybersecurityChatbot.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public DateTime? ReminderDate { get; set; } = null;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string StatusIcon => IsCompleted ? "✅" : "🔲";
        public string ReminderText => ReminderDate.HasValue
            ? $"⏰ Reminder: {ReminderDate.Value:dd MMM yyyy}"
            : "No reminder set";
    }
}