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
            Loaded += ComparePanel_Loaded;
        }

        private void ComparePanel_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in GroupPanel.Children)
            {
                if (item is CheckBox box)
                {
                    box.Checked += CheckBox_Changed;
                    box.Unchecked += CheckBox_Changed;
                }
            }
        }

        public FileCompareFlags Value {
            get { return (FileCompareFlags)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(FileCompareFlags), typeof(ComparePanel), 
                new PropertyMetadata(FileCompareFlags.None, (d, s) => {
                    (d as ComparePanel)?.UpdateValue();
                }));

        public FileCompareFlags CompareFlags
        {
            get
            {
                var flag = FileCompareFlags.None;
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

        private void UpdateValue()
        {
            CompareFlags = Value;
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            Value = CompareFlags;
        }
    }
}
