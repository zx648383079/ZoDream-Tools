using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZoDream.CopyFiles.ViewModels;

namespace ZoDream.CopyFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainViewModel ViewModel => (MainViewModel)DataContext;

        private void FileBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void FileBox_Drop(object sender, DragEventArgs e)
        {
            var items = (IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop);
            if (items is null)
            {
                return;
            }
            ViewModel.Add(items);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            FileBox.UnselectAll();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DeleteCommand.Execute(FileBox.SelectedItems);
        }

        private void ExpandBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ExpandCommand.Execute(FileBox.SelectedItems);
        }
    }
}
