using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MeF.Client.Extensions
{
    /// <summary>
    ///
    /// </summary>
    public static class FileInfoExtensions
    {
        /// <summary>
        /// Deserializes the Binary data contained in the specified file.
        /// </summary>
        /// <typeparam name="T">The type of System.Object to be deserialized.</typeparam>
        /// <param name="fileInfo">This System.IO.FileInfo instance.</param>
        /// <returns>The System.Object being deserialized.</returns>
        public static T BinaryDeserialize<T>(this FileInfo fileInfo) where T : ISerializable
        {
            using (Stream stream = File.Open(fileInfo.FullName, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                T item = (T)binaryFormatter.Deserialize(stream);
                stream.Close();
                return item;
            }
        }

        public static byte[] GetBytes(this FileInfo fileInfo)
        {
            return fileInfo.GetBytes(0x1000);
        }

        /// <summary>
        /// Gets the file data as an array of bytes
        /// </summary>
        /// <param name="fileInfo">This System.IO.FileInfo instance.</param>
        /// <param name="maxBufferSize">The buffer size.</param>
        /// <returns>System.Byte[] representing the file data.</returns>
        public static byte[] GetBytes(this FileInfo fileInfo, long maxBufferSize)
        {
            byte[] buffer = new byte[(fileInfo.Length > maxBufferSize ? maxBufferSize : fileInfo.Length)];
            using (FileStream fileStream = new FileStream(
                fileInfo.FullName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                buffer.Length))
            {
                fileStream.Read(buffer, 0, buffer.Length);
                fileStream.Close();
            }
            return buffer;
        }

        /// <summary>
        /// Gets the file size in KiloBytes
        /// </summary>
        /// <param name="fileInfo">This System.IO.FileInfo instance.</param>
        /// <returns>System.Double representing the size of the file.</returns>
        public static double GetFileSizeInKiloBytes(this FileInfo fileInfo)
        {
            return (double)fileInfo.Length / 1024;
        }

        /// <summary>
        /// Gets the file size in MegaBytes
        /// </summary>
        /// <param name="fileInfo">This System.IO.FileInfo instance.</param>
        /// <returns>System.Double representing the size of the file.</returns>
        public static double GetFileSizeInMegaBytes(this FileInfo fileInfo)
        {
            return fileInfo.GetFileSizeInKiloBytes() / 1024;
        }

        /// <summary>
        /// Gets the file size in GigaBytes
        /// </summary>
        /// <param name="fileInfo">This System.IO.FileInfo instance.</param>
        /// <returns>System.Double representing the size of the file.</returns>
        public static double GetFileSizeInGigaBytes(this FileInfo fileInfo)
        {
            return fileInfo.GetFileSizeInMegaBytes() / 1024;
        }
    }
}