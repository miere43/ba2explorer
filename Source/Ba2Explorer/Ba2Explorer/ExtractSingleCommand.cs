//using Ba2Explorer.ViewModel;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Input;

//namespace Ba2Explorer
//{
//    public class ExtractSingleCommand : ICommand
//    {
//        public event EventHandler CanExecuteChanged;

//        private MainWindow view;

//        public ExtractSingleCommand(MainWindow view)
//        {
//            this.view = view;
//        }

//        public bool CanExecute(object parameter)
//        {
//            if (!view.MainViewModel.ArchiveInfo.IsOpened)
//                return false;

//            return view.ArchiveFilesList.SelectedIndex != -1;
//        }

//        public void Execute(object parameter)
//        {
//            string sel = view.ArchiveFilesList.SelectedItem as string;

//            view.MainViewModel.ArchiveInfo.ExtractSingle(sel, "D:/");
//        }
//    }
//}
