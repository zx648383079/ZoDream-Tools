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

namespace ZoDream.FindFile.Controls
{
    /// <summary>
    /// ComparePanel.xaml 的交互逻辑
    /// </summary>
    public partial class ComparePanel : UserControl
    {
        public ComparePanel()
        {
            InitializeComponent();
        }

        public FileCompareFlags CompareFlags
        {
            get
            {
                var flag = (FileCompareFlags)0;
                if (NameTb.IsChecked == true)
                {
                    flag |= FileCompareFlags.Name;
                }
                if (SizeTb.IsChecked == true)
                {
                    flag |= FileCompareFlags.Size;
                }
                if (MtimeTb.IsChecked == true)
                {
                    flag |= FileCompareFlags.Mtime;
                }
                if (Md5Tb.IsChecked == true)
                {
                    flag |= FileCompareFlags.MD5;
                }
                if (CrcTb.IsChecked == true)
                {
                    flag |= FileCompareFlags.CRC32;
                }
                return flag;
            }
            set
            {
                NameTb.IsChecked = value.HasFlag(FileCompareFlags.Name);
                SizeTb.IsChecked = value.HasFlag(FileCompareFlags.Size);
                MtimeTb.IsChecked = value.HasFlag(FileCompareFlags.Mtime);
                Md5Tb.IsChecked = value.HasFlag(FileCompareFlags.MD5);
                CrcTb.IsChecked = value.HasFlag(FileCompareFlags.CRC32);
            }
        }
    }
}
