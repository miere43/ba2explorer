using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Ba2Explorer.Utility;
using Ba2Explorer.View;
using Ba2Explorer.ViewModel;
using S16.Drawing;

namespace Ba2Explorer.Service
{
    public enum PreviewFileType
    {
        Unknown,
        Text,
        Wav,
        Dds
    }

    public class PreviewFileData
    {
        public PreviewFileType FileType;

        /// <summary>
        /// Data, which depends on FileType. `null` indicates error.
        /// </summary>
        public object Data;

        /// <summary>
        /// File index in archive.
        /// </summary>
        public int FileIndex;

        public string TakeTextArgs()
        {
            return (string)Data;
        }

        public DdsImage TakeDdsArgs()
        {
            return (DdsImage)Data;
        }

        public Stream TakeWavArgs()
        {
            return (Stream)Data;
        }
    }

    public static class FilePreviewService
    {
        private static EncodedStringConverter m_stringConv = new EncodedStringConverter();

        public static PreviewFileData LoadPreview(DoWorkEventArgs args)
        {
            var argsArray = (object[])args.Argument;
            var archive = (ArchiveInfo)argsArray[0];
            var fileIndex = (int)argsArray[1];
            var fileType = (PreviewFileType)argsArray[2];

            PreviewFileData result = new PreviewFileData()
            {
                FileType = fileType,
                FileIndex = fileIndex
            };

            if (fileType == PreviewFileType.Unknown)
                return result;

            var stream = new MemoryStream();
            try
            {
                archive.Archive.ExtractToStream(fileIndex, stream);
            }
            catch (ObjectDisposedException e)
            {
                App.Logger.LogException(Logging.LogPriority.Error, "FilePreviwer.LoadPreview", e);
                return null;
            }

            switch (fileType)
            {
                // before adding another one make sure you closed `stream` if requred.
                case PreviewFileType.Text:
                    result.Data = CreateTextPreview(stream);
                    break;
                case PreviewFileType.Dds:
                    result.Data = CreateDdsPreview(stream);
                    break;
                case PreviewFileType.Wav:
                    result.Data = CreateWavPreview(stream);
                    break;
            }

            return result;
        }

        private static Stream CreateWavPreview(Stream stream)
        {
            // yay!
            return stream;
        }

        private static DdsImage CreateDdsPreview(Stream stream)
        {
            Contract.Requires(stream != null);
            stream.Seek(0, SeekOrigin.Begin);

            DdsImage image = null;
            try
            {
                image = new DdsImage(stream);
            }
            catch (Exception)
            {
                return null;
            }

            if (!image.IsValid)
            {
                image.Dispose();
                return null;
            }

            stream.Dispose();
            return image;
        }

        private static string CreateTextPreview(Stream stream)
        {
            Contract.Requires(stream != null);

            byte[] buffer = new byte[stream.Length];
            int readed = stream.Read(buffer, 0, (int)stream.Length);
            //Contract.Requires(readed == stream.Length);

            stream.Dispose();
            return m_stringConv.GetConvertedString(buffer, Encoding.ASCII);
        }

        /// <summary>
        /// Resolves file name to FileType enum.
        /// </summary>
        public static PreviewFileType ResolveFileTypeFromFileName(string fileName)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(fileName));

            if (fileName.EndsWith("txt", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith("xml", StringComparison.OrdinalIgnoreCase))
            {
                return PreviewFileType.Text;
            }
            else if (fileName.EndsWith("wav", StringComparison.OrdinalIgnoreCase))
            {
                return PreviewFileType.Wav;
            }
            else if (fileName.EndsWith("dds", StringComparison.OrdinalIgnoreCase))
            {
                return PreviewFileType.Dds;
            }

            return PreviewFileType.Unknown;
        }
    }
}
