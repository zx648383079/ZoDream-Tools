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
using ZoDream.FindFile.Models;
using ZoDream.FindFile.Shared;
using ZoDream.FindFile.ViewModels;

namespace ZoDream.FindFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
            finder.FileChanged += Finder_FileChanged;
            finder.FoundChanged += Finder_FoundChanged;
            finder.Finished += Finder_Finished;
        }

        private void Finder_Finished()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ViewModel.ShowMessage("查找完成");
                ResetBtn.IsEnabled = StartBtn.IsEnabled = true;
            });
        }

        private void Finder_FoundChanged(Models.FileItem item)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ViewModel.FileItems.Add(new FilenNotifyItem(item));
            });
        }

        private void Finder_FileChanged(string fileName)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ViewModel.Message = fileName;
            });
            
        }

        public MainViewModel ViewModel = new();
        private Finder finder = new Finder();

        private void AddFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            var picker = new System.Windows.Forms.FolderBrowserDialog();
            if (picker.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            if (ViewModel.FolderItems.Contains(picker.SelectedPath))
            {
                return;
            }
            ViewModel.FolderItems.Add(picker.SelectedPath);
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.FolderItems.Count == 0)
            {
                MessageBox.Show("请选择文件夹");
                return;
            }
            ViewModel.FileItems.Clear();
            ResetBtn.IsEnabled = StartBtn.IsEnabled = false;
            finder.CompareFlags = CompareBox.CompareFlags;
            finder.FilterFlags = FilterBox.FilterFlags;
            finder.Start(ViewModel.FolderItems.ToArray());
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FileItems.Clear();
            ViewModel.FolderItems.Clear();
            finder.Stop();
        }

        private void CheckFileBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.FileItems.Count == 0)
            {
                MessageBox.Show("没有可用数据", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (ViewModel.FileItems.Any(x => x.IsChecked))
            {
                foreach (var item in ViewModel.FileItems)
                {
                    item.IsChecked = false;
                }
            }
            else
            {
                foreach (var item in ViewModel.FileItems)
                {
                    item.IsChecked = true;
                }
                var keyList = ViewModel.FileItems.GroupBy(x => x.Guid);
                foreach (var keyItem in keyList)
                {
                    ViewModel.FileItems.First(x => x.Guid == keyItem.Key).IsChecked = false;
                }
            }
        }

        private void DeleteFileBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.FileItems.Count == 0)
                {
                    MessageBox.Show("没有可用数据", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (MessageBox.Show($"确认要删除选中文件吗？文件删除后不可恢复！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                foreach (var file in ViewModel.FileItems)
                {
                    if (file.IsChecked == false)
                    {
                        continue;
                    }
                    System.IO.File.Delete(file.FileName);
                }
                ViewModel.FileItems.Clear();
                MessageBox.Show("删除完成", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败：{ex.Message}", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckBtn_Checked(object sender, RoutedEventArgs e)
        {
            var checkTb = sender as CheckBox;
            if (checkTb == null)
            {
                return;
            }
            var item = checkTb.DataContext as FilenNotifyItem;
            if (item == null)
            {
                return;
            }
            if (!ViewModel.FileItems.Any(x => x.Guid == item.Guid && !x.IsChecked))
            {
                MessageBox.Show("必须至少保留重复文件中的一个", "提示", MessageBoxButton.OK, MessageBoxImage.Stop);
                checkTb.IsChecked = false;
                return;
            }
        }

        private void RemoveFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button)?.Tag;
            if (tag == null)
            {
                return;
            }
            ViewModel.FolderItems.Remove(tag.ToString());
        }
    }
}
