using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using Common.Notify;
using Common.Utilities;

using Microsoft.Win32;

namespace Common.Interop
{
    public static class WinApi
    {
        private const string User32 = "user32.dll";

        public const int WM_DEVICECHANGE = 0x0219;
        public const int WM_GETMINMAXINFO = 0x0024;
        public const int DBT_DEVTYP_DEVICEINTERFACE = 0x05;
        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        #region Constants
        public enum WindowStyles
        {
            Maximize = 0x01000000
        }
        public enum ShowWindowMode
        {
            Maximize = 3,
            Restore = 9
        }
        #endregion //Constants

        #region User32
        [DllImport(User32)]
        public static extern Boolean ShowWindow(IntPtr hWnd, Int32 nCmdShow);

        [DllImport(User32)]
        public static extern int SetForegroundWindow(int hWnd);

        [DllImport(User32, SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        #endregion //User32

        public static string GetComPortNum(string vid, string pid)
        {
            var comNum = string.Empty;
            var format = string.Empty;
            if (!string.IsNullOrEmpty(vid + pid))
                format = string.Format("VID_{0}&PID_{1}", vid, pid);
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"Select * from Win32_SerialPort"))
                {
                    using (ManagementObjectCollection managements = searcher.Get())
                    {
                        foreach (ManagementObject manObj in managements)
                        {
                            var pnp = manObj["PNPDeviceID"] == null ? string.Empty : manObj["PNPDeviceID"].ToString();
                            if (string.IsNullOrEmpty(pnp) || !pnp.Contains(format))
                                continue;
                            comNum = manObj["DeviceID"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return comNum;
        }

        public static string GetUSBComPortNum(string vid, string pid)
        {
            var comNum = string.Empty;
            var format = string.Empty;
            if (!string.IsNullOrEmpty(vid + pid))
                format = string.Format("VID_{0}&PID_{1}", vid, pid);
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"SELECT * FROM WIN32_SerialPort"))
                {
                    using (ManagementObjectCollection managements = searcher.Get())
                    {
                        foreach (ManagementObject manObj in managements)
                        {
                            var caption = manObj["Caption"]?.ToString();
                            var pnpid = manObj["PNPDeviceID"]?.ToString();
                            var deviceid = manObj["DeviceID"]?.ToString();
                            //var hwid = manObj["HardwareID"] as string[];
                            if (string.IsNullOrEmpty(caption))
                                continue;
                            if (!caption.Contains("(COM"))
                                continue;
                            //var pnp = manObj["PNPDeviceID"] == null ? string.Empty : manObj["PNPDeviceID"].ToString();
                            //if (string.IsNullOrEmpty(pnp) || !pnp.Contains(format))
                            //    continue;
                            comNum = manObj["DeviceID"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return comNum;
        }

        private const string vidPattern = @"VID_([0-9A-F]{4})";
        private const string pidPattern = @"PID_([0-9A-F]{4})";
        public struct ComPort // custom struct with our desired values
        {
            public string name;
            public string vid;
            public string pid;
            public string description;
            public PropertyDataCollection properties;
        }
        public static async System.Threading.Tasks.Task<List<DeviceInfo>> GetSerialPorts(string vid, string pid)
        {
            var devices = new List<DeviceInfo>();
            await System.Threading.Tasks.Task.Run(() => {
                //WIN32_SerialPort
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%Serial Port%'"))
                {
                    var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                    var list = ports.Select(p =>
                    {
                        try
                        {
                            var deviceID    = p.GetPropertyValue("DeviceID")?.ToString();
                            var pnpDeviceID = p.GetPropertyValue("PNPDeviceID")?.ToString();
                            Match mVID = Regex.Match(pnpDeviceID, vidPattern, RegexOptions.IgnoreCase);
                            Match mPID = Regex.Match(pnpDeviceID, pidPattern, RegexOptions.IgnoreCase);
                            
                            if (mVID.Success && mPID.Success)
                            {
                                var c = new ComPort
                                {
                                    name = deviceID,
                                    vid = vid,
                                    description = p.GetPropertyValue("Caption")?.ToString(),
                                    properties = p.Properties
                                };
                                c.vid = mVID.Groups[1].Value;
                                c.pid = mPID.Groups[1].Value;
                                return c;
                            }
                            return new ComPort();
                        }
                        catch (Exception)
                        {
                            return new ComPort();
                        }
                    }).ToList();
                    list.ForEach(c => {
                        if (c.vid != null && c.vid.Equals(vid) && c.pid != null && c.pid.Equals(pid))
                        {
                            var dev = new DeviceInfo(String.Empty, c.properties);
                            dev.ComNum = dev.Name.Replace("USB Serial Port(", "").Replace(")", "");
                            devices.Add(dev);
                        }
                    });
                    //or if we want to extract all devices with specified values:
                    //var coms = list.FindAll(c => c.vid != null && c.vid.Equals(vid) && c.pid != null && c.pid.Equals(pid));
                    ////if we want to find one device
                    //var com = list.FindLast(c => c.vid.Equals(vid) && c.pid.Equals(pid));
                }
            });
            return devices;
        }

        /// <summary>
        /// 장치 정보를 레지스트리에서 가져온다.
        /// </summary>
        /// <param name="VID"></param>
        /// <param name="PID"></param>
        /// <returns></returns>
        public static List<string> GetRegistryComPortNames(String VID, String PID, String deviceID = null)
        {
            List<string> comports = new List<string>();
            try
            {
                var device_ids = deviceID?.Split(new char[] { '\\' });
                var pattern = String.Format("^VID_{0}.PID_{1}", VID, PID);
                Regex _rx = new Regex(pattern, RegexOptions.IgnoreCase);

                RegistryKey rk1 = Registry.LocalMachine;
                RegistryKey rk2 = rk1.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum");

                foreach (String s3 in rk2.GetSubKeyNames())
                {
                    RegistryKey rk3 = rk2.OpenSubKey(s3);
                    foreach (String s in rk3.GetSubKeyNames())
                    {
                        if (_rx.Match(s).Success && s.Contains(VID) && s.Contains(PID))
                        {
                            if (device_ids != null && device_ids.Length > 2 && s.ToLower().Contains(device_ids.Last().ToLower()))
                                continue;

                            RegistryKey rk4 = rk3.OpenSubKey(s);
                            foreach (String s2 in rk4.GetSubKeyNames())
                            {
                                RegistryKey rk5 = rk4.OpenSubKey(s2);
                                var HardwareID = (string[])rk5.GetValue("HardwareID");
                                var FriendlyName = (string)rk5.GetValue("FriendlyName");
                                if (HardwareID.FirstOrDefault() is string id && !string.IsNullOrEmpty(FriendlyName))
                                {
                                    var start = FriendlyName.IndexOf('(') + 1;
                                    var end   = FriendlyName.IndexOf(')');
                                    var port  = FriendlyName.Substring(start, end - start).TrimStart('0');
                                    if (!String.IsNullOrEmpty(port))
                                        comports.Add(String.Format("{0:####}", port));
                                }
                                //RegistryKey rk6 = rk5.OpenSubKey("Device Parameters");
                                //comports.Add((string)rk6.GetValue("PortName"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return comports;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<DeviceInfo> GetUSBDevices(string modelName = "", string vid = "", string pid = "")
        {
            List<DeviceInfo> devices = new List<DeviceInfo>();

            try
            {
                Thread thread = new Thread(() =>
                {
                    var model = string.Empty;
                    var list = GetDevices("USB", modelName, vid, pid);
                    //var list = GetDevices("USB", modelName, out string model);
                    foreach (var device in list)
                    {
                        Logger.WriteLog(LogTypes.Info, "----- DEVICE -----");
                        //var model = device.GetPropertyValue("Model")?.ToString();
                        var deviceID = device.GetPropertyValue("DeviceID")?.ToString();
                        var dev = new DeviceInfo(model, device.Properties) {
                            ComNum = GetRegistryComPortNames(vid, pid, deviceID)?.FirstOrDefault()
                            //ComNum = GetUSBComPortNum(vid, pid)
                            //ComNum = GetSerialPorts(vid, pid)?.LastOrDefault().name
                        };
                        devices.Add(dev);
                        //device.Dispose();
                        Logger.WriteLog(LogTypes.Info, "------------------");
                    }
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join(); //wait for the thread to finish
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }

            return devices;
        }

        public static IList<ManagementBaseObject> GetDevices(string InterfaceType, string model, out string modelName)
        {
            var disks = new List<ManagementBaseObject>();
            modelName = string.Empty;
            try
            {
                ManagementObjectCollection drivers = QueryMi(string.Format(@"SELECT DeviceID, Model FROM Win32_DiskDrive WHERE InterfaceType='{0}'", InterfaceType));
                // browse all USB WMI physical disks
                foreach (ManagementObject drive in drivers)
                {
                    modelName = (string)drive.GetPropertyValue("Model");
                    if (!modelName.Contains(model))
                        continue;

                    string DeviceID = (string)drive.GetPropertyValue("DeviceID");
                    ManagementObjectCollection partitions = QueryMi(string.Format(@"associators of {{Win32_DiskDrive.DeviceID='{0}'}} where AssocClass = Win32_DiskDriveToDiskPartition", DeviceID));
                    // associate physical disks with partitions
                    foreach (var partition in partitions)
                    {
                        DeviceID = (string)partition.GetPropertyValue("DeviceID");
                        // associate partitions with logical disks (drive letter volumes)
                        ManagementObjectCollection logicals = QueryMi(String.Format(@"associators of {{Win32_DiskPartition.DeviceID='{0}'}} where AssocClass = Win32_LogicalDiskToPartition", DeviceID));
                        foreach (var logical in logicals)
                        {
                            var logicalName = (string)logical.GetPropertyValue("Name");
                            // finally find the logical disk entry to determine the volume name
                            ManagementObjectCollection volumes = QueryMi(String.Format(@"select * from Win32_LogicalDisk where Name='{0}'", logicalName));
                            foreach (var volume in volumes)
                                disks.Add(volume);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }

            return disks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usbtype"></param>
        /// <param name="devicetype"></param>
        /// <param name="vid"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static IList<ManagementBaseObject> GetDevices(string usbtype = "", string devicetype = "", string vid = "", string pid = "")
        {
            var usbDeviceAddresses = LookUpUsbDeviceAddresses(usbtype, devicetype);

            var usbDevices = new List<ManagementBaseObject>();
            var format = string.Empty;
            if (!string.IsNullOrEmpty(vid + pid))
                format = string.Format("VID_{0}&PID_{1}", vid, pid);

            foreach (string usbDeviceAddress in usbDeviceAddresses.Where(w => w.Contains(format)))
            {
                // query MI for the PNP device info
                // address must be escaped to be used in the query; luckily, the form we extracted previously is already escaped
                ManagementObjectCollection curMoc = QueryMi(@"Select * from Win32_PnPEntity where PNPDeviceID = " + usbDeviceAddress);
                foreach (ManagementObject device in curMoc)
                {
                    usbDevices.Add(device);
                }
            }

            return usbDevices;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="InterfaceType"></param>
        /// <param name="devicetype"></param>
        /// <returns></returns>
        private static IList<string> LookUpUsbDeviceAddresses(string InterfaceType, string devicetype)
        {
            List<string> usbDeviceAddresses = new List<string>();
            try
            {
                // this query gets the addressing information for connected USB devices
                ManagementObjectCollection usbDeviceAddressInfo = QueryMi(@"Select * from Win32_USBControllerDevice");

                foreach (var device in usbDeviceAddressInfo)
                {
                    string curPnpAddress = (string)device.GetPropertyValue("Dependent");
                    // split out the address portion of the data; note that this includes escaped backslashes and quotes
                    curPnpAddress = curPnpAddress.Split(new String[] { "DeviceID=" }, 2, StringSplitOptions.None)[1];
                    if (!string.IsNullOrWhiteSpace(InterfaceType) && !curPnpAddress.Contains(InterfaceType))
                        continue;
                    if (!string.IsNullOrWhiteSpace(devicetype) && !curPnpAddress.Contains(devicetype))
                        continue;
                    usbDeviceAddresses.Add(curPnpAddress);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(LogTypes.Exception, "", ex);
            }
            return usbDeviceAddresses;
        }

        /// <summary>
        /// run a query against Windows Management Infrastructure (MI) and return the resulting collection
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private static ManagementObjectCollection QueryMi(string query)
        {
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection result = managementObjectSearcher.Get();

            managementObjectSearcher.Dispose();
            return result;
        }
    }


    [DefaultValue(USB)]
    public enum DeviceType
    {
        MOBILE,
        USB,
    }

    /// <summary>
    /// 장치정보
    /// </summary>
    public class DeviceInfo : BindableAndDisposable
    {
        private const int KB = 1024;
        private const int MB = KB * 1000;
        private const int GB = MB * 1000;

        #region Fields
        private string _RegisterDateString;
        #endregion //Fields

        #region Properties
        public DeviceType DeviceType { get; protected set; } = DeviceType.USB;
        public DateTime RegisterDate { get; private set; }
        public string RegisterDateString
        {
            get { return _RegisterDateString; }
            set { SetValue(ref _RegisterDateString, value); }
        }
        public string ComNum { get; set; }
        public string DeviceID { get { return GetPropertyData(); } }
        public string PNPDeviceID { get { return GetPropertyData(); } }
        public string Description { get { return GetPropertyData(); } }
        public virtual string Name { get { return GetPropertyData(); } }
        public virtual string Serial { get { return GetPropertyData(); } }

        /// <summary>
        /// Gets the available free space on the disk, specified in bytes.
        /// </summary>
        public ulong FreeSpace
        {
            get
            {
                var free = GetPropertyData();
                return ulong.TryParse(free, out ulong result) ? result : default;
            }
        }

        /// <summary>
        /// Gets the total size of the disk, specified in bytes.
        /// </summary>
        public ulong Size
        {
            get
            {
                var size = GetPropertyData();
                return ulong.TryParse(size, out ulong result) ? result : default;
            }
        }

        /// <summary>
        /// Get the volume name of this disk.  This is the friently name ("Stick").
        /// </summary>
        /// <remarks>
        /// When this class is used to identify a removed USB device, the Volume
        /// property is set to String.Empty.
        /// </remarks>
        public string Volume { get { return GetPropertyData(); } }//VolumeSerialNumber
        public string VolumeName { get { return GetPropertyData(); } }
        public string VolumeSerialNumber { get { return GetPropertyData(); } }


        /// <summary>
        /// Get the model of this disk.  This is the manufacturer's name.
        /// </summary>
        /// <remarks>
        /// When this class is used to identify a removed USB device, the Model
        /// property is set to String.Empty.
        /// </remarks>
        public string Model { get; internal set; }

        private Dictionary<string, object> _Attributes = new Dictionary<string, object>();
        public Dictionary<string, object> Attributes
        {
            get { return _Attributes; }
            set { SetValue(ref _Attributes, value); }
        }
        #endregion //Properties

        private string GetPropertyData([CallerMemberName] string propertyName = "")
        {
            return Attributes.Count > 0 ? (Attributes[propertyName]?.ToString()) : null;
        }



        public DeviceInfo(DeviceInfo device)
        {
            this.Copy(device);
        }
        public DeviceInfo(string model, PropertyDataCollection collection = null)
        {
            Model = model;
            Attributes.Clear();
            if (collection != null)
            {
                try
                {
                    RegisterDate = DateTime.Now;
                    RegisterDateString = RegisterDate.ToString(Defined.DateSFormat);
                    foreach (var item in collection)
                    {
                        Attributes.Add(item.Name, item.Value);
                        Logger.WriteLog(LogTypes.Info, string.Format("{0}: {1}", item.Name, item.Value));
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLog(LogTypes.Exception, "", ex);
                }
            }
        }
        protected override void DisposeManaged()
        {
            Attributes.Clear();
            base.DisposeManaged();
        }

        /// <summary>
        /// Device Data Copy
        /// </summary>
        /// <param name="device"></param>
        protected virtual void Copy(DeviceInfo device)
        {
            if (device == null)
                return;
            DeviceType = device.DeviceType;
            RegisterDate = device.RegisterDate;
            RegisterDateString = device.RegisterDateString;
            Attributes = device.Attributes;
        }

        /// <summary>
        /// Pretty print the disk.
        /// </summary>
        /// <returns></returns>
        public string ToDiskString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Name);
            builder.Append(" ");
            builder.Append(Volume);
            builder.Append(" (");
            builder.Append(Model);
            builder.Append(") ");
            builder.Append(FormatByteCount(FreeSpace));
            builder.Append(" free of ");
            builder.Append(FormatByteCount(Size));
            return builder.ToString();
        }


        private string FormatByteCount(ulong bytes)
        {
            string format;
            if (bytes < KB)
            {
                format = String.Format("{0} Bytes", bytes);
            }
            else if (bytes < MB)
            {
                bytes /= KB;
                format = String.Format("{0} KB", bytes.ToString("N"));
            }
            else if (bytes < GB)
            {
                double dree = bytes / MB;
                format = String.Format("{0} MB", dree.ToString("N1"));
            }
            else
            {
                double gree = bytes / GB;
                format = String.Format("{0} GB", gree.ToString("N1"));
            }

            return format;
        }
    }
}
