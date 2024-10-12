using System;
using System.IO;
using MeF.Client.Exceptions;

namespace MeF.Client.Helpers
{
    public sealed class StreamingHelper
    {
        #region SaveStreamToFile

        /// <summary>
        /// Saves a stream to a file
        /// </summary>
        /// <param name="stream">stream to save</param>
        /// <param name="filePath">output folder and file to write the stream to</param>
        //public static void SaveStreamToFile(Stream stream, string filePath)
        //{
        //    using (FileStream outstream = File.Open(filePath, FileMode.Create, FileAccess.Write))
        //    {
        //        CopyStream(stream, outstream);
        //        outstream.Close();
        //        stream.Close();
        //    }
        //}

        #endregion SaveStreamToFile

        #region CopyStream

        /// <summary>
        /// read from the input stream in 4K chunks and save to output stream
        /// </summary>
        /// <param name="instream">input stream</param>
        /// <param name="outstream">output stream</param>
        /*
        private static void CopyStream(Stream instream, Stream outstream)
        {
            const int bufferLen = 4096;
            byte[] buffer = new byte[bufferLen];
            int count = 0;
            int bytecount = 0;
            while ((count = instream.Read(buffer, 0, bufferLen)) > 0)
            {
                outstream.Write(buffer, 0, count);
                bytecount += count;
            }
        }
        */
        #endregion CopyStream

        #region ConvertToBytes

        /// <summary>
        /// Converts the given filestructure to a byte[]
        /// </summary>
        /// <param name="FileName">Filename to process</param>
        /// <returns>byte[]</returns>
        public static byte[] ConvertToBytes(string FileName)
        {
            //Create an instance of the FileInfo class
            FileInfo FInfo = new FileInfo(FileName);

            // Binary reader for the selected file
            using (BinaryReader br = new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read)))
            {
               

                // convert the file to a byte array
                byte[] data = br.ReadBytes((int)FInfo.Length);

                //close the binaryreader
                br.Close();

                //return the byte[]
                return data;
            }
        }

        #endregion ConvertToBytes

        public static byte[] Chunk(string FileName)
        {
            string FilePath = FileName;

            // check that requested file exists
            if (!File.Exists(FilePath))

                throw new ToolkitException("File not found", String.Format("The file {0} does not exist", FilePath));

            long FileSize = new FileInfo(FilePath).Length;
            int BufferSize = 16 * 1024;
            // open the file to return the requested chunk as a byte[]
            byte[] TmpBuffer;
            int BytesRead;

            try
            {
                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    TmpBuffer = new byte[BufferSize];
                    BytesRead = fs.Read(TmpBuffer, 0, BufferSize);	// read the first chunk in the buffer (which is re-used for every chunk)
                }
                if (BytesRead != BufferSize)
                {
                    // the last chunk will almost certainly not fill the buffer, so it must be trimmed before returning
                    byte[] TrimmedBuffer = new byte[BytesRead];
                    Array.Copy(TmpBuffer, TrimmedBuffer, BytesRead);
                    return TrimmedBuffer;
                }
                else
                    return TmpBuffer;
            }
            catch (IOException ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Reads all bytes.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        /*
        public static byte[] ReadAllBytes(String path)
        {
            byte[] bytes;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int index = 0;
                long fileLength = fs.Length;
                if (fileLength > Int32.MaxValue)
                    throw new IOException("File too Long");
                int count = (int)fileLength;
                bytes = new byte[count];
                while (count > 0)
                {
                    int n = fs.Read(bytes, index, count);
                    if (n == 0)
                        throw new InvalidOperationException("End of file reached before expected");
                    index += n;
                    count -= n;
                }
            }
            return bytes;
        }
        */
    }
}