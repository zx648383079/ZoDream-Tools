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

        private readonly ConcurrentDictionary<long, List<FileItem>> _fileSizeItems = new();
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
            var token = _cancelTokenSource.Token;
            var mapFinished = false;
            Task.Factory.StartNew(() =>
            {
                foreach (var item in folders)
                {
                    if (_cancelTokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                    CheckFolderOrFile(item, token);
                }
                mapFinished = true;
            }, token);
            Task.Factory.StartNew(() => {
                while (true)
                {
                    Thread.Sleep(100);
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    foreach (var item in _fileSizeItems)
                    {
                        if (item.Value.Count < 2)
                        {
                            continue;
                        }
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }
                        if (mapFinished)
                        {
                            FileChanged?.Invoke(item.Value[0].FileName);
                        }
                        CompleteFiles(item.Value, token);
                    }
                    if (mapFinished)
                    {
                        break;
                    }
                }
                Finished?.Invoke();
            }, token);
        }

        public void Stop()
        {
            if (_cancelTokenSource != null)
            {
                _cancelTokenSource.Cancel();
                Finished?.Invoke();
            }
            _fileGroupItems.Clear();
            _fileSizeItems.Clear();
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

        private void CheckFolderOrFile(string folder, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }
            var file = new FileInfo(folder);
            if (file.Exists)
            {
                CheckFile(folder, token);
                return;
            }
            EachFiles(folder, items =>
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                foreach (var item in items)
                {
                    CheckFile(item, token);
                }
            });
        }

        private void CheckFile(string fileName, CancellationToken token)
        {
            if (token.IsCancellationRequested)
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
            //}, token);
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
            var items = new List<FileItem>() { item };
            var resItems = _fileSizeItems.AddOrUpdate(item.Size, items, (k, oldItems) => {
                oldItems.Add(item);
                return oldItems;
            });
        }

        private void AppendFile(FileItem item, CancellationToken token)
        {
            if (token.IsCancellationRequested)
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

        private void CompleteFiles(List<FileItem> items, CancellationToken token)
        {
            foreach (var item in items)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                CompleteFile(item, token);
            }
        }

        private void CompleteFile(FileItem file, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(file.Md5) || !string.IsNullOrEmpty(file.Crc32) || !string.IsNullOrEmpty(file.Guid))
            {
                return;
            }
            var hasMd5 = CompareFlags.HasFlag(FileCompareFlags.MD5);
            var hasCrc = CompareFlags.HasFlag(FileCompareFlags.CRC32);
            if (!hasMd5 && !hasCrc)
            {
                file.Guid = FormatGuid(file);
                AppendFile(file, token);
                return;
            }
            using (var fs = File.OpenRead(file.FileName))
            {
                if (hasMd5) {
                    file.Md5 = Storage.GetMD5(fs);
                }
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (hasCrc)
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    file.Crc32 = Storage.GetCRC32(fs);
                }
            }
            file.Guid = FormatGuid(file);
            AppendFile(file, token);
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
#if NET6_0_OR_GREATER
            var data = MD5.HashData(buffer);
#else
            var data = MD5.Create().ComputeHash(buffer);
#endif
            sb.Clear();
            foreach (var t in data)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 过滤文件
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
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
