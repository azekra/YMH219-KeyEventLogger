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
            if (keyCode == Keys.None || !Enum.IsDefined(keyCode))
                throw new ArgumentOutOfRangeException(nameof(keyCode), "Geçerli bir tuş kodu belirtilmelidir.");

            var allowedModifiers = Keys.Control | Keys.Shift | Keys.Alt;
            if ((modifiers & ~allowedModifiers) != Keys.None)
                throw new ArgumentOutOfRangeException(nameof(modifiers), "Yalnızca Ctrl, Shift ve Alt kullanılabilir.");

            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Olay türü boş olamaz.", nameof(eventType));

            Timestamp = DateTime.Now;
            KeyCode = keyCode;
            KeyName = keyCode.ToString();
            Modifiers = modifiers == Keys.None ? string.Empty : modifiers.ToString();
            EventType = eventType.Trim();
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
