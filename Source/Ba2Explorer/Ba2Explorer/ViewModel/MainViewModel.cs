using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.IO;
using System;
using System.Diagnostics.Contracts;
using Microsoft.Win32;

namespace Ba2Explorer.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private ArchiveInfo archiveInfo;
        public ArchiveInfo ArchiveInfo
        {
            get
            {
                return archiveInfo;
            }
            private set
            {
                archiveInfo = value;
                RaisePropertyChanged();
            }
        }

        public MainWindow Window { get; set; }

        //private bool CharCmp(char c)
        //{
        //    return c == '\\';
        //}

        //public ObservableCollection<ArchiveFilename> CreateFromStrings(List<string> strings, int rootLevel = 0)
        //{
        //    strings.Sort((lhs, rhs) =>
        //    {
        //        int lhsDirs = lhs.Count(CharCmp);
        //        int rhsDirs = rhs.Count(CharCmp);

        //        if (lhsDirs > rhsDirs)
        //            return rhsDirs - lhsDirs;
        //        else if (rhsDirs < lhsDirs)
        //            return lhsDirs - rhsDirs;
        //        return 0;
        //    });

        //    ObservableCollection<ArchiveFilename> names = new ObservableCollection<ArchiveFilename>();

        //    foreach (string item in strings)
        //    {
        //        string[] subdirs = item.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
        //        if (subdirs.Length == 1)
        //        {
        //            names.Add(new ArchiveFilename(subdirs[0]));
        //            continue;
        //        }
        //    }

        //    return names;
        //}

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ArchiveInfo = null;

            //ArchiveList = new ObservableCollection<ArchiveFilename>(
            //    CreateFromStrings(new List<string>()
            //    {
            //        @"1",
            //        @"2\b",
            //        @"3\b\c",
            //    }));

            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }

        public void SetTitle(string title)
        {
            if (title == null)
                Window.Title = "BA2 Explorer";
            else
                Window.Title = "BA2 Explorer - " + title.Trim();
        }


        public void OpenArchiveWithDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = "Open archive";
            dialog.CheckPathExists = true;
            dialog.Filter = "BA2 Archives|*.ba2|All files|*.*";
            dialog.CheckFileExists = true;
            dialog.ShowDialog();

            OpenArchive(dialog.FileName);
        }

        /// <summary>
        /// Opens the archive from file path.
        /// </summary>
        public void OpenArchive(string path)
        {
            if (ArchiveInfo != null)
            {
                CloseArchive();
            }

            ArchiveInfo = ArchiveInfo.Open(path);
            SetTitle(ArchiveInfo.FileName);
        }

        private void ExtractFilesWithDialog(string destinationFolder, IEnumerable<string> files)
        {
            FileExtractionWindow window = new FileExtractionWindow();
            window.ViewModel.ArchiveInfo = this.ArchiveInfo;
            window.ViewModel.DestinationFolder = destinationFolder;
            window.ViewModel.FilesToExtract = files;
            window.ShowInTaskbar = true;
            window.Owner = this.Window;

            window.ShowDialog();
            window.Activate();
        }

        public void CloseArchive()
        {
            // TODO add checks
            ArchiveInfo.Dispose();
            ArchiveInfo = null;
        }

        public void ExtractFilesWithDialog(IEnumerable<string> files)
        {
            Contract.Ensures(ArchiveInfo.IsBusy == false);
            Contract.Assert(files != null);

            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Extract files to folder...";
                dialog.ShowNewFolderButton = true;

                var result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    ExtractFilesWithDialog(dialog.SelectedPath, files);
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
                // todo;
                MessageBox.Show(e.Message);
            }
        }
    }
}