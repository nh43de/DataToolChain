using System;
using System.IO;
using DataPowerTools.Strings;

namespace DataPowerTools.FileSystem
{
    /// <summary>
    /// Provides classes for scanning directories and performing actions based on files found. Can be optimized to be made parallel. (Add scanparallel methods).
    /// </summary>
    public static class DirectoryScanner
    {
        public static void ScanRecursive(string rootDir, Action<FileInfo> fileAction)
        {
            //recurse dirs too
            var dirs = Directory.GetDirectories(rootDir);
            foreach (var d in dirs)
                ScanRecursive(d, fileAction);

            ScanStandard(rootDir, fileAction);
        }

        public static void ScanStandard(string rootDir, Action<FileInfo> fileAction)
        {
            var files = new DirectoryInfo(rootDir).GetFiles();

            foreach (var file in files)
                fileAction(file);
        }

        public static void ScanRecursive(string rootDir, Action<string> fileAction)
        {
            //recurse dirs too
            var dirs = Directory.GetDirectories(rootDir);
            foreach (var d in dirs)
                ScanRecursive(d, fileAction);

            ScanStandard(rootDir, fileAction);
        }

        public static void ScanStandard(string rootDir, Action<string> fileAction)
        {
            var files = Directory.GetFiles(rootDir);

            foreach (var file in files)
                fileAction(file);
        }
    }
}