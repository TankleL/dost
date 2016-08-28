#define TEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.IO;
using System.Windows.Xps.Packaging;
using Microsoft.Win32;
using Word = Microsoft.Office.Interop.Word;

namespace dost
{
    static class DocOpt
    {
#if TEST
        private static string m_tempPoolPath = "../../../../temp/";
#else
        private static string m_tempPoolPath = "./temp/";
#endif

        public enum OfficeDocuFormat : int
        {
            doc = 1,
            docx,
            xls,
            xlsx,
            ppt,
            pptx
        };

        public static void ClearSpace()
        {
            string fullpath = System.IO.Path.GetFullPath(m_tempPoolPath);

            if (Directory.Exists(fullpath)==false)
            {
                throw new Exception("wrong path.");
            }

            DirectoryInfo dir = new DirectoryInfo(fullpath);
            FileInfo[] files = dir.GetFiles();
            try
            {
                foreach (var item in files)
                {
                    File.Delete(item.FullName);
                }
            }
            catch
            {
                throw new Exception("cannot delete files.");
            }
        }

        /// <summary> 
        ///  Convert the word document to xps document 
        /// </summary> 
        /// <param name="wordFilename">Word document Path</param> 
        /// <param name="xpsFilename">Xps document Path</param> 
        /// <returns></returns> 
        public static XpsDocument ConvertWordToXps(string wordFilename, string xpsFilename)
        {
            // Create a WordApplication and host word document 
            Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
            try
            {
                wordApp.Documents.Open(wordFilename);

                // To Invisible the word document 
                wordApp.Application.Visible = false;

                // Minimize the opened word document 
                wordApp.WindowState = Word.WdWindowState.wdWindowStateMinimize;

                Word.Document doc = wordApp.ActiveDocument;
                doc.SaveAs(xpsFilename, Word.WdSaveFormat.wdFormatXPS);

                XpsDocument xpsDocument = new XpsDocument(xpsFilename, FileAccess.Read);
                return xpsDocument;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurs, The error message is  " + ex.ToString());
                return null;
            }
            finally
            {
                wordApp.Documents.Close();
                ((Word._Application)wordApp).Quit(Word.WdSaveOptions.wdDoNotSaveChanges);
            }
        }

        /// <summary>
        ///  Show a Word document in DocumentViewer control.
        /// </summary>
        /// <param name="viewer">DocumentViewer</param>
        /// <param name="wordDocPath">Word document Path</param>
        public static void DisplayWordDocument(DocumentViewer viewer, string wordDocPath)
        {
            if (string.IsNullOrEmpty(wordDocPath) || !File.Exists(wordDocPath))
            {
                MessageBox.Show("The file is invalid. Please select an existing file again.");
                return;
            }

            string convertedXpsDoc = string.Concat(m_tempPoolPath, Guid.NewGuid().ToString(), ".xps");
            convertedXpsDoc = System.IO.Path.GetFullPath(convertedXpsDoc);
            XpsDocument xpsDocument = ConvertWordToXps(wordDocPath, convertedXpsDoc);
            if (xpsDocument == null)
            {
                return;
            }

            viewer.Document = xpsDocument.GetFixedDocumentSequence();
        }
        

        /// <summary>
        ///  Show a Word document in DocumentViewer control.
        /// </summary>
        /// <param name="viewer">DocumentViewer</param>
        /// <param name="wordDoc">Word document data</param>
        /// <param name="format">Document format</param>
        public static void DisplayWordDocument(DocumentViewer viewer, byte[] wordDoc, OfficeDocuFormat format)
        {
            if (wordDoc == null)
            {
                return;
            }
            
            string wordDocxPath = String.Empty;

            if (format == OfficeDocuFormat.docx)
                wordDocxPath = string.Concat(m_tempPoolPath, Guid.NewGuid().ToString(), ".docx");
            else if (format == OfficeDocuFormat.doc)
                wordDocxPath = string.Concat(m_tempPoolPath, Guid.NewGuid().ToString(), ".doc");
            else
            {
                throw new Exception("wrong format.");
            }

            wordDocxPath = System.IO.Path.GetFullPath(wordDocxPath);

            try
            {
                using (FileStream fs = new FileStream(wordDocxPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(wordDoc);
                    bw.Close();
                }

                DisplayWordDocument(viewer, wordDocxPath);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Compare two word documnet
        /// </summary>
        /// <param name="viewer"></param>
        /// <param name="wordDocDataRef"></param>
        /// <param name="RefFormat"></param>
        /// <param name="wordDocDataComp"></param>
        /// <param name="CompFormat"></param>
        public static void CompareWordDocument(DocumentViewer viewer, byte[] wordDocDataRef, OfficeDocuFormat RefFormat, byte[] wordDocDataComp, OfficeDocuFormat CompFormat)
        {
            string resDocPath = String.Empty;
            string pathRef = String.Empty;
            string pathComp = String.Empty;
            string pathRes = String.Empty;
            string pathXps = String.Empty;

            if (RefFormat == OfficeDocuFormat.docx)
                pathRef = string.Concat(m_tempPoolPath, Guid.NewGuid().ToString(), ".docx");
            else if (RefFormat == OfficeDocuFormat.doc)
                pathRef = string.Concat(m_tempPoolPath, Guid.NewGuid().ToString(), ".doc");
            else
            {
                throw new Exception("wrong format.");
            }

            if (CompFormat == OfficeDocuFormat.docx)
                pathComp = string.Concat(m_tempPoolPath, Guid.NewGuid().ToString(), ".docx");
            else if (CompFormat == OfficeDocuFormat.doc)
                pathComp = string.Concat(m_tempPoolPath, Guid.NewGuid().ToString(), ".doc");
            else
            {
                throw new Exception("wrong format.");
            }

            pathXps = string.Concat(m_tempPoolPath, Guid.NewGuid().ToString(), ".xps");
            pathRes = string.Concat(m_tempPoolPath, Guid.NewGuid().ToString(), ".docx");

            pathRef = System.IO.Path.GetFullPath(pathRef);
            pathComp = System.IO.Path.GetFullPath(pathComp);
            pathXps = System.IO.Path.GetFullPath(pathXps);
            pathRes = System.IO.Path.GetFullPath(pathRes);

            try
            {
                using (FileStream fs = new FileStream(pathRef, FileMode.Create, FileAccess.ReadWrite))
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(wordDocDataRef);
                    bw.Close();
                }

                using (FileStream fs = new FileStream(pathComp, FileMode.Create, FileAccess.ReadWrite))
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(wordDocDataComp);
                    bw.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // Create a WordApplication and host word document 
            Word.Application wordApp = new Microsoft.Office.Interop.Word.Application() { Visible = false };

            try
            {
                wordApp.Documents.Open(pathRef);

                // To Invisible the word document 
                wordApp.Application.Visible = false;

                // Minimize the opened word document 
                wordApp.WindowState = Word.WdWindowState.wdWindowStateMinimize;

                Word.Document doc = wordApp.ActiveDocument;
                doc.Compare(pathComp);
                doc.Close(Word.WdSaveOptions.wdDoNotSaveChanges);
                doc = wordApp.ActiveDocument;
                doc.SaveAs(pathRes, Word.WdSaveFormat.wdFormatXMLDocument);
                doc.Close(Word.WdSaveOptions.wdDoNotSaveChanges);

                wordApp.Documents.Open(pathRes);
                doc = wordApp.ActiveDocument;
                doc.SaveAs(pathXps, Word.WdSaveFormat.wdFormatXPS);

                XpsDocument xpsDocument = new XpsDocument(pathXps, FileAccess.Read);
                viewer.Document = xpsDocument.GetFixedDocumentSequence();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                wordApp.Documents.Close(Word.WdSaveOptions.wdDoNotSaveChanges);
                ((Word._Application)wordApp).Quit(Word.WdSaveOptions.wdDoNotSaveChanges);
            }
        }
    }
}
