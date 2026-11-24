using System.Windows.Forms;

namespace YMH219_KeyEventLogger.Models
{
    public class KeyLogEntry
    {
        public DateTime Timestamp { get; }
        public Keys KeyCode { get; }
        public string KeyName { get; }
        public string Modifiers { get; }
        public string EventType { get; }

        public KeyLogEntry(Keys keyCode, Keys modifiers, string eventType = "KeyDown")
        {
            Timestamp = DateTime.Now;
            KeyCode = keyCode;
            KeyName = keyCode.ToString();
            Modifiers = modifiers == Keys.None ? string.Empty : modifiers.ToString();
            EventType = eventType;
        }

        public override string ToString()
        {
            string timeText = Timestamp.ToString("dd.MM.yyyy HH:mm:ss");
            string keyDisplay = string.IsNullOrEmpty(Modifiers)
                ? KeyName
                : $"{Modifiers} + {KeyName}";

            return $"[{timeText}] | {EventType} | Tuş: {keyDisplay}";
        }
    }
}
