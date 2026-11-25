using YMH219_KeyEventLogger.Models;
using YMH219_KeyEventLogger.Services;

namespace YMH219_KeyEventLogger
{
    public class Form1 : Form
    {
        private readonly KeyLoggerManager _keyLoggerManager = new();
        private readonly LogFileService _logFileService = new();
        private readonly ListBox _entriesList = new();
        private readonly Label _recordingStatus = new();
        private readonly Label _statisticsLabel = new();
        private readonly DateTimePicker _startTimePicker = new();
        private readonly DateTimePicker _endTimePicker = new();
        private List<KeyLogEntry> _displayedEntries = new();
        private bool _isFilterActive;

        public Form1()
        {
            ConfigureForm();
            BuildInterface();
            RefreshView();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            var modifiers = Keys.None;
            if (e.Control)
                modifiers |= Keys.Control;
            if (e.Shift)
                modifiers |= Keys.Shift;
            if (e.Alt)
                modifiers |= Keys.Alt;

            var entry = _keyLoggerManager.RecordKey(e.KeyCode, modifiers);
            if (entry is null)
                return;

            if (_isFilterActive)
            {
                ApplyTimeFilter(showErrors: false);
                return;
            }

            _displayedEntries.Add(entry);
            RefreshView();
        }

        private void ConfigureForm()
        {
            Text = "YMH219 Key Event Logger";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(760, 480);
            Size = new Size(900, 600);
            KeyPreview = true;
        }

        private void BuildInterface()
        {
            var actionsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                Padding = new Padding(10, 8, 10, 4),
                WrapContents = false,
                AutoScroll = true
            };

            actionsPanel.Controls.Add(CreateButton("Başlat", (_, _) => StartRecording()));
            actionsPanel.Controls.Add(CreateButton("Durdur", (_, _) => StopRecording()));
            actionsPanel.Controls.Add(CreateButton("Temizle", (_, _) => ClearEntries()));
            actionsPanel.Controls.Add(CreateButton("TXT'ye Kaydet", (_, _) => ExportEntries()));

            _recordingStatus.AutoSize = true;
            _recordingStatus.Padding = new Padding(12, 9, 0, 0);
            _recordingStatus.Font = new Font(Font, FontStyle.Bold);
            actionsPanel.Controls.Add(_recordingStatus);

            var filterPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 48,
                Padding = new Padding(10, 7, 10, 4),
                WrapContents = false,
                AutoScroll = true
            };

            ConfigureDatePicker(_startTimePicker, DateTime.Today);
            ConfigureDatePicker(_endTimePicker, DateTime.Now);

            filterPanel.Controls.Add(CreateFilterLabel("Başlangıç:"));
            filterPanel.Controls.Add(_startTimePicker);
            filterPanel.Controls.Add(CreateFilterLabel("Bitiş:"));
            filterPanel.Controls.Add(_endTimePicker);
            filterPanel.Controls.Add(CreateButton("Filtrele", (_, _) => ApplyTimeFilter(showErrors: true)));
            filterPanel.Controls.Add(CreateButton("Tümünü Göster", (_, _) => ShowAllEntries()));

            _entriesList.Dock = DockStyle.Fill;
            _entriesList.HorizontalScrollbar = true;
            _entriesList.IntegralHeight = false;
            _entriesList.Font = new Font("Consolas", 10);

            _statisticsLabel.Dock = DockStyle.Bottom;
            _statisticsLabel.Height = 64;
            _statisticsLabel.Padding = new Padding(10, 8, 10, 8);
            _statisticsLabel.BorderStyle = BorderStyle.FixedSingle;

            Controls.Add(_entriesList);
            Controls.Add(_statisticsLabel);
            Controls.Add(filterPanel);
            Controls.Add(actionsPanel);
        }

        private static Button CreateButton(string text, EventHandler onClick)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 30,
                Margin = new Padding(3)
            };
            button.Click += onClick;
            return button;
        }

        private static Label CreateFilterLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Padding = new Padding(0, 7, 0, 0),
                Margin = new Padding(8, 3, 3, 3)
            };
        }

        private static void ConfigureDatePicker(DateTimePicker picker, DateTime value)
        {
            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = "dd.MM.yyyy HH:mm:ss";
            picker.Width = 150;
            picker.Value = value;
            picker.Margin = new Padding(3);
        }

        private void StartRecording()
        {
            _keyLoggerManager.Start();
            RefreshView();
            ActiveControl = null;
        }

        private void StopRecording()
        {
            _keyLoggerManager.Stop();
            RefreshView();
            ActiveControl = null;
        }

        private void ClearEntries()
        {
            _keyLoggerManager.Clear();
            _displayedEntries.Clear();
            _isFilterActive = false;
            RefreshView();
            ActiveControl = null;
        }

        private void ExportEntries()
        {
            if (_displayedEntries.Count == 0)
            {
                MessageBox.Show(this, "Kaydedilecek kayıt bulunmuyor.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Title = "Kayıtları TXT dosyasına kaydet",
                Filter = "Metin dosyası (*.txt)|*.txt",
                DefaultExt = "txt",
                AddExtension = true,
                FileName = "key-event-log.txt",
                OverwritePrompt = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                _logFileService.SaveToTxt(dialog.FileName, _displayedEntries);
                MessageBox.Show(this, "Kayıtlar dosyaya kaydedildi.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                MessageBox.Show(this, $"Dosya kaydedilemedi: {ex.Message}", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTimeFilter(bool showErrors)
        {
            try
            {
                _displayedEntries = _keyLoggerManager.FilterByTimeRange(
                    _startTimePicker.Value, _endTimePicker.Value);
                _isFilterActive = true;
                RefreshView();
            }
            catch (ArgumentException ex)
            {
                if (showErrors)
                    MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            ActiveControl = null;
        }

        private void ShowAllEntries()
        {
            _isFilterActive = false;
            _displayedEntries = _keyLoggerManager.Entries.ToList();
            RefreshView();
            ActiveControl = null;
        }

        private void RefreshView()
        {
            _entriesList.BeginUpdate();
            _entriesList.Items.Clear();
            _entriesList.Items.AddRange(_displayedEntries
                .Select(entry => (object)entry.ToString())
                .ToArray());
            if (_entriesList.Items.Count > 0)
                _entriesList.TopIndex = _entriesList.Items.Count - 1;
            _entriesList.EndUpdate();

            _recordingStatus.Text = _keyLoggerManager.IsRecording
                ? "● Kayıt aktif"
                : "● Kayıt durduruldu";
            _recordingStatus.ForeColor = _keyLoggerManager.IsRecording
                ? Color.ForestGreen
                : Color.Firebrick;

            var topKeys = _keyLoggerManager.GetTopKeys();
            var topKeysText = topKeys.Count == 0
                ? "Henüz tuş kaydı yok"
                : string.Join(", ", topKeys.Select(item => $"{item.Key}: {item.Value}"));

            _statisticsLabel.Text = $"Toplam kayıt: {_keyLoggerManager.TotalKeyCount}    " +
                $"Gösterilen: {_displayedEntries.Count}\r\n" +
                $"En sık kullanılan tuşlar: {topKeysText}";
        }
    }
}
