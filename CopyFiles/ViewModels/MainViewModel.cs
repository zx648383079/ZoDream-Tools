using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using ZoDream.CopyFiles.Models;
using ZoDream.Shared.ViewModels;

namespace ZoDream.CopyFiles.ViewModels
{
    public class MainViewModel: BindableBase
    {

        public MainViewModel()
        {
            BaseCommand = new RelayCommand(TapBase);
            ResetCommand = new RelayCommand(TapReset);
            SaveAsCommand = new RelayCommand(TapSaveAs);
            ClearCommand = new RelayCommand(TapClear);
            DeleteCommand = new RelayCommand(TapDelete);
            ExpandCommand = new RelayCommand(TapExpand);
        }


        private string baseFolder = string.Empty;

        public string BaseFolder {
            get => baseFolder;
            set => Set(ref baseFolder, value);
        }

        private ObservableCollection<FileItem> fileItems = new();

        public ObservableCollection<FileItem> FileItems 
        {
            get => fileItems;
            set => Set(ref fileItems, value);
        }

        private string message = string.Empty;

        public string Message {
            get => message;
            set => Set(ref message, value);
        }

        public ICommand BaseCommand { get; private set; }
        public ICommand ResetCommand { get; private set; }
        public ICommand SaveAsCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand ExpandCommand { get; private set; }

        private void TapBase(object? _)
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
            BaseFolder = folder.SelectedPath;
            for (int i = FileItems.Count - 1; i >= 0; i--)
            {
                var item = FileItems[i];
                item.Name = GetRelativeFileName(item.FileName);
                if (string.IsNullOrWhiteSpace(item.Name))
                {
                    FileItems.RemoveAt(i);
                }
            }
        }

        private void TapReset(object? _)
        {
            BaseFolder = string.Empty;
            FileItems.Clear();
        }

        private void TapSaveAs(object? _)
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
                Message = $"正在复制：{item.Name}";
                if (item.IsFile)
                {
                    CreateDirectory(copyFile);
                    File.Copy(item.FileName, copyFile);
                }
                else
                {
                    CopyDirectory(item.FileName, copyFile);
                }
            }
            Message = "复制成功";
        }

        private void TapClear(object? _)
        {
            FileItems.Clear();
        }

        private void TapDelete(object? arg)
        {
            if (arg is IList<FileItem> items)
            {
                foreach (var item in items)
                {
                    FileItems.Remove(item);
                }
            }
        }

        private void TapExpand(object? arg)
        {
            if (arg is IList<FileItem> items)
            {
                var folderItems = new List<string>();
                foreach (var item in items)
                {
                    FileItems.Remove(item);
                    if (item.IsFile)
                    {
                        continue;
                    }
                    folderItems.Add(item.FileName);
                }
                foreach(var item in folderItems)
                {
                    var files = Directory.GetFiles(item, "*", SearchOption.AllDirectories);
                    Add(files);
                }
            }
        }

        public void Add(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                var fileItem = new FileItem(item);
                if (string.IsNullOrWhiteSpace(baseFolder))
                {
                    if (fileItem.IsFile)
                    {
                        BaseFolder = Path.GetDirectoryName(item)!;
                    } else if (items.Count() > 1) 
                    {
                        BaseFolder = Path.GetDirectoryName(item)!;
                    } else
                    {
                        BaseFolder = item;
                        return;
                    }
                }
                fileItem.Name = GetRelativeFileName(item);
                if (string.IsNullOrWhiteSpace(fileItem.Name))
                {
                    continue;
                }
                if (FileItems.Contains(fileItem))
                {
                    continue;
                }
                FileItems.Add(fileItem);
            }
        }

        private string GetRelativeFileName(string file)
        {
            if (!file.StartsWith(baseFolder))
            {
                return string.Empty;// Path.GetFileName(file);
            }
            return file.Substring(BaseFolder.Length + 1);
        }

        public static void CreateDirectory(string fileName)
        {
            if (File.Exists(fileName))
            {
                return;
            }
            //判断路径中的文件夹是否存在
            var folder = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        public static void CopyDirectory(string srcPath, string destPath)
        {
            try
            {
                CreateDirectory(destPath);
                var dir = new DirectoryInfo(srcPath); 
                foreach (var i in dir.GetFileSystemInfos())
                {
                    var destFileName = Path.Combine(destPath, i.Name); 
                    if (i is DirectoryInfo)     //判断是否文件夹
                    {
                        if (!Directory.Exists(destFileName))
                        {
                            Directory.CreateDirectory(destFileName);   //目标目录下不存在此文件夹即创建子文件夹
                        }
                        CopyDirectory(i.FullName, destFileName);    //递归调用复制子文件夹
                    }
                    else
                    {
                        File.Copy(i.FullName, destFileName, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}
