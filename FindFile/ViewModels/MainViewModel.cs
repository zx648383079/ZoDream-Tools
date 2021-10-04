using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.FindFile.Models;

namespace ZoDream.FindFile.ViewModels
{
    public class MainViewModel: BindableBase
    {

        private ObservableCollection<string> folderItems = new();

        public ObservableCollection<string> FolderItems
        {
            get => folderItems;
            set => Set(ref folderItems, value);
        }

        private ObservableCollection<FilenNotifyItem> fileItems = new ObservableCollection<FilenNotifyItem>();

        public ObservableCollection<FilenNotifyItem> FileItems
        {
            get => fileItems;
            set => Set(ref fileItems, value);
        }

        private CancellationTokenSource messageToken = new CancellationTokenSource();
        private string message = string.Empty;

        public string Message
        {
            get => message;
            set => Set(ref message, value);
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
    }
}
