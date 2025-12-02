using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;


namespace SECSClient.Logging
{
    public class LogBuffer
    {
        // 供 UI 顯示（維持原有行為）
        public ObservableCollection<string> Lines { get; } = new();
        //檔案寫入
        private readonly string _rootLogDir;
        private readonly ConcurrentDictionary<string, StreamWriter> _writers = new();

        public LogBuffer()
        {
            _rootLogDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(_rootLogDir);
        }

        public void Add(string text)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] {text}";
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() => Lines.Add(logEntry));
        }

        /// 為指定設備/控制器寫log檔案，可選擇同時顯示到 UI。
        public void AddFor(string component, string text, bool alsoUi = false, Exception? ex = null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] {text}";
            try
            {
                var writer = GetWriter(component);
                writer.WriteLine(logEntry);
                if (ex is not null) writer.WriteLine(ex.ToString());
            }
            catch (Exception fileEx)
            {
                // 若寫檔失敗，至少把錯誤拋到 UI 緩衝，避免靜默
                App.Current.Dispatcher.BeginInvoke(() => Lines.Add($"[{timestamp}] [FILE-LOG-ERROR] {fileEx.Message}"));
            }

            if (alsoUi)
            {
                App.Current.Dispatcher.BeginInvoke(() => Lines.Add(logEntry));
            }
        }

        private StreamWriter GetWriter(string component)
        {
            string folder = Path.Combine(_rootLogDir, component);
            Directory.CreateDirectory(folder);
            string file = Path.Combine(folder, $"{DateTime.Now:yyyyMMdd}.log");

            return _writers.GetOrAdd(file, f =>
            {
                var fs = new FileStream(f, FileMode.Append, FileAccess.Write, FileShare.Read);
                var sw = new StreamWriter(fs, new UTF8Encoding(false)) { AutoFlush = true };
                return sw;
            });
        }

        public void Dispose()
        {
            foreach (var kv in _writers)
                kv.Value.Dispose();
            _writers.Clear();
        }
    }
}