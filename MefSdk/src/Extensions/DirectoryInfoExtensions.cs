using System.Collections.Generic;
using System.IO;

namespace MeF.Client.Extensions
{
    /// <summary>
    ///
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// 	Gets all files in the directory matching one of the several (!) supplied patterns (instead of just one in the regular implementation).
        /// </summary>
        /// <param name = "directory">The directory.</param>
        /// <param name = "patterns">The patterns.</param>
        /// <returns>The matching files</returns>
        /// <remarks>
        /// 	This methods is quite perfect to be used in conjunction with the newly created FileInfo-Array extension methods.
        /// </remarks>
        /// <example>
        /// 	<code>
        /// 		var files = directory.GetFiles("*.txt", "*.xml");
        /// 	</code>
        /// </example>
        public static FileInfo[] GetFiles(this DirectoryInfo directory, params string[] patterns)
        {
            var files = new List<FileInfo>();
            foreach (var pattern in patterns)
                files.AddRange(directory.GetFiles(pattern));
            return files.ToArray();
        }

        /// <summary>
        /// 	Searches the provided directory recursively and returns the first file matching the provided pattern.
        /// </summary>
        /// <param name = "directory">The directory.</param>
        /// <param name = "pattern">The pattern.</param>
        /// <returns>The found file</returns>
        /// <example>
        /// 	<code>
        /// 		var directory = new DirectoryInfo(@"c:\");
        /// 		var file = directory.FindFileRecursive("win.ini");
        /// 	</code>
        /// </example>
        public static FileInfo FindFileRecursive(this DirectoryInfo directory, string pattern)
        {
            var files = directory.GetFiles(pattern);
            if (files.Length > 0)
                return files[0];

            foreach (var subDirectory in directory.GetDirectories())
            {
                var foundFile = subDirectory.FindFileRecursive(pattern);
                if (foundFile != null)
                    return foundFile;
            }
            return null;
        }

        // Many thanks to Keith Rull: http://www.keithrull.com/2008/02/04/AnotherRealWorldExampleOnWhenToUseExtensionMethods.aspx
        /// <summary>
        /// Get files using the specified pattern
        /// </summary>
        /// <param name="directoryInfo">the DirectoryInfo object</param>
        /// <param name="searchPatterns"> the list of search patterns</param>
        /// <returns>returns a list of FileInfo objects</returns>
        public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo directoryInfo, IEnumerable<string> searchPatterns)
        {
            List<FileInfo> matchingFiles = new List<FileInfo>();

            foreach (string pattern in searchPatterns)
            {
                matchingFiles.AddRange(directoryInfo.GetFiles(pattern));
            }

            return matchingFiles;
        }

        /// <summary>
        /// Creates the directory, if it does not already exist
        /// </summary>
        /// <param name="directoryInfo"></param>
        public static void Ensure(this DirectoryInfo directoryInfo)
        {
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
        }
    }
}