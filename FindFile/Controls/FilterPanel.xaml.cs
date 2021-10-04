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
    /// FilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class FilterPanel : UserControl
    {
        public FilterPanel()
        {
            InitializeComponent();
        }



        public FileFilterFlags FilterFlags
        {
            get {
                var flag = (FileFilterFlags)0;
                if (EmptyTb.IsChecked == true)
                {
                    flag |= FileFilterFlags.NotEmpty;
                }
                if (HiddenTb.IsChecked == true)
                {
                    flag |= FileFilterFlags.NotHidden;
                }
                if (SmallTb.IsChecked == true)
                {
                    flag |= FileFilterFlags.NotLess;
                }
                if (ImageTb.IsChecked == true)
                {
                    flag |= FileFilterFlags.FindImage;
                }
                if (MediaTb.IsChecked == true)
                {
                    flag |= FileFilterFlags.FindMedia;
                }
                if (ZipTb.IsChecked == true)
                {
                    flag |= FileFilterFlags.FindZip;
                }
                if (DocumentTb.IsChecked == true)
                {
                    flag |= FileFilterFlags.FindDoc;
                }
                return flag;
            }
            set {
                EmptyTb.IsChecked = value.HasFlag(FileFilterFlags.NotEmpty);
                HiddenTb.IsChecked = value.HasFlag(FileFilterFlags.NotHidden);
                SmallTb.IsChecked = value.HasFlag(FileFilterFlags.NotLess);
                ImageTb.IsChecked = value.HasFlag(FileFilterFlags.FindImage);
                MediaTb.IsChecked = value.HasFlag(FileFilterFlags.FindMedia);
                ZipTb.IsChecked = value.HasFlag(FileFilterFlags.FindZip);
                DocumentTb.IsChecked = value.HasFlag(FileFilterFlags.FindDoc);
            }
        }

    }
}
