using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilities
{
    public class GitHelper
    {
        /// <summary>IsDirty: true</summary>
        public static bool IsDirty { get => ThisAssembly.Git.IsDirty; }

        /// <summary>IsDirtyString: true</summary>
        public static String IsDirtyString { get => ThisAssembly.Git.IsDirtyString; }

        /// <summary>Repository URL: https://github.com/SangHo5068/Common.git</summary>
        public static String RepositoryUrl { get => ThisAssembly.Git.RepositoryUrl; }

        /// <summary>Branch: master</summary>
        public static String Branch { get => ThisAssembly.Git.Branch; }

        /// <summary>Commit: 9fb8850</summary>
        public static String Commit { get => ThisAssembly.Git.Commit; }

        /// <summary>Sha: 9fb88505c801a7677a0444bd3a9e1dc9c1cb1fe8</summary>
        public static String Sha { get => ThisAssembly.Git.Sha; }

        /// <summary>Commit date: 2021-05-20T00:32:55+09:00</summary>
        public static String CommitDate { get => ThisAssembly.Git.CommitDate; }

        /// <summary>Commits on top of base version: 9</summary>
        public static String Commits { get => ThisAssembly.Git.Commits; }

        /// <summary>Tag: </summary>
        public static String Tag { get => ThisAssembly.Git.Tag; }

        /// <summary>Base tag: </summary>
        public static String BaseTag { get => ThisAssembly.Git.BaseTag; }

        /// <summary>Provides access to SemVer information for the current assembly.</summary>
        public class SemVer
        {
            /// <summary>Major: 0</summary>
            public static String Major { get => ThisAssembly.Git.SemVer.Major; }

            /// <summary>Minor: 0</summary>
            public static String Minor { get => ThisAssembly.Git.SemVer.Minor; }

            /// <summary>Patch: 9</summary>
            public static String Patch { get => ThisAssembly.Git.SemVer.Patch; }

            /// <summary>Label: </summary>
            public static String Label { get => ThisAssembly.Git.SemVer.Label; }

            /// <summary>Label with dash prefix: </summary>
            public static String DashLabel { get => ThisAssembly.Git.SemVer.DashLabel; }

            /// <summary>Source: Default</summary>
            public static String Source { get => ThisAssembly.Git.SemVer.Source; }
        }

        public static String GitVersionToString()
        {
            var format = "{0}.{1}.{2}-{3}+{4}";
            return String.Format(format, SemVer.Major, SemVer.Minor, SemVer.Patch, Branch, Commit);
        }
    }
}
