// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.DotNet.Docker.Tests
{
    public class ImageData
    {
        private Version _runtimeDepsVersion;
        private Version _sdkVersion;

        public Arch Arch { get; set; }
        public Version Version { get; set; }
        public string VersionString { get => Version.ToString(2); }
        public bool HasNoSdk { get => SdkVersion != Version; }
        public bool IsWeb { get; set; }
        public bool IsArm { get => Arch == Arch.Arm || Arch == Arch.Arm64; }
        public string OS { get; set; }

        public string Rid
        {
            get {
                string rid;

                if (Arch == Arch.Arm)
                {
                    rid = "linux-arm";
                }
                else if (Arch == Arch.Arm64)
                {
                    rid = "linux-arm64";
                }
                else if (OS.StartsWith("alpine"))
                {
                    rid = "linux-musl-x64";
                }
                else if (Version.Major == 1)
                {
                    rid = OS == Tests.OS.Jessie ? "debian.8-x64" : "debian.9-x64";;
                }
                else
                {
                    rid = "linux-x64";
                }

                return rid;
            }
        }

        public Version RuntimeDepsVersion
        {
            get { return _runtimeDepsVersion ?? Version; }
            set { _runtimeDepsVersion = value; }
        }

        public string SdkOS { get => OS == Tests.OS.StretchSlim ? Tests.OS.Stretch : OS; }
        
        public Version SdkVersion
        {
            get { return _sdkVersion ?? Version; }
            set { _sdkVersion = value; }
        }

        public string GetTag(DotNetImageType imageType)
        {
            Version imageVersion;
            string os;
            string variantName = Enum.GetName(typeof(DotNetImageType), imageType).ToLowerInvariant().Replace('_', '-');

            switch (imageType)
            {
                case DotNetImageType.Runtime:
                case DotNetImageType.AspNetCore_Runtime:
                    imageVersion = Version;
                    os = OS;
                    break;
                case DotNetImageType.Runtime_Deps:
                    imageVersion = RuntimeDepsVersion;
                    os = OS;
                    break;
                case DotNetImageType.SDK:
                    imageVersion = SdkVersion;
                    os = SdkOS;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported image type '{variantName}'");
            }

            string arch = string.Empty;
            if (Arch == Arch.Arm)
            {
                arch = $"-arm32v7";
            }
            else if (Arch == Arch.Arm64)
            {
                arch = $"-arm64v8";
            }

            return $"{imageVersion.ToString(2)}-{variantName}-{os}{arch}";
        }

        public override string ToString()
        {
            return typeof(ImageData).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(propInfo => $"{propInfo.Name}='{propInfo.GetValue(this) ?? "<null>"}'")
                .Aggregate((working, next) => $"{working}, {next}");
        }
    }
}
