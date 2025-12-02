using Microsoft.Extensions.DependencyInjection;
using SECSClient.Logging;
using System.Collections.Specialized;
using System.Windows;

namespace SECSClient
{
    public partial class MainWindow : Window
    {
        private readonly LogBuffer _buffer;
        private const int MaxLines = 500; // 可自行調整最大行數
        public MainWindow()
        {
            InitializeComponent();
            _buffer = App.AppHost.Services.GetRequiredService<LogBuffer>();
            lstLog.ItemsSource = _buffer.Lines;
            _buffer.Lines.CollectionChanged += Lines_CollectionChanged; // 加這行

        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _buffer.Lines.Clear();
        }

        private void Lines_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                // 限制行數
                while (_buffer.Lines.Count > MaxLines)
                {
                    _buffer.Lines.RemoveAt(0);
                }
                // 如果有新增項目且勾選自動捲動
                if (e.Action == NotifyCollectionChangedAction.Add && chkAutoScroll.IsChecked == true)
                {
                    lstLog.ScrollIntoView(_buffer.Lines[_buffer.Lines.Count - 1]);
                }
            }
            catch (Exception)
            {
            }

        }
    }
}