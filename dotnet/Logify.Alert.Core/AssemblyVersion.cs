﻿using System;

namespace DevExpress.Logify.Core {
    public static class AssemblyInfo {
        public const string AssemblyCopyright = "Copyright (c) 2000-2016 Developer Express Inc.";
        public const string AssemblyCompany = "Developer Express Inc.";
        public const string AssemblyProduct = "DevExpress Logify Alert";
        public const string AssemblyTrademark = "DevExpress Logify Alert";

        public const string VersionShort = "1.0";

#if SILVERLIGHT
    public const string Version = VersionShort + ".0.5"; //SL
#elif NETFX_CORE
    public const string Version = VersionShort + ".0.3";
#else
        public const string Version = VersionShort + ".23";
#endif


        public const string FileVersion = Version;
        public const string SatelliteContractVersion = VersionShort + ".0";
    }
}
