using System.Management;
using System.Runtime.InteropServices;

namespace DxtrSysInfo
{
    public class DxSystemInfo
    {
        public static SystemInformation GetSystemInformation()
        {
            var info = new SystemInformation
            {
                OperatingSystem = GetOperatingSystemInfo(),
                Processor = GetProcessorInfo(),
                Memory = GetMemoryInfo(),
                Storage = GetStorageInfo(),
                Network = GetNetworkInfo(),
                Runtime = GetRuntimeInfo()
            };

            return info;
        }

        private static string GetOperatingSystemInfo()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return $"Windows {obj["Version"]} ({obj["Caption"]}) - Build {obj["BuildNumber"]}";
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return $"Linux - {RuntimeInformation.OSDescription}";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return $"macOS - {RuntimeInformation.OSDescription}";
                }
            }
            catch { }

            return RuntimeInformation.OSDescription;
        }

        private static string GetProcessorInfo()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString() ?? "Unknown";
                        string cores = obj["NumberOfCores"]?.ToString() ?? "Unknown";
                        string logicalProcessors = obj["NumberOfLogicalProcessors"]?.ToString() ?? "Unknown";
                        string maxClockSpeed = obj["MaxClockSpeed"]?.ToString() ?? "Unknown";
                        
                        return $"{name}\nCores: {cores}, Logical Processors: {logicalProcessors}, Max Clock Speed: {maxClockSpeed} MHz";
                    }
                }
            }
            catch { }

            return $"{RuntimeInformation.ProcessArchitecture} - {Environment.ProcessorCount} logical processors";
        }

        private static string GetMemoryInfo()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        ulong totalMemoryBytes = Convert.ToUInt64(obj["TotalPhysicalMemory"]);
                        double totalMemoryGB = totalMemoryBytes / (1024.0 * 1024.0 * 1024.0);
                        
                        return $"Total Physical Memory: {totalMemoryGB:F2} GB";
                    }
                }
            }
            catch { }

            var workingSet = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
            return $"GC Working Set: {workingSet:F2} MB";
        }

        private static string GetStorageInfo()
        {
            try
            {
                var drives = DriveInfo.GetDrives();
                var driveInfo = new System.Text.StringBuilder();

                foreach (var drive in drives.Where(d => d.IsReady))
                {
                    double totalGB = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                    double freeGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                    double usedGB = totalGB - freeGB;
                    double percentUsed = (usedGB / totalGB) * 100;

                    driveInfo.AppendLine($"{drive.Name} ({drive.DriveType}) - {drive.VolumeLabel}");
                    driveInfo.AppendLine($"  Total: {totalGB:F2} GB | Used: {usedGB:F2} GB ({percentUsed:F1}%) | Free: {freeGB:F2} GB");
                    driveInfo.AppendLine($"  File System: {drive.DriveFormat}");
                }

                return driveInfo.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                return $"Error retrieving storage info: {ex.Message}";
            }
        }

        private static string GetNetworkInfo()
        {
            try
            {
                var networkInfo = new System.Text.StringBuilder();
                var hostName = System.Net.Dns.GetHostName();
                networkInfo.AppendLine($"Host Name: {hostName}");

                var addresses = System.Net.Dns.GetHostAddresses(hostName);
                networkInfo.AppendLine("IP Addresses:");
                foreach (var addr in addresses)
                {
                    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        networkInfo.AppendLine($"  IPv4: {addr}");
                    }
                    else if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        networkInfo.AppendLine($"  IPv6: {addr}");
                    }
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'True'");
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string description = obj["Description"]?.ToString() ?? "Unknown";
                        string macAddress = obj["MACAddress"]?.ToString() ?? "Unknown";
                        networkInfo.AppendLine($"\nAdapter: {description}");
                        networkInfo.AppendLine($"  MAC Address: {macAddress}");
                    }
                }

                return networkInfo.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                return $"Error retrieving network info: {ex.Message}";
            }
        }

        private static string GetRuntimeInfo()
        {
            var runtimeInfo = new System.Text.StringBuilder();
            runtimeInfo.AppendLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
            runtimeInfo.AppendLine($"OS Architecture: {RuntimeInformation.OSArchitecture}");
            runtimeInfo.AppendLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
            runtimeInfo.AppendLine($"CLR Version: {Environment.Version}");
            runtimeInfo.AppendLine($"Machine Name: {Environment.MachineName}");
            runtimeInfo.AppendLine($"User Name: {Environment.UserName}");
            runtimeInfo.AppendLine($"User Domain: {Environment.UserDomainName}");
            runtimeInfo.AppendLine($"System Directory: {Environment.SystemDirectory}");
            runtimeInfo.AppendLine($"Current Directory: {Environment.CurrentDirectory}");
            runtimeInfo.AppendLine($"64-bit OS: {Environment.Is64BitOperatingSystem}");
            runtimeInfo.AppendLine($"64-bit Process: {Environment.Is64BitProcess}");
            runtimeInfo.AppendLine($"Processor Count: {Environment.ProcessorCount}");
            runtimeInfo.AppendLine($"Working Set: {Environment.WorkingSet / (1024.0 * 1024.0):F2} MB");
            runtimeInfo.AppendLine($"Tick Count: {Environment.TickCount / 1000.0 / 60.0:F2} minutes");

            return runtimeInfo.ToString().TrimEnd();
        }
    }

    public class SystemInformation
    {
        public string OperatingSystem { get; set; } = string.Empty;
        public string Processor { get; set; } = string.Empty;
        public string Memory { get; set; } = string.Empty;
        public string Storage { get; set; } = string.Empty;
        public string Network { get; set; } = string.Empty;
        public string Runtime { get; set; } = string.Empty;
    }
}

