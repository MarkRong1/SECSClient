using Secs4Net;
using System.Globalization;
using System.IO;

namespace SECSClient.Config
{
    public static class IniConfigLoader
    {
        public static SecsGemOptions LoadSecsGemOptions(string path, SecsGemOptions? defaults = null)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"找不到設定檔: {path}");

            var dict = ParseIni(path);
            if (!dict.TryGetValue("secs4net", out var kv))
                throw new InvalidOperationException("config.ini 缺少 [secs4net] 區段");

            var opt = defaults ?? new SecsGemOptions();

            // 字串
            if (TryGet(kv, "IpAddress", out string ip)) opt.IpAddress = ip;

            // 布林
            if (TryGetBool(kv, "IsActive", out bool isActive)) opt.IsActive = isActive;

            // 整數 (int)
            if (TryGetInt(kv, "Port", out int port)) opt.Port = port;
            if (TryGetInt(kv, "T3", out int t3)) opt.T3 = t3;
            if (TryGetInt(kv, "T5", out int t5)) opt.T5 = t5;
            if (TryGetInt(kv, "T6", out int t6)) opt.T6 = t6;
            if (TryGetInt(kv, "T7", out int t7)) opt.T7 = t7;
            if (TryGetInt(kv, "T8", out int t8)) opt.T8 = t8;
            if (TryGetInt(kv, "LinkTestInterval", out int lti)) opt.LinkTestInterval = lti;

            // ushort
            if (TryGetUShort(kv, "DeviceId", out ushort deviceId)) opt.DeviceId = deviceId;

            return opt;
        }

        private static Dictionary<string, Dictionary<string, string>> ParseIni(string path)
        {
            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            var current = string.Empty;

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    current = line.Substring(1, line.Length - 2).Trim();
                    if (!result.ContainsKey(current))
                        result[current] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    continue;
                }

                var idx = line.IndexOf('=');
                if (idx <= 0) continue;

                var key = line.Substring(0, idx).Trim();
                var val = line.Substring(idx + 1).Trim();

                if (!result.ContainsKey(current))
                    result[current] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                result[current][key] = val;
            }

            return result;
        }

        private static bool TryGet(Dictionary<string, string> kv, string key, out string value)
            => kv.TryGetValue(key, out value);

        private static bool TryGetInt(Dictionary<string, string> kv, string key, out int value)
        {
            value = default;
            return kv.TryGetValue(key, out var s)
                && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryGetUShort(Dictionary<string, string> kv, string key, out ushort value)
        {
            value = default;
            return kv.TryGetValue(key, out var s)
                && ushort.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryGetBool(Dictionary<string, string> kv, string key, out bool value)
        {
            value = default;
            if (!kv.TryGetValue(key, out var s)) return false;

            s = s.Trim().ToLowerInvariant();
            if (s is "true" or "false")
            {
                value = bool.Parse(s);
                return true;
            }
            if (s is "1" or "0")
            {
                value = (s == "1");
                return true;
            }
            return false;
        }
    }
}
