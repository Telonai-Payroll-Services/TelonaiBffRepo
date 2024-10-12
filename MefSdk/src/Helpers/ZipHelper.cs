using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace MeF.Client.Helpers
{
    internal static class ZipHelper
    {
        /// <summary>
        /// A method that creates a zip file
        /// </summary>
        /// <param name="zipFileStoragePath">the storage location</param>
        /// <param name="zipFileName">the zip file name</param>
        /// <param name="fileToCompress">the file to compress</param>
        public static void CreateZipFile(string zipFileStoragePath
            , string zipFileName
            , string fileToCompress)
        {
                ZipFile z = null;
                try{
                    
                    //create our zip file
                    z = ZipFile.Create(zipFileStoragePath + zipFileName);

                    //initialize the file so that it can accept updates
                    z.BeginUpdate();

                    //add the file to the zip file
                    z.Add(fileToCompress, Path.GetFileName(fileToCompress));
                    
                    //commit the update once we are done
                    z.CommitUpdate();
                }
           finally{
            z.Close();
           }
        }

        /*
        public static void SetZipProperties(string innerZipFilename)
        {
            FileStream file = null;
            try
            {
                file = File.Open(innerZipFilename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                ZipEntry zipE = new ZipEntry(innerZipFilename);
                var s = zipE.Flags.ToString();

                zipE.CompressionMethod = CompressionMethod.Stored;
                zipE.CompressedSize = file.Length;
                zipE.Size = file.Length;
            }
            finally
            {
                file.Close();
            }
        }
        */
    }
}