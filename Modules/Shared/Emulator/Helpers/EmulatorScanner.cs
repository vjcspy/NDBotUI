using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;

namespace NDBotUI.Modules.Shared.Emulator.Helpers
{
    /// <summary>
    /// Lớp chứa thông tin chi tiết của 1 emulator (device).
    /// </summary>
    public class EmulatorInfo
    {
        /// <summary>
        /// Chuỗi serial của adb (ví dụ: "127.0.0.1:21503" hoặc "emulator-5554")
        /// </summary>
        public string Serial { get; set; }

        /// <summary>
        /// Các thông tin chi tiết trả về từ adb (ví dụ: "device product:... transport_id:6")
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Trạng thái online (true nếu đang online)
        /// </summary>
        public bool Online { get; set; }

        /// <summary>
        /// Tên file của tiến trình (ví dụ: "MEmuHeadless.exe")
        /// </summary>
        public string Exe { get; set; }

        /// <summary>
        /// Command line của tiến trình (ví dụ: "(C:\Program Files\... MEmuHeadless.exe, --comment, ...)")
        /// </summary>
        public string Cmdline { get; set; }

        /// <summary>
        /// Process Id của tiến trình
        /// </summary>
        public int Pid { get; set; }

        /// <summary>
        /// Địa chỉ IP mà adb đang kết nối (ví dụ: "127.0.0.1")
        /// </summary>
        public string AddrIp { get; set; }

        /// <summary>
        /// Port của adb (ví dụ: 21503)
        /// </summary>
        public int AddrPort { get; set; }

        /// <summary>
        /// Đối tượng Process (bạn có thể xem thông tin chi tiết của process)
        /// </summary>
        public Process ProcessInfo { get; set; }

        /// <summary>
        /// Lệnh adb được xây dựng (ví dụ: "C:\Android\adb.exe -s 127.0.0.1:21503")
        /// </summary>
        public string AdbCommand { get; set; }
    }

    /// <summary>
    /// Lớp tiện ích để lưu trữ thông tin dòng từ netstat.
    /// </summary>
    internal class NetstatEntry
    {
        public string LocalAddress { get; set; } // Ví dụ: "127.0.0.1:21503"
        public string IP { get; set; }
        public int Port { get; set; }
        public int PID { get; set; }
    }

    /// <summary>
    /// Lớp tiện ích để lưu thông tin từ lệnh "adb devices -l"
    /// </summary>
    internal class AdbDeviceInfo
    {
        public string Serial { get; set; }
        public string Details { get; set; }
        public bool Online { get; set; }

        /// <summary>
        /// Port được lấy từ chuỗi Serial. Nếu dạng "127.0.0.1:21503" thì port = 21503; 
        /// nếu dạng "emulator-5554" thì giả sử port thực là 5554 + 1 (theo cách mà nhiều emulator hoạt động).
        /// </summary>
        public int Port { get; set; }
    }

    /// <summary>
    /// Lớp thực hiện việc quét các emulator dựa trên scan port và connect adb.
    /// </summary>
    public class EmulatorScanner
    {
        /// <summary>
        /// Quét và kết nối các emulator đang chạy thông qua port.
        /// </summary>
        /// <param name="adbPath">Đường dẫn đầy đủ tới adb.exe</param>
        /// <param name="restartServer">Nếu true thì kill &amp; start lại adb server</param>
        /// <param name="connectTimeoutSeconds">Thời gian timeout (giây) để chờ kết nối adb</param>
        /// <param name="aliveSleepSeconds">Khoảng thời gian (giây) sleep giữa các lần kiểm tra</param>
        /// <returns>Danh sách các đối tượng EmulatorInfo chứa thông tin của từng emulator</returns>
        public static List<EmulatorInfo> ScanEmulators(string adbPath, bool restartServer = false,
            int connectTimeoutSeconds = 15, int aliveSleepSeconds = 3)
        {
            var result = new List<EmulatorInfo>();

            // Nếu restartServer == true thì kill &amp; start lại adb server
            if (restartServer)
            {
                RunCommand(adbPath, "kill-server");
                RunCommand(adbPath, "start-server");
            }

            // Lấy danh sách các kết nối TCP (netstat) với option: -a -n -o -p TCP
            var netstatOutput = RunCommand("netstat.exe", "-a -n -o -p TCP");

            if (string.IsNullOrEmpty(netstatOutput))
            {
                return result;
            }

            var netEntries = ParseNetstatOutput(netstatOutput);

            // Với mỗi entry, gọi lệnh "adb connect {localAddress}"
            foreach (var entry in netEntries.Where(entry => entry.Port >= 5000 && entry.IP == "127.0.0.1"))
            {
                // Ví dụ: adb.exe connect 127.0.0.1:21503
                RunCommand(adbPath, $"connect {entry.LocalAddress}");
            }

            // Lấy danh sách device từ adb
            var adbDevicesOutput = RunCommand(adbPath, "devices -l");
            if (string.IsNullOrEmpty(adbDevicesOutput))
            {
                return result;
            }

            var adbDevices = ParseAdbDevicesOutput(adbDevicesOutput);

            // Với mỗi device trả về, tìm thông tin tương ứng trong danh sách netEntries để ghép nối với thông tin process.
            foreach (var adbDev in adbDevices)
            {
                // Nếu chưa lấy được port thì bỏ qua
                if (adbDev.Port == 0)
                    continue;

                // Tìm entry có port khớp (nếu không khớp trực tiếp, có thể thử khớp theo port ±1 như cách làm của Python)
                var matchingEntry = netEntries.FirstOrDefault(e => e.Port == adbDev.Port);
                if (matchingEntry == null)
                {
                    // Nếu không tìm thấy, thử tìm entry có port = (adbDev.Port - 1)
                    matchingEntry = netEntries.FirstOrDefault(e => e.Port == adbDev.Port - 1);
                }

                if (matchingEntry == null)
                    continue;

                Process proc = null;
                try
                {
                    proc = Process.GetProcessById(matchingEntry.PID);
                }
                catch
                {
                    // Không lấy được process
                }

                string exeName = proc != null ? proc.ProcessName : "";
                string cmdline = GetCommandLine(matchingEntry.PID);

                var info = new EmulatorInfo
                {
                    Serial = adbDev.Serial,
                    Details = adbDev.Details,
                    Online = adbDev.Online,
                    Exe = exeName,
                    Cmdline = cmdline,
                    Pid = matchingEntry.PID,
                    AddrIp = matchingEntry.IP,
                    AddrPort = matchingEntry.Port,
                    ProcessInfo = proc,
                    AdbCommand = $"{adbPath} -s {adbDev.Serial}"
                };

                result.Add(info);
            }

            return result;
        }

        #region Helper Methods

        /// <summary>
        /// Chạy một lệnh và trả về output dạng string.
        /// </summary>
        private static string? RunCommand(string fileName, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using var process = new Process();
                process.StartInfo = psi;
                Console.WriteLine($"Run command {fileName} {arguments}");
                process.Start();
                process.WaitForExit(1000);
                var output = process?.StandardOutput.ReadToEnd();
                var err = process?.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(err))
                {
                    Console.WriteLine(err);
                    return "";
                }

                Console.WriteLine(output);
                return output;
            }
            catch (Exception ex)
            {
                // Log hoặc xử lý exception theo nhu cầu
                Debug.WriteLine($"Error running command {fileName} {arguments}: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// Phân tích output của lệnh netstat.exe để lấy danh sách các kết nối TCP dạng LISTENING với IP là 127.0.0.1 hoặc 0.0.0.0.
        /// </summary>
        private static List<NetstatEntry> ParseNetstatOutput(string netstatOutput)
        {
            var list = new List<NetstatEntry>();

            if (string.IsNullOrWhiteSpace(netstatOutput))
                return list;

            var lines = netstatOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // Lọc các dòng chứa "LISTENING"
                if (!line.Contains("LISTENING", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Ví dụ một dòng: "  TCP    127.0.0.1:21503   0.0.0.0:0    LISTENING    20064"
                var tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 5)
                    continue;

                // tokens[0] là protocol (TCP)
                // tokens[1] là địa chỉ local
                // tokens[3] là trạng thái (LISTENING)
                // tokens[4] là PID
                string localAddr = tokens[1];
                if (!(localAddr.StartsWith("127.0.0.1:") || localAddr.StartsWith("0.0.0.0:")))
                    continue;

                // Tách IP và Port
                var parts = localAddr.Split(':');
                if (parts.Length != 2)
                    continue;

                if (!int.TryParse(parts[1], out int port))
                    continue;

                if (!int.TryParse(tokens[4], out int pid))
                    continue;

                list.Add(new NetstatEntry
                {
                    LocalAddress = localAddr,
                    IP = parts[0],
                    Port = port,
                    PID = pid
                });
            }

            return list;
        }

        /// <summary>
        /// Phân tích output của lệnh "adb devices -l" để lấy danh sách device có chứa "transport_id:".
        /// </summary>
        private static List<AdbDeviceInfo> ParseAdbDevicesOutput(string adbDevicesOutput)
        {
            var list = new List<AdbDeviceInfo>();
            if (string.IsNullOrWhiteSpace(adbDevicesOutput))
                return list;

            var lines = adbDevicesOutput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                // Bỏ qua dòng header ("List of devices attached")
                if (line.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Chỉ lấy những dòng chứa "transport_id:"
                if (!line.Contains("transport_id:"))
                    continue;

                // Phân tách: token đầu tiên là serial, phần còn lại là details
                var tokens = line.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 2)
                    continue;

                string serial = tokens[0].Trim();
                string details = tokens[1].Trim();
                // Xét trạng thái online: nếu details bắt đầu bằng "offline" thì online = false
                bool online = !details.StartsWith("offline", StringComparison.OrdinalIgnoreCase);

                // Lấy port từ serial:
                // Nếu serial dạng "127.0.0.1:21503" thì lấy số sau dấu ":"
                // Nếu serial dạng "emulator-5554" thì giả sử port thực là 5554 + 1
                int port = 0;
                if (serial.Contains(":"))
                {
                    var sp = serial.Split(':');
                    if (sp.Length == 2 && int.TryParse(sp[1], out int p))
                    {
                        port = p;
                    }
                }
                else if (serial.StartsWith("emulator-"))
                {
                    string portPart = serial.Substring("emulator-".Length);
                    if (int.TryParse(portPart, out int basePort))
                    {
                        port = basePort + 1;
                    }
                }

                list.Add(new AdbDeviceInfo
                {
                    Serial = serial,
                    Details = details,
                    Online = online,
                    Port = port
                });
            }

            return list;
        }

        /// <summary>
        /// Lấy command line của một process theo PID sử dụng WMI.
        /// </summary>
        private static string GetCommandLine(int pid)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {pid}");

                foreach (var o in searcher.Get())
                {
                    var mo = (ManagementObject)o;
                    return mo["CommandLine"]?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting command line for PID {pid}: {ex.Message}");
            }

            return "";
        }

        #endregion
    }
}