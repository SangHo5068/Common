using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;


namespace Common.Utilities
{
    [DefaultValue(Unknown)]
    public enum OSType
    {
        [Display(Name = "Unknown")]
        Unknown = -99,
        [Display(Name = "Windows 95")]
        Windows_95 = 0,
        [Display(Name = "Windows 95 OSR2")]
        Windows_95_OSR2,
        [Display(Name = "Windows 98")]
        Windows_98,
        [Display(Name = "Windows 98 Second Edition")]
        Windows_98_Second_Edition,
        [Display(Name = "Windows Me")]
        Windows_Me,
        [Display(Name = "Windows NT 3.51")]
        Windows_NT_3_51,
        [Display(Name = "Windows NT 4.0")]
        Windows_NT_4_0,
        [Display(Name = "Windows NT 4.0 Server")]
        Windows_NT_4_0_Server,
        [Display(Name = "Windows 2000")]
        Windows_2000,
        [Display(Name = "Windows XP")]
        Windows_XP,
        [Display(Name = "Windows Server 2003")]
        Windows_Server_2003,
        [Display(Name = "Windows Server 2003 R2")]
        Windows_Server_2003_R2,
        [Display(Name = "Windows Vista")]
        Windows_Vista,
        [Display(Name = "Windows Server 2008")]
        Windows_Server_2008,
        [Display(Name = "Windows Server 2008 R2")]
        Windows_Server_2008_R2,
        [Display(Name = "Windows 7")]
        Windows_7,
        [Display(Name = "Windows Server 2012")]
        Windows_Server_2012,
        [Display(Name = "Windows 8")]
        Windows_8,
        [Display(Name = "Windows Server 2012 R2")]
        Windows_Server_2012_R2,
        [Display(Name = "Windows 8.1")]
        Windows_8_1,
        [Display(Name = "Windows Server 2016")]
        Windows_Server_2016,
        [Display(Name = "Windows 10")]
        Windows_10,
        [Display(Name = "Windows 11")]
        Windows_11,
    }
    /// <summary>
    /// Provides detailed information about the host operating system.
    /// </summary>
    /// <remarks>
    /// Operating system        Version number  dwMajorVersion dwMinorVersion  Other
    /// Windows 10              10.0*           10              0               OSVERSIONINFOEX.wProductType == VER_NT_WORKSTATION
    /// Windows Server 2016     10.0*           10              0               OSVERSIONINFOEX.wProductType != VER_NT_WORKSTATION
    /// Windows 8.1             6.3*            6               3               OSVERSIONINFOEX.wProductType == VER_NT_WORKSTATION
    /// Windows Server 2012 R2  6.3*            6               3               OSVERSIONINFOEX.wProductType != VER_NT_WORKSTATION
    /// Windows 8               6.2             6               2               OSVERSIONINFOEX.wProductType == VER_NT_WORKSTATION
    /// Windows Server 2012     6.2             6               2               OSVERSIONINFOEX.wProductType != VER_NT_WORKSTATION
    /// Windows 7               6.1             6               1               OSVERSIONINFOEX.wProductType == VER_NT_WORKSTATION
    /// Windows Server 2008 R2  6.1             6               1               OSVERSIONINFOEX.wProductType != VER_NT_WORKSTATION
    /// Windows Server 2008     6.0             6               0               OSVERSIONINFOEX.wProductType != VER_NT_WORKSTATION
    /// Windows Vista           6.0             6               0               OSVERSIONINFOEX.wProductType == VER_NT_WORKSTATION
    /// Windows Server 2003 R2  5.2             5               2               GetSystemMetrics(SM_SERVERR2) != 0
    /// Windows Server 2003     5.2             5               2               GetSystemMetrics(SM_SERVERR2) == 0
    /// Windows XP              5.1             5               1               Not applicable
    /// Windows 2000            5.0             5               0               Not applicable
    /// </remarks>
    public static class OSInfo
    {
        #region BITS
        /// <summary>
        /// Determines if the current application is 32 or 64-bit.
        /// </summary>
        public static int Bits
        {
            get
            {
                return IntPtr.Size * 8;
            }
        }
        #endregion //BITS

        #region EDITION
        private static string s_Edition;
        /// <summary>
        /// Gets the edition of the operating system running on this computer.
        /// </summary>
        public static string Edition
        {
            get
            {
                if (s_Edition != null)
                    return s_Edition;  //***** RETURN *****//

                string edition = String.Empty;

                OperatingSystem osVersion = Environment.OSVersion;
                OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
                osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));

                if (GetVersionEx(ref osVersionInfo))
                {
                    int majorVersion = osVersion.Version.Major;
                    int minorVersion = osVersion.Version.Minor;
                    byte productType = osVersionInfo.wProductType;
                    short suiteMask = osVersionInfo.wSuiteMask;

                    #region VERSION 4
                    if (majorVersion == 4)
                    {
                        if (productType == VER_NT_WORKSTATION)
                        {
                            // Windows NT 4.0 Workstation
                            edition = "Workstation";
                        }
                        else if (productType == VER_NT_SERVER)
                        {
                            if ((suiteMask & VER_SUITE_ENTERPRISE) != 0)
                            {
                                // Windows NT 4.0 Server Enterprise
                                edition = "Enterprise Server";
                            }
                            else
                            {
                                // Windows NT 4.0 Server
                                edition = "Standard Server";
                            }
                        }
                    }
                    #endregion //VERSION 4

                    #region VERSION 5
                    else if (majorVersion == 5)
                    {
                        if (productType == VER_NT_WORKSTATION)
                        {
                            if ((suiteMask & VER_SUITE_PERSONAL) != 0)
                            {
                                // Windows XP Home Edition
                                edition = "Home";
                            }
                            else
                            {
                                // Windows XP / Windows 2000 Professional
                                edition = "Professional";
                            }
                        }
                        else if (productType == VER_NT_SERVER)
                        {
                            if (minorVersion == 0)
                            {
                                if ((suiteMask & VER_SUITE_DATACENTER) != 0)
                                {
                                    // Windows 2000 Datacenter Server
                                    edition = "Datacenter Server";
                                }
                                else if ((suiteMask & VER_SUITE_ENTERPRISE) != 0)
                                {
                                    // Windows 2000 Advanced Server
                                    edition = "Advanced Server";
                                }
                                else
                                {
                                    // Windows 2000 Server
                                    edition = "Server";
                                }
                            }
                            else
                            {
                                if ((suiteMask & VER_SUITE_DATACENTER) != 0)
                                {
                                    // Windows Server 2003 Datacenter Edition
                                    edition = "Datacenter";
                                }
                                else if ((suiteMask & VER_SUITE_ENTERPRISE) != 0)
                                {
                                    // Windows Server 2003 Enterprise Edition
                                    edition = "Enterprise";
                                }
                                else if ((suiteMask & VER_SUITE_BLADE) != 0)
                                {
                                    // Windows Server 2003 Web Edition
                                    edition = "Web Edition";
                                }
                                else
                                {
                                    // Windows Server 2003 Standard Edition
                                    edition = "Standard";
                                }
                            }
                        }
                    }
                    #endregion //VERSION 5

                    #region VERSION 6
                    else if (majorVersion == 6)
                    {
                        int ed;
                        if (GetProductInfo(majorVersion, minorVersion,
                            osVersionInfo.wServicePackMajor, osVersionInfo.wServicePackMinor,
                            out ed))
                        {
                            switch (ed)
                            {
                                case PRODUCT_BUSINESS:
                                    edition = "Business";
                                    break;
                                case PRODUCT_BUSINESS_N:
                                    edition = "Business N";
                                    break;
                                case PRODUCT_CLUSTER_SERVER:
                                    edition = "HPC Edition";
                                    break;
                                case PRODUCT_DATACENTER_SERVER:
                                    edition = "Datacenter Server";
                                    break;
                                case PRODUCT_DATACENTER_SERVER_CORE:
                                    edition = "Datacenter Server (core installation)";
                                    break;
                                case PRODUCT_ENTERPRISE:
                                    edition = "Enterprise";
                                    break;
                                case PRODUCT_ENTERPRISE_N:
                                    edition = "Enterprise N";
                                    break;
                                case PRODUCT_ENTERPRISE_SERVER:
                                    edition = "Enterprise Server";
                                    break;
                                case PRODUCT_ENTERPRISE_SERVER_CORE:
                                    edition = "Enterprise Server (core installation)";
                                    break;
                                case PRODUCT_ENTERPRISE_SERVER_CORE_V:
                                    edition = "Enterprise Server without Hyper-V (core installation)";
                                    break;
                                case PRODUCT_ENTERPRISE_SERVER_IA64:
                                    edition = "Enterprise Server for Itanium-based Systems";
                                    break;
                                case PRODUCT_ENTERPRISE_SERVER_V:
                                    edition = "Enterprise Server without Hyper-V";
                                    break;
                                case PRODUCT_HOME_BASIC:
                                    edition = "Home Basic";
                                    break;
                                case PRODUCT_HOME_BASIC_N:
                                    edition = "Home Basic N";
                                    break;
                                case PRODUCT_HOME_PREMIUM:
                                    edition = "Home Premium";
                                    break;
                                case PRODUCT_HOME_PREMIUM_N:
                                    edition = "Home Premium N";
                                    break;
                                case PRODUCT_HYPERV:
                                    edition = "Microsoft Hyper-V Server";
                                    break;
                                case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
                                    edition = "Windows Essential Business Management Server";
                                    break;
                                case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
                                    edition = "Windows Essential Business Messaging Server";
                                    break;
                                case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
                                    edition = "Windows Essential Business Security Server";
                                    break;
                                case PRODUCT_SERVER_FOR_SMALLBUSINESS:
                                    edition = "Windows Essential Server Solutions";
                                    break;
                                case PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
                                    edition = "Windows Essential Server Solutions without Hyper-V";
                                    break;
                                case PRODUCT_SMALLBUSINESS_SERVER:
                                    edition = "Windows Small Business Server";
                                    break;
                                case PRODUCT_STANDARD_SERVER:
                                    edition = "Standard Server";
                                    break;
                                case PRODUCT_STANDARD_SERVER_CORE:
                                    edition = "Standard Server (core installation)";
                                    break;
                                case PRODUCT_STANDARD_SERVER_CORE_V:
                                    edition = "Standard Server without Hyper-V (core installation)";
                                    break;
                                case PRODUCT_STANDARD_SERVER_V:
                                    edition = "Standard Server without Hyper-V";
                                    break;
                                case PRODUCT_STARTER:
                                    edition = "Starter";
                                    break;
                                case PRODUCT_STORAGE_ENTERPRISE_SERVER:
                                    edition = "Enterprise Storage Server";
                                    break;
                                case PRODUCT_STORAGE_EXPRESS_SERVER:
                                    edition = "Express Storage Server";
                                    break;
                                case PRODUCT_STORAGE_STANDARD_SERVER:
                                    edition = "Standard Storage Server";
                                    break;
                                case PRODUCT_STORAGE_WORKGROUP_SERVER:
                                    edition = "Workgroup Storage Server";
                                    break;
                                case PRODUCT_UNDEFINED:
                                    edition = "Unknown product";
                                    break;
                                case PRODUCT_ULTIMATE:
                                    edition = "Ultimate";
                                    break;
                                case PRODUCT_ULTIMATE_N:
                                    edition = "Ultimate N";
                                    break;
                                case PRODUCT_WEB_SERVER:
                                    edition = "Web Server";
                                    break;
                                case PRODUCT_WEB_SERVER_CORE:
                                    edition = "Web Server (core installation)";
                                    break;
                            }
                        }
                    }
                    #endregion //VERSION 6
                }

                s_Edition = edition;
                return edition;
            }
        }
        #endregion //EDITION

        #region NAME
        private static OSType s_Name = OSType.Unknown;
        /// <summary>
        /// Gets the name of the operating system running on this computer.
        /// 
        /// Operating System    Version Details         Version Number
        /// Windows 11          Windows 11 (21H2)       10.0.22000
        /// Windows 10          Windows 10 (21H2)       10.0.19044
        /// Windows 10          (21H1)                  10.0.19043
        /// Windows 10          (20H2)                  10.0.19042
        /// Windows 10          (2004)                  10.0.19041
        /// Windows 10          (1909)                  10.0.18363
 	    /// Windows 10          (1903)                  10.0.18362
 	    /// Windows 10          (1809)                  10.0.17763
 	    /// Windows 10          (1803)                  10.0.17134
 	    /// Windows 10          (1709)                  10.0.16299
 	    /// Windows 10          (1703)                  10.0.15063
 	    /// Windows 10          (1607)                  10.0.14393
 	    /// Windows 10          (1511)                  10.0.10586
 	    /// Windows 10                                  10.0.10240
        /// Windows 8           Windows 8.1 (Update 1)  6.3.9600
 	    /// Windows 8.1                                 6.3.9200
 	    /// Windows 8                                   6.2.9200
        /// Windows 7           Windows 7 SP1           6.1.7601
 	    /// Windows 7                                   6.1.7600
        /// Windows Vista       Windows Vista SP2       6.0.6002
 	    /// Windows Vista SP1                           6.0.6001
 	    /// Windows Vista                               6.0.6000
        /// Windows XP          Windows XP2             5.1.26003
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static OSType Name
        {
            get
            {
                if (s_Name != OSType.Unknown)
                    return s_Name;  //***** RETURN *****//

                OSType name = OSType.Unknown;

                OperatingSystem osVersion = Environment.OSVersion;
                OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX {
                    dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX))
                };

                if (GetVersionEx(ref osVersionInfo))
                {
                    int majorVersion = osVersion.Version.Major;
                    int minorVersion = osVersion.Version.Minor;

                    switch (osVersion.Platform)
                    {
                        case PlatformID.Win32Windows:
                            {
                                if (majorVersion == 4)
                                {
                                    string csdVersion = osVersionInfo.szCSDVersion;
                                    switch (minorVersion)
                                    {
                                        case 0:
                                            if (csdVersion == "B" || csdVersion == "C")
                                                name = OSType.Windows_95_OSR2;
                                            else
                                                name = OSType.Windows_95;
                                            break;
                                        case 10:
                                            if (csdVersion == "A")
                                                name = OSType.Windows_98_Second_Edition;
                                            else
                                                name = OSType.Windows_98;
                                            break;
                                        case 90:
                                            name = OSType.Windows_Me;
                                            break;
                                    }
                                }
                                break;
                            }

                        case PlatformID.Win32NT:
                            {
                                byte productType = osVersionInfo.wProductType;

                                switch (majorVersion)
                                {
                                    case 3:
                                        name = OSType.Windows_NT_3_51;
                                        break;
                                    case 4:
                                        switch (productType)
                                        {
                                            case 1:
                                                name = OSType.Windows_NT_4_0;
                                                break;
                                            case 3:
                                                name = OSType.Windows_NT_4_0_Server;
                                                break;
                                        }
                                        break;
                                    case 5:
                                        switch (minorVersion)
                                        {
                                            case 0:
                                                name = OSType.Windows_2000;
                                                break;
                                            case 1:
                                                name = OSType.Windows_XP;
                                                break;
                                            case 2:
                                                name = OSType.Windows_Server_2003;
                                                break;
                                        }
                                        break;
                                    case 6:
                                        switch (productType)
                                        {
                                            case 0:
                                                name = OSType.Windows_Vista;
                                                break;
                                            case 1:
                                                name = OSType.Windows_7;
                                                break;
                                            case 2:
                                                name = OSType.Windows_8;
                                                break;
                                            case 3:
                                                name = OSType.Windows_Server_2008;
                                                break;
                                        }
                                        break;
                                    case 10:
                                        if (osVersion.Version.Build >= 22000)
                                            name = OSType.Windows_11;
                                        else
                                            name = OSType.Windows_10;
                                        break;
                                }
                            }
                            break;
                    }
                }

                s_Name = name;
                return name;
            }
        }
        #endregion //NAME

        #region PINVOKE
        #region GET
        #region PRODUCT INFO
        [DllImport("Kernel32.dll")]
        internal static extern bool GetProductInfo(
            int osMajorVersion,
            int osMinorVersion,
            int spMajorVersion,
            int spMinorVersion,
            out int edition);
        #endregion //PRODUCT INFO

        #region VERSION
        [DllImport("kernel32.dll")]
        private static extern bool GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);
        #endregion //VERSION
        #endregion //GET

        #region OSVERSIONINFOEX
        [StructLayout(LayoutKind.Sequential)]
        private struct OSVERSIONINFOEX
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szCSDVersion;
            public short wServicePackMajor;
            public short wServicePackMinor;
            public short wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }
        #endregion //OSVERSIONINFOEX

        #region PRODUCT
        private const int PRODUCT_UNDEFINED = 0x00000000;
        private const int PRODUCT_ULTIMATE = 0x00000001;
        private const int PRODUCT_HOME_BASIC = 0x00000002;
        private const int PRODUCT_HOME_PREMIUM = 0x00000003;
        private const int PRODUCT_ENTERPRISE = 0x00000004;
        private const int PRODUCT_HOME_BASIC_N = 0x00000005;
        private const int PRODUCT_BUSINESS = 0x00000006;
        private const int PRODUCT_STANDARD_SERVER = 0x00000007;
        private const int PRODUCT_DATACENTER_SERVER = 0x00000008;
        private const int PRODUCT_SMALLBUSINESS_SERVER = 0x00000009;
        private const int PRODUCT_ENTERPRISE_SERVER = 0x0000000A;
        private const int PRODUCT_STARTER = 0x0000000B;
        private const int PRODUCT_DATACENTER_SERVER_CORE = 0x0000000C;
        private const int PRODUCT_STANDARD_SERVER_CORE = 0x0000000D;
        private const int PRODUCT_ENTERPRISE_SERVER_CORE = 0x0000000E;
        private const int PRODUCT_ENTERPRISE_SERVER_IA64 = 0x0000000F;
        private const int PRODUCT_BUSINESS_N = 0x00000010;
        private const int PRODUCT_WEB_SERVER = 0x00000011;
        private const int PRODUCT_CLUSTER_SERVER = 0x00000012;
        private const int PRODUCT_HOME_SERVER = 0x00000013;
        private const int PRODUCT_STORAGE_EXPRESS_SERVER = 0x00000014;
        private const int PRODUCT_STORAGE_STANDARD_SERVER = 0x00000015;
        private const int PRODUCT_STORAGE_WORKGROUP_SERVER = 0x00000016;
        private const int PRODUCT_STORAGE_ENTERPRISE_SERVER = 0x00000017;
        private const int PRODUCT_SERVER_FOR_SMALLBUSINESS = 0x00000018;
        private const int PRODUCT_SMALLBUSINESS_SERVER_PREMIUM = 0x00000019;
        private const int PRODUCT_HOME_PREMIUM_N = 0x0000001A;
        private const int PRODUCT_ENTERPRISE_N = 0x0000001B;
        private const int PRODUCT_ULTIMATE_N = 0x0000001C;
        private const int PRODUCT_WEB_SERVER_CORE = 0x0000001D;
        private const int PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 0x0000001E;
        private const int PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 0x0000001F;
        private const int PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 0x00000020;
        private const int PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 0x00000023;
        private const int PRODUCT_STANDARD_SERVER_V = 0x00000024;
        private const int PRODUCT_ENTERPRISE_SERVER_V = 0x00000026;
        private const int PRODUCT_STANDARD_SERVER_CORE_V = 0x00000028;
        private const int PRODUCT_ENTERPRISE_SERVER_CORE_V = 0x00000029;
        private const int PRODUCT_HYPERV = 0x0000002A;
        #endregion //PRODUCT

        #region VERSIONS
        private const int VER_NT_WORKSTATION = 1;
        private const int VER_NT_DOMAIN_CONTROLLER = 2;
        private const int VER_NT_SERVER = 3;
        private const int VER_SUITE_SMALLBUSINESS = 1;
        private const int VER_SUITE_ENTERPRISE = 2;
        private const int VER_SUITE_TERMINAL = 16;
        private const int VER_SUITE_DATACENTER = 128;
        private const int VER_SUITE_SINGLEUSERTS = 256;
        private const int VER_SUITE_PERSONAL = 512;
        private const int VER_SUITE_BLADE = 1024;
        #endregion //VERSIONS
        #endregion //PINVOKE

        #region SERVICE PACK
        /// <summary>
        /// Gets the service pack information of the operating system running on this computer.
        /// </summary>
        public static string ServicePack
        {
            get
            {
                string servicePack = String.Empty;
                OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX
                {
                    dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX))
                };

                if (GetVersionEx(ref osVersionInfo))
                {
                    servicePack = osVersionInfo.szCSDVersion;
                }

                return servicePack;
            }
        }
        #endregion //SERVICE PACK

        #region VERSION
        #region BUILD
        /// <summary>
        /// Gets the build version number of the operating system running on this computer.
        /// </summary>
        public static int BuildVersion
        {
            get
            {
                return Environment.OSVersion.Version.Build;
            }
        }
        #endregion //BUILD

        #region FULL
        #region STRING
        /// <summary>
        /// Gets the full version string of the operating system running on this computer.
        /// </summary>
        public static string VersionString
        {
            get
            {
                return Environment.OSVersion.Version.ToString();
            }
        }
        #endregion //STRING

        #region VERSION
        /// <summary>
        /// Gets the full version of the operating system running on this computer.
        /// </summary>
        public static Version Version
        {
            get
            {
                return Environment.OSVersion.Version;
            }
        }
        #endregion //VERSION
        #endregion //FULL

        #region MAJOR
        /// <summary>
        /// Gets the major version number of the operating system running on this computer.
        /// </summary>
        public static int MajorVersion
        {
            get
            {
                return Environment.OSVersion.Version.Major;
            }
        }
        #endregion //MAJOR

        #region MINOR
        /// <summary>
        /// Gets the minor version number of the operating system running on this computer.
        /// </summary>
        public static int MinorVersion
        {
            get
            {
                return Environment.OSVersion.Version.Minor;
            }
        }
        #endregion MINOR

        #region REVISION
        /// <summary>
        /// Gets the revision version number of the operating system running on this computer.
        /// </summary>
        public static int RevisionVersion
        {
            get
            {
                return Environment.OSVersion.Version.Revision;
            }
        }
        #endregion //REVISION
        #endregion //VERSION
    }
}
