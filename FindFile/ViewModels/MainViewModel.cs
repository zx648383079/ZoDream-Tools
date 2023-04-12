using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ZoDream.FindFile.Models;
using ZoDream.FindFile.Shared;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ZoDream.FindFile.ViewModels
{
    public class MainViewModel: BindableBase
    {

        public MainViewModel()
        {
            FindCommand = new RelayCommand(TapFind);
            ResetCommand = new RelayCommand(TapReset);
            DeleteCommand = new RelayCommand(TapDelete);
            CheckCommand = new RelayCommand(TapCheck);
            OpenFolderCommand = new RelayCommand(TapOpenFolder);
            DeleteFolderCommand = new RelayCommand(TapDeleteFolder);
            CheckFileCommand = new RelayCommand(TapCheckFile);
            SeeFileCommand = new RelayCommand(TapSeeFile);
            Finder.FileChanged += Finder_FileChanged;
            Finder.FoundChanged += Finder_FoundChanged;
            Finder.Finished += Finder_Finished;
        }

        private readonly StorageFinder Finder = new();

        private FileCompareFlags compareFlags = FileCompareFlags.MD5;

        public FileCompareFlags CompareFlags {
            get => compareFlags;
            set => Set(ref compareFlags, value);
        }

        private FileFilterFlags filterFlags = FileFilterFlags.NotEmpty;

        public FileFilterFlags FilterFlags {
            get => filterFlags;
            set => Set(ref filterFlags, value);
        }



        private ObservableCollection<string> folderItems = new();

        public ObservableCollection<string> FolderItems
        {
            get => folderItems;
            set => Set(ref folderItems, value);
        }

        private ObservableCollection<FilenNotifyItem> fileItems = new();

        public ObservableCollection<FilenNotifyItem> FileItems
        {
            get => fileItems;
            set => Set(ref fileItems, value);
        }

        private bool isPaused = true;

        public bool IsPaused {
            get => isPaused;
            set {
                Set(ref isPaused, value);
                FindText = value ? "查找" : "停止查找";
            }
        }

        private string findText = "查找";

        public string FindText {
            get => findText;
            set => Set(ref findText, value);
        }


        private CancellationTokenSource messageToken = new();
        private string message = string.Empty;

        public string Message
        {
            get => message;
            set => Set(ref message, value);
        }

        public ICommand FindCommand { get; set; }
        public ICommand ResetCommand { get; set; }

        public ICommand CheckCommand { get; set; }

        public ICommand DeleteCommand { get; set; }
        public ICommand OpenFolderCommand { get; set; }
        public ICommand DeleteFolderCommand { get; set; }
        public ICommand CheckFileCommand { get; set; }

        public ICommand SeeFileCommand { get; set; }

        private void TapFind(object? _)
        {
            if (!IsPaused)
            {
                Finder.Stop();
                return;
            }
            if (FolderItems.Count == 0)
            {
                MessageBox.Show("请选择文件夹");
                return;
            }
            FileItems.Clear();
            IsPaused = false;
            Finder.CompareFlags = CompareFlags;
            Finder.FilterFlags = FilterFlags;
            Finder.Start(FolderItems.ToArray());
        }

        private void TapReset(object? _)
        {
            FileItems.Clear();
            FolderItems.Clear();
            Finder.Stop();
        }

        private void TapCheck(object? _)
        {
            if (FileItems.Count == 0)
            {
                MessageBox.Show("没有可用数据", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (FileItems.Any(x => x.IsChecked))
            {
                foreach (var item in FileItems)
                {
                    item.IsChecked = false;
                }
            }
            else
            {
                foreach (var item in FileItems)
                {
                    item.IsChecked = true;
                }
                var keyList = FileItems.GroupBy(x => x.Guid);
                foreach (var keyItem in keyList)
                {
                    FileItems.First(x => x.Guid == keyItem.Key).IsChecked = false;
                }
            }
        }

        private void TapDelete(object? _)
        {
            try
            {
                if (FileItems.Count == 0)
                {
                    MessageBox.Show("没有可用数据", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (MessageBox.Show($"确认要删除选中文件吗？文件删除后不可恢复！", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                foreach (var file in FileItems)
                {
                    if (file.IsChecked == false)
                    {
                        continue;
                    }
                    System.IO.File.Delete(file.FileName);
                }
                FileItems.Clear();
                MessageBox.Show("删除完成", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败：{ex.Message}", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TapOpenFolder(object? _)
        {
            if (!IsPaused)
            {
                return;
            }
            var picker = new System.Windows.Forms.FolderBrowserDialog();
            if (picker.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            AddFolder(picker.SelectedPath);
        }

        private void TapDeleteFolder(object? arg)
        {
            if (arg is string item)
            {
                FolderItems.Remove(item);
            }
        }

        private void TapCheckFile(object? arg)
        {
            if (arg is FilenNotifyItem o)
            {
                if (!FileItems.Any(x => x.Guid == o.Guid && !x.IsChecked))
                {
                    MessageBox.Show("必须至少保留重复文件中的一个", "提示", MessageBoxButton.OK, MessageBoxImage.Stop);
                    o.IsChecked = false;
                    return;
                }
            }
        }

        private void TapSeeFile(object? arg)
        {
            if (arg is FilenNotifyItem o)
            {
                Process.Start("explorer", $"/select,{o.FileName}");
            }
        }

        private void Finder_Finished()
        {
            App.Current.Dispatcher.Invoke(() => {
                ShowMessage("查找完成");
                IsPaused = true;
            });
        }

        private void Finder_FoundChanged(Models.FileItem item)
        {
            App.Current.Dispatcher.Invoke(() => {
                FileItems.Add(new FilenNotifyItem(item));
            });
        }

        private void Finder_FileChanged(string fileName)
        {
            App.Current.Dispatcher.Invoke(() => {
                Message = fileName;
            });

        }

        public void ShowMessage(string message)
        {
            messageToken.Cancel();
            messageToken = new CancellationTokenSource();
            var token = messageToken.Token;
            Message = message;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(3000);
                if (token.IsCancellationRequested)
                {
                    return;
                }
                Message = string.Empty;
            }, token);
        }

        public void AddFolder(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                AddFolder(item);
            }
        }

        public void AddFolder(string item)
        {
            if (FolderItems.Contains(item))
            {
                return;
            }
            FolderItems.Add(item);
        }
    }
}
