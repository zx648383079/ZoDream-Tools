using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using ZoDream.FindFile.Models;
using ZoDream.FindFile.Shared.Filters;
using ZoDream.FindFile.Utils;
using System.Security.Cryptography;
using System.Collections.Concurrent;

namespace ZoDream.FindFile.Shared
{
    public class StorageFinder
    {
        private CancellationTokenSource? _cancelTokenSource;

        private IList<IFileFilter>? _filterItems;

        private readonly ConcurrentDictionary<string, List<FileItem>> _fileGroupItems = new();

        public event FinderLogEventHandler? FileChanged;

        public event FinderEventHandler? FoundChanged;

        public event FinderFinishedEventHandler? Finished;

        private FileFilterFlags filterFlags;

        public FileFilterFlags FilterFlags
        {
            get { return filterFlags; }
            set { 
                filterFlags = value;
                _filterItems = null;
            }
        }


        public FileCompareFlags CompareFlags { get; set; }

        public void Start(IEnumerable<string> folders)
        {
            Stop();
            _cancelTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                foreach (var item in folders)
                {
                    if (_cancelTokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                    RunFolder(item);
                }
                Finished?.Invoke();
            }, _cancelTokenSource.Token);
        }

        public void Stop()
        {
            if (_cancelTokenSource != null)
            {
                _cancelTokenSource.Cancel();
                Finished?.Invoke();
            }
            _fileGroupItems.Clear();
        }

        private void InitFilters()
        {
            if (_filterItems != null)
            {
                return;
            }
            _filterItems = new List<IFileFilter>();
            if (FilterFlags.HasFlag(FileFilterFlags.NotEmpty))
            {
                _filterItems.Add(new EmptyFileFilter(true));
            }
            if (FilterFlags.HasFlag(FileFilterFlags.NotHidden))
            {
                _filterItems.Add(new HiddenFileFilter(true));
            }
            if (FilterFlags.HasFlag(FileFilterFlags.NotLess))
            {
                _filterItems.Add(new SizeFileFilter(1024 * 1024));
            }
            var filter = new ExtensionFileFilter();
            if (FilterFlags.HasFlag(FileFilterFlags.FindDoc))
            {
                filter.Add(ExtensionFileFilter.DOCUMENT_FLAGS);
            }
            if (FilterFlags.HasFlag(FileFilterFlags.FindImage))
            {
                filter.Add(ExtensionFileFilter.IMAGE_FLAGS);
            }
            if (FilterFlags.HasFlag(FileFilterFlags.FindZip))
            {
                filter.Add(ExtensionFileFilter.ZIPARCHIVE_FLAGS);
            }
            if (FilterFlags.HasFlag(FileFilterFlags.FindMedia))
            {
                filter.Add(ExtensionFileFilter.MEDIA_FLAGS);
            }
            _filterItems.Add(filter);
        }

        private void RunFolder(string folder)
        {
            if (_cancelTokenSource == null || _cancelTokenSource.IsCancellationRequested)
            {
                return;
            }
            var file = new FileInfo(folder);
            if (file.Exists)
            {
                RunFile(folder);
                return;
            }
            EachFiles(folder, items =>
            {
                if (_cancelTokenSource.IsCancellationRequested)
                {
                    return;
                }
                foreach (var item in items)
                {
                    RunFile(item);
                }
            });
        }

        private void RunFile(string fileName)
        {
            if (_cancelTokenSource == null || _cancelTokenSource.IsCancellationRequested)
            {
                return;
            }
            FileChanged?.Invoke(fileName);
            var fileInfo = new FileInfo(fileName);
            if (!IsValidFile(fileInfo))
            {
                return;
            }
            CheckFile(fileInfo);
            //Task.Factory.StartNew(() => {
            //    CheckFile(fileInfo);
            //}, _cancelTokenSource.Token);
        }

        private void CheckFile(FileInfo fileInfo)
        {
            var item = new FileItem()
            {
                Name = fileInfo.Name,
                FileName = fileInfo.FullName,
                Extension = fileInfo.Extension,
                Size = fileInfo.Length,
                Mtime = fileInfo.LastWriteTime,
                Ctime = fileInfo.CreationTime,
            };
            using (var fs = fileInfo.OpenRead())
            {
                item.Md5 = Storage.GetMD5(fs);
                if (_cancelTokenSource.IsCancellationRequested)
                {
                    return;
                }
                fs.Seek(0, SeekOrigin.Begin);
                item.Crc32 = Storage.GetCRC32(fs);
            }
            item.Guid = FormatGuid(item);
            AppendFile(item);
        }

        private void AppendFile(FileItem item)
        {
            if (_cancelTokenSource == null || _cancelTokenSource.IsCancellationRequested)
            {
                return;
            }
            var items = new List<FileItem>() { item };
            var resItems = _fileGroupItems.AddOrUpdate(item.Guid, items, (k, oldItems) =>
            {
                oldItems.Add(item);
                return oldItems;
            });
            if (resItems.Count > 1)
            {
                if (resItems.Count == 2)
                {
                    FoundChanged?.Invoke(resItems[0]);
                }
                FoundChanged?.Invoke(item);
            }
        }

        private string FormatGuid(FileItem item)
        {
            var sb = new StringBuilder();
            if (CompareFlags.HasFlag(FileCompareFlags.Name))
            {
                sb.Append(item.Name);
            }
            if (CompareFlags.HasFlag(FileCompareFlags.Size))
            {
                sb.Append(item.Size);
            }
            if (CompareFlags.HasFlag(FileCompareFlags.Mtime))
            {
                sb.Append(item.Mtime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            if (CompareFlags.HasFlag(FileCompareFlags.MD5))
            {
                sb.Append(item.Md5);
            }
            if (CompareFlags.HasFlag(FileCompareFlags.CRC32))
            {
                sb.Append(item.Crc32);
            }
            var buffer = Encoding.Default.GetBytes(sb.ToString());
            var data = MD5.Create().ComputeHash(buffer);
            sb.Clear();
            foreach (var t in data)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }

        private bool IsValidFile(FileInfo fileInfo)
        {
            InitFilters();
            if (_filterItems == null)
            {
                return true;
            }
            foreach (var item in _filterItems)
            {
                if (!item.Valid(fileInfo))
                {
                    return false;
                }
            }
            return true;
        }

        private void EachFiles(string folder, Action<IEnumerable<string>> success)
        {
            try
            {
                if (_cancelTokenSource == null || _cancelTokenSource.IsCancellationRequested)
                {
                    return;
                }
                Array.ForEach(Directory.GetDirectories(folder), fileName =>
                {
                    EachFiles(fileName, success);
                });
                success.Invoke(Directory.GetFiles(folder));
            }
            catch (UnauthorizedAccessException)
            {

            }
        }
    }
}
