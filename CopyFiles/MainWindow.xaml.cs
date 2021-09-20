using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CopyFiles
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string baseFolder;
        private ObservableCollection<FileItem> fileItems = new ObservableCollection<FileItem>();

        private void FileListBox_Drop(object sender, DragEventArgs e)
        {
            var items = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (items.Length < 1)
            {
                return;
            }
            if (string.IsNullOrEmpty(baseFolder))
            {
                if (items.Length > 1 || File.Exists(items[0]))
                {
                    SetBase(Path.GetDirectoryName(items[0]));
                }
                else
                {
                    SetBase(items[0]);
                    return;
                }
            }
            AddFiles(items);
            FileListBox.ItemsSource = fileItems;
        }

        private void AddFiles(string[] items)
        {
            foreach (var item in items)
            {
                if (!item.StartsWith(baseFolder))
                {
                    continue;
                }
                var name = item.Substring(baseFolder.Length + 1);
                var fileInfo = new FileInfo(item);
                fileItems.Add(new FileItem()
                {
                    FileName = item,
                    IsFile = (fileInfo.Attributes & FileAttributes.Directory) == 0,
                    Name = name
                });
            }
        }

        private void FileListBox_DragEnter(object sender, DragEventArgs e)
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

        private void saveAsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (fileItems.Count < 1)
            {
                MessageBox.Show("请先添加文件");
                return;
            }
            var folder = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = AppDomain.CurrentDomain.BaseDirectory,
                ShowNewFolderButton = false
            };
            if (folder.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            var path = folder.SelectedPath;
            if (Directory.GetFiles(path).Length > 0 && MessageBox.Show("当前文件夹不为空，是否继续？") != MessageBoxResult.OK)
            {
                return;
            }
            foreach (var item in fileItems)
            {
                var copyFile = Path.Combine(path, item.Name);
                if (item.IsFile)
                {
                    CreateDirectory(copyFile);
                    File.Copy(item.FileName, copyFile);
                } else
                {
                    CopyDirectory(item.FileName, copyFile);
                }
            }
            MessageTb.Text = "复制成功";
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            SetBase(string.Empty);
        }

        public static void CreateDirectory(string filefullpath)
        {
            if (File.Exists(filefullpath))
            {
                return;
            }
            //判断路径中的文件夹是否存在
            var dirpath = filefullpath.Substring(0, filefullpath.LastIndexOf('\\'));
            var pathes = dirpath.Split('\\');
            if (pathes.Length <= 1) return;
            var path = pathes[0];
            for (var i = 1; i < pathes.Length; i++)
            {
                path += "\\" + pathes[i];
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        public static void CopyDirectory(string srcPath, string destPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath); FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)     //判断是否文件夹
                    {
                        if (!Directory.Exists(destPath + "\\" + i.Name))
                        {
                            Directory.CreateDirectory(destPath + "\\" + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
                        }
                        CopyDirectory(i.FullName, destPath + "\\" + i.Name);    //递归调用复制子文件夹
                    }
                    else
                    {
                        File.Copy(i.FullName, destPath + "\\" + i.Name, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void BaseBtn_Click(object sender, RoutedEventArgs e)
        {
            var folder = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = AppDomain.CurrentDomain.BaseDirectory,
                ShowNewFolderButton = false
            };
            if (folder.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            SetBase(folder.SelectedPath);
        }

        private void SetBase(string path)
        {
            MessageTb.Text = "";
            baseFolder = path;
            fileItems.Clear();
            FileListBox.ItemsSource = fileItems;
            if (string.IsNullOrWhiteSpace(path))
            {
                BaseBtn.Visibility = Visibility.Visible;
                BaseTb.Visibility = Visibility.Collapsed;
                return;
            }
            BaseBtn.Visibility = Visibility.Collapsed;
            BaseTb.Visibility = Visibility.Visible;
            BaseTb.Text = path;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectIndexItems = new List<int>();
            foreach (FileItem item in FileListBox.SelectedItems)
            {
                for (int i = fileItems.Count - 1; i >= 0; i--)
                {
                    if (fileItems[i] == item)
                    {
                        selectIndexItems.Add(i);
                    }
                }
            }
            selectIndexItems.Sort((x, y) => y.CompareTo(x));
            switch ((sender as MenuItem).Header)
            {
                case "清空":
                    fileItems.Clear();
                    FileListBox.ItemsSource = fileItems;
                    break;
                case "取消选中":
                    FileListBox.UnselectAll();
                    break;
                case "展开文件夹":
                    foreach (var i in selectIndexItems)
                    {
                        var item = fileItems[i];
                        if (item.IsFile)
                        {
                            continue;
                        }
                        fileItems.RemoveAt(i);
                        var files = Directory.GetFiles(item.FileName, "*", SearchOption.AllDirectories);
                        AddFiles(files);
                    }
                    FileListBox.ItemsSource = fileItems;
                    break;
                case "删除选中":
                    foreach (var i in selectIndexItems)
                    {
                        fileItems.RemoveAt(i);
                    }
                    FileListBox.ItemsSource = fileItems;
                    break;
            }
        }

        /// <summary>
        /// 更新根目录导致相对路径的变化
        /// </summary>
        private void RefreshBase()
        {
            for (int i = fileItems.Count - 1; i >= 0; i--)
            {
                var item = fileItems[i];
                if (!item.FileName.StartsWith(baseFolder))
                {
                    fileItems.RemoveAt(i);
                    continue;
                }
                item.Name = item.FileName.Substring(baseFolder.Length + 1);
            }
        }
    }

    class FileItem
    {
        public string Name { get; set; }

        public bool IsFile { get; set; } = true;

        public string FileName { get; set; }
    }
}
