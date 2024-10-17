using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using MeF.Client.Exceptions;
using MeF.Client.Logging;
using MeF.Client.Services.TransmitterServices;
using MeF.Client.Validation;
using MeF.Client.Util;

namespace MeF.Client.Services.InputComposition
{
    /// <summary>
    /// Utility Class containing methods for creating SubmissionArchives in the correct format.
    /// </summary>
    public class SubmissionBuilder
    {
        private MeFLog Log = MeFLog.Instance();

        /// <summary>
        /// Helper API to create IRS Submission Archive
        /// </summary>
        /// <param name="SubmissionID"></param>
        /// <param name="SubmissionManifest"></param>
        /// <param name="SubmissionXML"></param>
        /// <param name="Attachments"></param>
        /// <param name="archiveOutputLocation"></param>
        /// <returns></returns>
        public IRSSubmissionArchive CreateIRSSubmissionArchive(string SubmissionID,
            SubmissionManifest SubmissionManifest, SubmissionXml SubmissionXML,
            List<BinaryAttachment> Attachments, string archiveOutputLocation)
        {
            MeFLog.LogDebug("Enter CreateIRSSubmissionArchive");
            Validate.IsSubmissionIDValid(SubmissionID, "Invalid Submission ID", "CreateIRSSubmissionArchive");
            IRSSubmissionArchive arch = new IRSSubmissionArchive(
                                        SubmissionID,
                                        SubmissionManifest,
                                        SubmissionXML,
                                        Attachments,
                                        archiveOutputLocation);
            arch.zipFileName = CreateIRSInnerZip(arch);
            MeFLog.LogDebug("Exit CreateIRSSubmissionArchive");

            return arch;
        }

        /// <summary>
        /// Helper API to create IRS Submission Archive
        /// </summary>
        /// <param name="SubmissionID"></param>
        /// <param name="SubmissionManifest"></param>
        /// <param name="SubmissionXML"></param>
        /// <param name="Attachments"></param>        
        /// <returns></returns>
        public IRSSubmissionArchive CreateIRSSubmissionArchive(string SubmissionID,
            SubmissionManifest SubmissionManifest, SubmissionXml SubmissionXML,
            List<BinaryAttachment> Attachments)
        {
            MeFLog.LogDebug("Enter CreateIRSSubmissionArchive");
            Validate.IsSubmissionIDValid(SubmissionID, "Invalid Submission ID", "CreateIRSSubmissionArchive");
            IRSSubmissionArchive arch = new IRSSubmissionArchive(
                                        SubmissionID,
                                        SubmissionManifest,
                                        SubmissionXML,
                                        Attachments);                                        
            
            MeFLog.LogDebug("Exit CreateIRSSubmissionArchive");
            return arch;
        }

        /// <summary>
        /// Helper API to create State Submission Archive
        /// </summary>
        /// <param name="SubmissionID"></param>
        /// <param name="SubmissionManifest"></param>
        /// <param name="SubmissionXML"></param>
        /// <param name="Attachments"></param>
        /// <param name="IrsSubmissionXML"></param>
        /// <param name="IrsAttachments"></param>
        /// <param name="archiveOutputLocation"></param>
        /// <returns></returns>
        public StateSubmissionArchive CreateStateSubmissionArchive(string SubmissionID,
            SubmissionManifest SubmissionManifest, SubmissionXml SubmissionXML,
            List<BinaryAttachment> Attachments, SubmissionXml IrsSubmissionXML,
            List<BinaryAttachment> IrsAttachments, string archiveOutputLocation)
        {
            MeFLog.LogDebug("Enter CreateStateSubmissionArchive");
            Validate.IsSubmissionIDValid(SubmissionID, "Invalid Submission ID", "CreateStateSubmissionArchive");
            StateSubmissionArchive arch = new StateSubmissionArchive(SubmissionID,
                                        SubmissionManifest, SubmissionXML,
                                        Attachments, archiveOutputLocation, IrsSubmissionXML,
                                        IrsAttachments);
            arch.zipFileName = CreateStateInnerZip(arch);
            MeFLog.LogDebug("Exit CreateStateSubmissionArchive");
            return arch;
        }

        /// <summary>
        /// Helper API to create State Submission Archive
        /// </summary>
        /// <param name="SubmissionID"></param>
        /// <param name="SubmissionManifest"></param>
        /// <param name="SubmissionXML"></param>
        /// <param name="Attachments"></param>
        /// <param name="IrsSubmissionXML"></param>
        /// <param name="IrsAttachments"></param>
        /// <returns></returns>
        public StateSubmissionArchive CreateStateSubmissionArchive(string SubmissionID,
            SubmissionManifest SubmissionManifest, SubmissionXml SubmissionXML,
            List<BinaryAttachment> Attachments, SubmissionXml IrsSubmissionXML,
            List<BinaryAttachment> IrsAttachments)
        {
            MeFLog.LogDebug("Enter CreateStateSubmissionArchive");
            Validate.IsSubmissionIDValid(SubmissionID, "Invalid Submission ID", "CreateStateSubmissionArchive");
            StateSubmissionArchive arch = new StateSubmissionArchive(SubmissionID,
                                        SubmissionManifest, SubmissionXML,
                                        Attachments, IrsSubmissionXML, IrsAttachments);
            MeFLog.LogDebug("Exit CreateStateSubmissionArchive");
            return arch;
        }

        /// <summary>
        /// Helper API to create IRS Submission Archive based on any existing archive zip file
        /// </summary>
        /// <param name="SubmissionZipFilename"></param>
        /// <returns></returns>
        public IRSSubmissionArchive CreateIRSSubmissionArchive(string SubmissionZipFilename)
        {
            MeFLog.LogDebug("Enter CreateIRSSubmissionArchive with Zipfile" );
            IRSSubmissionArchive arch;
            if (File.Exists(SubmissionZipFilename))
            {
                arch = new IRSSubmissionArchive();
                arch.submissionId = GetFilename(SubmissionZipFilename).Replace(".zip", "");
                arch.zipFileName = SubmissionZipFilename;
            }
            else
            {
                //string msg = ErrorMessages.MeFClientSDK000004.getErrorMessage() +
                string msg = "File not found" +
                "; SubmissionBuilder; zip file";
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                throw new ToolkitException(msg);
            }
            MeFLog.LogDebug("Exit CreateIRSSubmissionArchive with Zipfile");
            return arch;
        }

        /// <summary>
        /// Helper API to create IRS Submission Archive based on any existing archive zip file
        /// </summary>
        /// <param name="SubmissionZipFile"></param>
        /// <returns></returns>
        public IRSSubmissionArchive CreateIRSSubmissionArchive(FileStream SubmissionZipFile)
        {   
            if (SubmissionZipFile == null)
            {
                //string msg = ErrorMessages.MeFClientSDK000004.getErrorMessage() +
                string msg = "File is empty or null" +
                "; SubmissionBuilder; zip file";
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                throw new ToolkitException(msg);
            }

            ZipFile zf = new ZipFile(SubmissionZipFile);
            foreach (ZipEntry zipEntry in zf)
            {
                if (!ZipStreamReader.isValidPath(zipEntry.Name))
                {
                    string msg = "File path inside zip is invalid" + "; SubmissionBuilder; zip file";
                    MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                    throw new ToolkitException(msg);
                }                
            }

            return CreateIRSSubmissionArchive(SubmissionZipFile.Name);
        }

        /// <summary>
        /// Helper API to create State Submission Archive based on any existing archive zip file
        /// </summary>
        /// <param name="SubmissionZipFilename"></param>
        /// <returns></returns>
        public StateSubmissionArchive CreateStateSubmissionArchive(string SubmissionZipFilename)
        {
            MeFLog.LogDebug("CreateStateSubmissionArchive using direct Zip");

            StateSubmissionArchive arch;
            if (File.Exists(SubmissionZipFilename))
            {
                arch = new StateSubmissionArchive();
                arch.submissionId = GetFilename(SubmissionZipFilename).Replace(".zip", "");
                arch.zipFileName = SubmissionZipFilename;
            }
            else
            {
                //string msg = ErrorMessages.MeFClientSDK000004.getErrorMessage() +
                string msg = "File not found" +
                "; SubmissionBuilder; zip file";
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                throw new ToolkitException(msg);
            }
            return arch;
        }

        /// <summary>
        /// Helper API to create State Submission Archive based on any existing archive zip file
        /// </summary>
        /// <param name="SubmissionZipFile"></param>
        /// <returns></returns>
        public StateSubmissionArchive CreateStateSubmissionArchive(FileStream SubmissionZipFile)
        {
            MeFLog.LogDebug("CreateStateSubmissionArchive using direct Zip stream");

            if (SubmissionZipFile == null)
            {
                //string msg = ErrorMessages.MeFClientSDK000004.getErrorMessage() +
                string msg = "File is empty or null" +
                "; SubmissionBuilder; zip file";
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                throw new ToolkitException(msg);
            }

            ZipFile zf = new ZipFile(SubmissionZipFile);
            foreach (ZipEntry zipEntry in zf)
            {
                if (zipEntry.IsFile)
                {
                    if (!ZipStreamReader.isValidPath(zipEntry.Name))
                    {
                        string msg = "File path inside zip is invalid" + "; SubmissionBuilder; zip file";
                        MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                        throw new ToolkitException(msg);
                    }
                }
            }

            return CreateStateSubmissionArchive(SubmissionZipFile.Name);
        }

        private string GetFilename(string fullPath)
        {
            int index = fullPath.LastIndexOf(@"\");
            if (index == -1)
                index = fullPath.LastIndexOf(@"/");
            if (index == -1)
                return fullPath.ToLower();
            else
                return fullPath.Substring(index + 1).ToLower();
        }

        /// <summary>
        /// Creates the postmarked submission archive.
        /// </summary>
        /// <param name="submissionArchive">The submission archive.</param>
        /// <param name="postMarkDate">The post mark date.</param>
        /// <returns></returns>
        public PostmarkedSubmissionArchive CreatePostmarkedSubmissionArchive(SubmissionArchive submissionArchive, DateTime postMarkDate)
        {
            return new PostmarkedSubmissionArchive(submissionArchive, postMarkDate);
        }

        /// <summary>
        /// Creates the submission container.
        /// </summary>
        /// <param name="colPostMarkedArchives">The col post marked archives.</param>
        /// <param name="archiveOutputLocation">The archive output location.</param>
        /// <returns></returns>
        public SubmissionContainer CreateSubmissionContainer(List<PostmarkedSubmissionArchive> colPostMarkedArchives, string archiveOutputLocation)
        {
            MeFLog.LogDebug("Enter CreateSubmissionContainer");

            string ArchiveOutputLocation;
            if (archiveOutputLocation.EndsWith(@"\"))
                ArchiveOutputLocation = archiveOutputLocation;
            else
                ArchiveOutputLocation = archiveOutputLocation + @"\";
            if (!Directory.Exists(ArchiveOutputLocation))
            {
                Directory.CreateDirectory(ArchiveOutputLocation);
            }

            string ZipFilename = System.IO.Path.GetRandomFileName();
            ZipFilename = ZipFilename.Remove(ZipFilename.LastIndexOf("."), 4);
            string outerZipFile = ArchiveOutputLocation + ZipFilename + ".zip";
            FastZip oFastZip = new FastZip();
            //string innerZipsFolder = ArchiveOutputLocation + @"ComposedInnerZips\" + ZipFilename;
            string innerZipsFolder = ArchiveOutputLocation + ZipFilename;
            lock (syncObj) { Directory.CreateDirectory(innerZipsFolder); }
            foreach (PostmarkedSubmissionArchive pArch in colPostMarkedArchives)
            {
                string innerFilename = pArch.PostMarkedArchive.zipFileName.Substring(pArch.PostMarkedArchive.zipFileName.LastIndexOf("\\") + 1);
                File.Copy(pArch.PostMarkedArchive.zipFileName, innerZipsFolder + @"\" + innerFilename, true);
            }
            MeFLog.LogDebug("Creating Outerzip");
            oFastZip.CreateZip(outerZipFile, innerZipsFolder, true, null);
            // ZipHelper.Zip(innerZipsFolder, outerZipFile, 4096);
            //ZipHelper.CreateZipFile(ArchiveOutputLocation, ZipFilename, outerZipFile);
            MeFLog.LogDebug("Done Creating Outerzip");

            Directory.Delete(innerZipsFolder, true);
            //deleteFolder(ArchiveOutputLocation + @"ComposedInnerZips\", true);            
            SubmissionContainer subCont = new SubmissionContainer(colPostMarkedArchives, ArchiveOutputLocation);
            SetZipProperties(outerZipFile);
            subCont.ZipFilename = outerZipFile;
            return subCont;
        }

        /// <summary>
        /// Creates the submission container.
        /// </summary>
        /// <param name="colPostMarkedArchives">The col post marked archives.</param>        
        /// <returns></returns>
        public SubmissionContainer CreateSubmissionContainer(List<PostmarkedSubmissionArchive> colPostMarkedArchives)
        {
            MeFLog.LogDebug("Enter CreateSubmissionContainer");
            
            SubmissionContainer subCont = new SubmissionContainer(colPostMarkedArchives);
            
            return subCont;
        }

        /// <summary>
        /// Creates the submission container.
        /// </summary>
        /// <param name="submissionArchives">The collection of SubmissionArchives .</param>
        /// <param name="archiveOutputLocation">The archive output location.</param>
        /// <returns></returns>
        public SubmissionContainer CreateSubmissionContainer(List<SubmissionArchive> submissionArchives, string archiveOutputLocation)
        {
            MeFLog.LogDebug("Enter CreateSubmissionContainer");

            string ArchiveOutputLocation;
            if (archiveOutputLocation.EndsWith(@"\"))
                ArchiveOutputLocation = archiveOutputLocation;
            else
                ArchiveOutputLocation = archiveOutputLocation + @"\";
            if (!Directory.Exists(ArchiveOutputLocation))
            {
                //String msg = ErrorMessages.MeFClientSDK000027.getErrorMessage() +
                String msg = "Folder does not exists." +
                "; SubmissionBuilder; " + ArchiveOutputLocation;
                MeFLog.LogError("MeFClientSDK000004", msg, new IOException());
                throw new ToolkitException(msg);
            }

            string ZipFilename = System.IO.Path.GetRandomFileName();
            ZipFilename = ZipFilename.Remove(ZipFilename.LastIndexOf("."), 4);
            string outerZipFile = ArchiveOutputLocation + ZipFilename + ".zip";
            FastZip oFastZip = new FastZip();
            //string innerZipsFolder = ArchiveOutputLocation + @"ComposedInnerZips\" + ZipFilename;
            string innerZipsFolder = ArchiveOutputLocation + ZipFilename;
            lock (syncObj) { Directory.CreateDirectory(innerZipsFolder); }
            foreach (SubmissionArchive pArch in submissionArchives)
            {
                string innerFilename = pArch.zipFileName.Substring(pArch.zipFileName.LastIndexOf("\\") + 1);
                File.Copy(pArch.zipFileName, innerZipsFolder + @"\" + innerFilename, true);
            }
            MeFLog.LogDebug("Creating Outerzip");
            oFastZip.CreateZip(outerZipFile, innerZipsFolder, true, null);

            // ZipHelper.Zip(innerZipsFolder, outerZipFile, 4096);
            //ZipHelper.CreateZipFile(ArchiveOutputLocation, ZipFilename, outerZipFile);
            MeFLog.LogDebug("Done Creating Outerzip");

            Directory.Delete(innerZipsFolder, true);
            //deleteFolder (ArchiveOutputLocation + @"ComposedInnerZips\", true);
            SubmissionContainer subCont = new SubmissionContainer(submissionArchives, ArchiveOutputLocation);
            SetZipProperties(outerZipFile);
            subCont.ZipFilename = outerZipFile;
            return subCont;
        }

        private string CreateStateInnerZip(StateSubmissionArchive arch)
        {
            MeFLog.LogDebug("CreateStateInnerZip");
            string ArchiveOutputLocation;
            if (arch.archiveOutputLocation.EndsWith(@"\"))
                ArchiveOutputLocation = arch.archiveOutputLocation;
            else
                ArchiveOutputLocation = arch.archiveOutputLocation + @"\";

            //make the innerzip of Arch
            FastZip oFastZip = new FastZip();
            string innerZipsFolder;
            string innerZipFilename;
            string ZipFilename;
            try
            {
                ZipFilename = System.IO.Path.GetRandomFileName();
                ZipFilename = ZipFilename.Remove(ZipFilename.LastIndexOf("."), 4);
                innerZipsFolder = ArchiveOutputLocation;
                //string submissionFilesLocation = ArchiveOutputLocation + "\\SubmissionFiles\\" + ZipFilename + @"\" + arch.submissionId;
                string submissionFilesLocation = ArchiveOutputLocation + ZipFilename + @"\" + arch.submissionId;

                innerZipFilename = innerZipsFolder + arch.submissionId + ".zip";

                //manifest file
                MeFLog.LogDebug("Creating Manifest");
                string manifestFilename = arch.submissionManifest.getFileName().Substring(arch.submissionManifest.getFileName().LastIndexOf("\\") + 1);
                string newManifestFolder = submissionFilesLocation + "\\manifest";
                Directory.CreateDirectory(newManifestFolder);
                string newManifestFile = submissionFilesLocation + "\\manifest\\" + manifestFilename.ToLower();

                File.Copy(arch.submissionManifest.getFileName(), newManifestFile, true);
                if (manifestFilename != "manifest.xml")
                {
                    File.Move(newManifestFile, submissionFilesLocation + "\\manifest\\" + "manifest.xml");
                }
                arch.submissionManifest.setFileName(submissionFilesLocation + "\\manifest\\" + "manifest.xml");

                //xml file
                MeFLog.LogDebug("Creating xml");
                string xmlFilename = arch.submissionXml.getFileName().Substring(arch.submissionXml.getFileName().LastIndexOf("\\") + 1);
                string newXMLFile = submissionFilesLocation + "\\xml\\" + xmlFilename.ToLower();
                string newXMLFolder = submissionFilesLocation + "\\xml";
                Directory.CreateDirectory(newXMLFolder);
                File.Copy(arch.submissionXml.getFileName(), newXMLFile, true);
                arch.submissionXml.setFileName(newXMLFile);

                //binary files
                if (!(arch.binaryAttachments == null))
                {
                    MeFLog.LogDebug("Creating Binary");

                    foreach (BinaryAttachment item in arch.binaryAttachments)
                    {
                        string bFilename = item.FileName.Substring(item.FileName.LastIndexOf("\\") + 1);
                        string newBFile = submissionFilesLocation + "\\attachment\\" + bFilename.ToLower();
                        string newBFolder = submissionFilesLocation + "\\attachment";
                        Directory.CreateDirectory(newBFolder);

                        File.Copy(item.FileName, newBFile, true);
                        item.FileName = newBFile;
                    }
                }
                if (!(arch.IrsSubmissionXML == null))
                {
                    MeFLog.LogDebug("Creating IRS XML");

                    string stateXmlFilename = arch.IrsSubmissionXML.getFileName().Substring(arch.IrsSubmissionXML.getFileName().LastIndexOf("\\") + 1);
                    string newStateXMLFile = submissionFilesLocation + "\\irs\\xml\\" + stateXmlFilename.ToLower();
                    string newStateXMLFolder = submissionFilesLocation + "\\irs\\xml";
                    Directory.CreateDirectory(newStateXMLFolder);

                    File.Copy(arch.IrsSubmissionXML.getFileName(), newStateXMLFile, true);
                    arch.IrsSubmissionXML.setFileName(newStateXMLFile);
                    if (!(arch.IrsBinaryAttachments == null))
                    {
                        MeFLog.LogDebug("Creating IRS Binary");

                        foreach (BinaryAttachment item in arch.IrsBinaryAttachments)
                        {
                            string bStateFilename = item.FileName.Substring(item.FileName.LastIndexOf("\\") + 1);
                            string newStateBFile = submissionFilesLocation + "\\irs\\attachment\\" + bStateFilename.ToLower();
                            string newStateBFolder = submissionFilesLocation + "\\irs\\attachment";
                            Directory.CreateDirectory(newStateBFolder);

                            File.Copy(item.FileName, newStateBFile, true);
                            item.FileName = newStateBFile;
                        }
                    }
                }
                MeFLog.LogDebug("Creating Innerzip");

                oFastZip.CreateZip(innerZipFilename, submissionFilesLocation, true, null);
                MeFLog.LogDebug("Done Creating Innerzip");

                //Directory.Delete(ArchiveOutputLocation + @"\SubmissionFiles\", true);
                deleteFolder(ArchiveOutputLocation + ZipFilename, true, false);
                SetZipProperties(innerZipFilename);
                return innerZipFilename;
            }
            catch (System.IO.IOException ex)
            {
                MeFLog.LogError("MeFClientSDK000028", "Unable to create zip archive", ex);
                throw new ToolkitException("MeFClientSDK000028" + "Unable to create zip archive");
            }
            catch (Exception ex)
            {
                MeFLog.LogError("MeFClientSDK000029", "Unexpected Error", ex);
                throw new ToolkitException("MeFClientSDK000029", "Unexpected Error", ex);
            }
        }

        private string CreateIRSInnerZip(IRSSubmissionArchive arch)
        {
            MeFLog.LogDebug("CreateIRSInnerZip");

            string ArchiveOutputLocation;
            if (arch.archiveOutputLocation.EndsWith(@"\"))
                ArchiveOutputLocation = arch.archiveOutputLocation;
            else
                ArchiveOutputLocation = arch.archiveOutputLocation + @"\";

            //make the innerzip of Arch
            FastZip oFastZip = new FastZip();
            string innerZipsFolder;
            string innerZipFilename;
            string ZipFilename;
            try
            {
                ZipFilename = System.IO.Path.GetRandomFileName();
                ZipFilename = ZipFilename.Remove(ZipFilename.LastIndexOf("."), 4);
                innerZipsFolder = ArchiveOutputLocation;
                //string submissionFilesLocation = ArchiveOutputLocation + @"\SubmissionFiles\" + ZipFilename + @"\" + arch.submissionId;
                string submissionFilesLocation = ArchiveOutputLocation + ZipFilename + @"\" + arch.submissionId;
                innerZipFilename = innerZipsFolder + arch.submissionId + ".zip";

                //manifest file
                MeFLog.LogDebug("Creating manifest");

                string manifestFilename = arch.submissionManifest.getFileName().Substring(arch.submissionManifest.getFileName().LastIndexOf("\\") + 1);

                string newManifestFolder = submissionFilesLocation + "\\manifest";
                Directory.CreateDirectory(newManifestFolder);
                string newManifestFile = submissionFilesLocation + "\\manifest\\" + manifestFilename.ToLower();

                File.Copy(arch.submissionManifest.getFileName(), newManifestFile, true);
                if (manifestFilename != "manifest.xml")
                {
                    File.Move(newManifestFile, submissionFilesLocation + "\\manifest\\" + "manifest.xml");
                }
                arch.submissionManifest.setFileName(submissionFilesLocation + "\\manifest\\" + "manifest.xml");

                //xml file
                MeFLog.LogDebug("Creating xml");

                string xmlFilename = arch.submissionXml.getFileName().Substring(arch.submissionXml.getFileName().LastIndexOf("\\") + 1);
                string newXMLFile = submissionFilesLocation + "\\xml\\" + xmlFilename.ToLower();
                string newXMLFolder = submissionFilesLocation + "\\xml";
                Directory.CreateDirectory(newXMLFolder);
                File.Copy(arch.submissionXml.getFileName(), newXMLFile, true);
                arch.submissionXml.setFileName(newXMLFile);

                //binary files
                if (!(arch.binaryAttachments == null))
                {
                    MeFLog.LogDebug("Creating Binary");

                    foreach (BinaryAttachment item in arch.binaryAttachments)
                    {
                        string bFilename = item.FileName.Substring(item.FileName.LastIndexOf("\\") + 1);
                        string newBFile = submissionFilesLocation + "\\attachment\\" + bFilename;
                        string newBFolder = submissionFilesLocation + "\\attachment";
                        Directory.CreateDirectory(newBFolder);

                        File.Copy(item.FileName, newBFile, true);
                        item.FileName = newBFile;
                    }
                }
                MeFLog.LogDebug("Creating innerzip");
                //DirectoryInfo zipDir = new DirectoryInfo(submissionFilesLocation);
                oFastZip.CreateZip(innerZipFilename, submissionFilesLocation, true, null);
                // ZipHelper.CreateZipFile(ArchiveOutputLocation, innerZipFilename, zipDir);
                MeFLog.LogDebug("Done Creating innerzip");

                //Directory.Delete(ArchiveOutputLocation + @"\SubmissionFiles\", true);
                deleteFolder (ArchiveOutputLocation + ZipFilename, true, false);
                SetZipProperties(innerZipFilename);
                return innerZipFilename;
            }
            catch (System.IO.IOException ex)
            {
                MeFLog.LogError("MeFClientSDK000028", "Unable to create zip archive", ex);
                throw new ToolkitException("MeFClientSDK000028", "Unable to create zip archive", ex);
            }
            catch (Exception ex)
            {
                MeFLog.LogError("MeFClientSDK000029", "Unexpected Error", ex);
                throw new ToolkitException("MeFClientSDK000029", "Unexpected Error", ex);
            }
        }

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

        /// <summary>
        /// Creates the submission container.
        /// </summary>
        /// <param name="postMarkedArchives">The post marked archives.</param>
        /// <param name="submissionArchives">The submission archives.</param>
        /// <param name="currentSaveDirectory">The current save directory.</param>
        /// <returns></returns>
        public SubmissionContainer CreateSubmissionContainer(List<PostmarkedSubmissionArchive> postMarkedArchives, List<SubmissionArchive> submissionArchives, string currentSaveDirectory)
        {
            MeFLog.LogDebug("Enter CreateSubmissionContainer");

            string ArchiveOutputLocation;
            if (currentSaveDirectory.EndsWith(@"\"))
                ArchiveOutputLocation = currentSaveDirectory;
            else
                ArchiveOutputLocation = currentSaveDirectory + @"\";
            if (!Directory.Exists(ArchiveOutputLocation))
            {
                Directory.CreateDirectory(ArchiveOutputLocation);
            }

            string ZipFilename = System.IO.Path.GetRandomFileName();
            ZipFilename = ZipFilename.Remove(ZipFilename.LastIndexOf("."), 4);
            string outerZipFile = ArchiveOutputLocation + ZipFilename + ".zip";
            FastZip oFastZip = new FastZip();
            //string innerZipsFolder = ArchiveOutputLocation + @"ComposedInnerZips\" + ZipFilename;
            string innerZipsFolder = ArchiveOutputLocation + ZipFilename;
            lock (syncObj) { Directory.CreateDirectory(innerZipsFolder); }
            MeFLog.LogInfo("Adding PostmarkedSubmissionArchives");
            foreach (PostmarkedSubmissionArchive pArch in postMarkedArchives)
            {
                string innerFilename = pArch.PostMarkedArchive.zipFileName.Substring(pArch.PostMarkedArchive.zipFileName.LastIndexOf("\\") + 1);
                File.Copy(pArch.PostMarkedArchive.zipFileName, innerZipsFolder + @"\" + innerFilename, true);
            }
            MeFLog.LogInfo("Adding SubmissionArchives");
            foreach (SubmissionArchive pArch in submissionArchives)
            {
                string innerFilename = pArch.zipFileName.Substring(pArch.zipFileName.LastIndexOf("\\") + 1);
                File.Copy(pArch.zipFileName, innerZipsFolder + @"\" + innerFilename, true);
            }
            MeFLog.LogDebug("Creating Outerzip");
            oFastZip.CreateZip(outerZipFile, innerZipsFolder, true, null);

            MeFLog.LogDebug("Done Creating Outerzip");

            Directory.Delete(innerZipsFolder, true);
            //deleteFolder(ArchiveOutputLocation + @"ComposedInnerZips\", true);
            SubmissionContainer subCont = new SubmissionContainer(postMarkedArchives, submissionArchives, ArchiveOutputLocation);
            SetZipProperties(outerZipFile);
            subCont.ZipFilename = outerZipFile;
            return subCont;
        }

        
        //delete folder to handle multi-thread case
        private static Object syncObj = new Object ();        
        private static void deleteFolder(string folderName, bool deleteSubDirectory, bool checkEmpty=true)
        {
            lock (syncObj)
            {
                try
                {
                    if (checkEmpty)
                    {
                        string[] dirs = Directory.GetDirectories(folderName);
                        string[] files = Directory.GetFiles(folderName);
                        if (dirs.Length == 0 && files.Length == 0)
                        {
                            Directory.Delete(folderName, deleteSubDirectory);
                        }
                    }
                    else
                    {
                        Directory.Delete(folderName, deleteSubDirectory);
                    }
                }
                catch (Exception e)
                {
                    MeFLog.LogDebug(e.ToString());
                }
            }
        }
        

    }
}