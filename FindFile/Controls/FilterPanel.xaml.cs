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
            Loaded += FilterPanel_Loaded;
        }

        private void FilterPanel_Loaded(object sender, RoutedEventArgs e)
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

        public FileFilterFlags Value {
            get { return (FileFilterFlags)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(FileFilterFlags), 
                typeof(FilterPanel), new PropertyMetadata(FileFilterFlags.None, (d, s) => {
                    (d as FilterPanel)?.UpdateValue();
                }));




        public FileFilterFlags FilterFlags
        {
            get {
                var flag = FileFilterFlags.None;
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

        private void UpdateValue()
        {
            FilterFlags = Value;
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            Value = FilterFlags;
        }
    }
}
