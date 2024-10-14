using System;
using System.IO;
using System.Text;
using MeF.Client.Logging;
//using Microsoft.Web.Services3;

namespace MeF.Client.Util
{
    internal static class AttachmentUtil
    {
        public static void ExtractAttachment(byte[] file, string contentType, string extractFolder, string zipFilename)
        {
            MeFLog.LogInfo("AttachmentUtil: ExtractAttachment");
            StringBuilder trace = new StringBuilder();
            trace.AppendLine("-------------- AttachmentUtil --------------");
            trace.AppendLine(String.Format("Saving response zipfile attachment"));
            trace.AppendLine(String.Format("File Name: {0}", zipFilename + ".zip"));
            trace.AppendLine(String.Format("Folder: {0}", extractFolder));
            trace.AppendLine(String.Format("File Size: {0} ", file.Length.ToString() + "bytes"));
            string message = trace.ToString();
            MeFLog.LogDebug(message);

            File.WriteAllBytes(extractFolder + zipFilename + ".zip", file);
        }
    }
}