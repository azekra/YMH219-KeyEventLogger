using System.Text;
using YMH219_KeyEventLogger.Models;

namespace YMH219_KeyEventLogger.Services
{
    public class LogFileService
    {
        public void SaveToTxt(string filePath, IEnumerable<KeyLogEntry> entries)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Geçerli bir dosya yolu belirtilmelidir.", nameof(filePath));

            ArgumentNullException.ThrowIfNull(entries);

            var logList = entries.ToList();
            if (logList.Count == 0)
                throw new InvalidOperationException("Kaydedilecek herhangi bir tuş kaydı bulunmamaktadır.");

            if (logList.Any(entry => entry is null))
                throw new ArgumentException("Kayıt listesi boş kayıt içeremez.", nameof(entries));

            using var writer = new StreamWriter(filePath, append: false, encoding: Encoding.UTF8);

            writer.WriteLine("=== YMH219 Key Event Logger - Kayıt Dosyası ===");
            writer.WriteLine($"Oluşturulma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            writer.WriteLine($"Toplam Kayıt Sayısı: {logList.Count}");
            writer.WriteLine(new string('-', 48));

            foreach (var entry in logList)
                writer.WriteLine(entry);
        }
    }
}
