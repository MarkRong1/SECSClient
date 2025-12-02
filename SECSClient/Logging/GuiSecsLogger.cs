using Secs4Net;
using System.IO;
using System.Text;

namespace SECSClient.Logging
{
    public class GuiSecsLogger : ISecsGemLogger
    {
        private readonly LogBuffer _buffer;
        private readonly string logDir = Path.Combine(AppContext.BaseDirectory, "logs");

        public GuiSecsLogger(LogBuffer buffer)
        {
            _buffer = buffer;
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
        }

        public void MessageIn(SecsMessage msg, int id)
        {
            string logEntry = FormatSecsLog("IN", msg,id);
            _buffer.Add(logEntry);
            WriteToFile(logEntry);
        }

        public void MessageOut(SecsMessage msg, int id)
        {
            string logEntry = FormatSecsLog("OUT", msg ,id);
            _buffer.Add(logEntry);
            WriteToFile(logEntry);
        }
        private void WriteToFile(string logEntry)
        {
            string fileName = DateTime.Now.ToString("yyyyMMdd") + ".log";
            string filePath = Path.Combine(logDir, fileName);
            File.AppendAllText(filePath, "["+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +"] " + logEntry + Environment.NewLine);
        }

        private string FormatSecsLog(string direction, SecsMessage msg ,int id)
        {
            //string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string header = $"Message {direction}, S{msg.S}F{msg.F}, SysByte:{id} {msg.Name}";
            string body = FormatSecsItem(msg.SecsItem, 1);
            return $"{header}\n{body}";
        }

        // 遞迴格式化 SecsItem
        private string FormatSecsItem(Item item, int indent)
        {
            if (item == null) return "";
            string tab = new string('\t', indent);

            if (item.Format == SecsFormat.List)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{tab}L[{item.Count}]");
                foreach (var sub in item.Items)
                    sb.Append(FormatSecsItem(sub, indent + 1));
                return sb.ToString();
            }
            else
            {
                string valueStr = "";
                try
                {
                    switch (item.Format)
                    {
                        case SecsFormat.ASCII:
                        case SecsFormat.JIS8:
                            valueStr = item.GetString();
                            break;
                        case SecsFormat.Binary:
                            var bytes = item.GetMemory<byte>().ToArray();
                            valueStr = BitConverter.ToString(bytes).Replace("-", " "); // 顯示十六進位
                            break;
                        case SecsFormat.Boolean:
                            var bools = item.GetMemory<bool>().ToArray();
                            valueStr = string.Join(", ", bools.Select(b => b ? "TRUE" : "FALSE"));
                            break;
                        case SecsFormat.I1:
                        case SecsFormat.I2:
                        case SecsFormat.I4:
                        case SecsFormat.I8:
                            valueStr = string.Join(", ", item.GetMemory<int>().ToArray());
                            break;
                        case SecsFormat.U1:
                        case SecsFormat.U2:
                        case SecsFormat.U4:
                        case SecsFormat.U8:
                            valueStr = string.Join(", ", item.GetMemory<uint>().ToArray());
                            break;
                        case SecsFormat.F4:
                        case SecsFormat.F8:
                            valueStr = string.Join(", ", item.GetMemory<double>().ToArray());
                            break;
                        default:
                            valueStr = "(Unsupported format)";
                            break;
                    }
                }
                catch
                {
                    valueStr = "(Error reading value)";
                }

                return $"{tab}{GetFormatShortName(item.Format)}[{item.Count}] {valueStr}\n"; // 移除空格
            }
        }


        // 其他 Logger 方法可不寫入檔案
        public void Debug(string text) => _buffer.Add($"DBG {text}");
        public void Info(string text) => _buffer.Add($"INF {text}");
        public void Warning(string text) => _buffer.Add($"WRN {text}");
        public void Error(string text) => _buffer.Add($"ERR {text}");
        private string GetFormatShortName(SecsFormat format)
        {
            return format switch
            {
                SecsFormat.Binary => "B",
                SecsFormat.Boolean => "BL",
                SecsFormat.ASCII => "A",
                SecsFormat.JIS8 => "J",
                SecsFormat.I1 => "I1",
                SecsFormat.I2 => "I2",
                SecsFormat.I4 => "I4",
                SecsFormat.I8 => "I8",
                SecsFormat.U1 => "U1",
                SecsFormat.U2 => "U2",
                SecsFormat.U4 => "U4",
                SecsFormat.U8 => "U8",
                SecsFormat.F4 => "F4",
                SecsFormat.F8 => "F8",
                SecsFormat.List => "L",
                _ => format.ToString()
            };
        }
    }
}