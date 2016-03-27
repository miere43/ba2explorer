using Ba2Tools;
using GalaSoft.MvvmLight;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Ba2Explorer.ViewModel
{
    public class ArchiveInfo : ObservableObject
    {
        private ObservableCollection<string> files;
        public ObservableCollection<string> Files
        {
            get { return files; }
            set
            {
                files = value;
                RaisePropertyChanged();
            }
        }

        private bool isOpened = false;
        public bool IsOpened
        {
            get { return isOpened; }
            private set
            {
                isOpened = true;
                RaisePropertyChanged();
            }
        }

        private bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            private set
            {
                isBusy = value;
                RaisePropertyChanged();
            }
        }

        public string FileName { get; private set; }

        public string FilePath { get; private set; }

        public int TotalFiles
        {
            get { return (int)archive.TotalFiles; }
        }

        private BA2Archive archive;

        public ArchiveInfo()
        {

        }

        public void ExtractSingle(string fileInArchive)
        {
            if (!archive.ContainsFile(fileInArchive))
            {
                MessageBox.Show("this file doesnt exist");
                return;
            }

            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.CheckPathExists = true;
                dialog.OverwritePrompt = true;
                dialog.ValidateNames = true;
                dialog.Title = "Extract file as...";
                dialog.FileName = Path.GetFileName(fileInArchive);

                string ext = Path.GetExtension(dialog.SafeFileName);

                dialog.Filter = "Specified file|*" + ext + "|All files|*.*";
                dialog.ShowDialog();

                using (FileStream stream = File.Create(dialog.FileName))
                {
                    archive.ExtractToStream(fileInArchive, stream);
                }

                MessageBox.Show("OK!");
            }
            catch (Exception e)
            {
                // todo;
                MessageBox.Show(e.Message);
            }
        }

        public void ExtractFiles(
            IEnumerable<string> files,
            string destination,
            CancellationToken cancellationToken,
            IProgress<int> progress)
        {
            Contract.Ensures(IsBusy == false);

            try
            {
                IsBusy = true;
                archive.ExtractFiles(files, destination, cancellationToken, progress, true);
            }
            catch (OperationCanceledException e)
            {
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ExtractFiles(IEnumerable<string> files)
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Save files to folder";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    archive.ExtractFiles(files, dialog.SelectedPath, true);
                }
            }
            catch (Exception e)
            {
                // todo;
                MessageBox.Show(e.Message);
            }
        }

        public void Open(string path)
        {
            if (archive != null && IsOpened)
            {
                if (IsBusy)
                    throw new ArgumentException();

                archive.Dispose();
                archive = null;
                IsOpened = false;
                Files = null;
                FilePath = null;
                FileName = null;
            }

            try
            {
                archive = BA2Loader.Load(path);
                Files = new ObservableCollection<string>(archive.ListFiles());
                FilePath = path;
                FileName = Path.GetFileName(FilePath);
                IsOpened = true;
            }
            catch (Exception e)
            {
                // todo
                MessageBox.Show(e.Message);
            }
        }
    }
}
