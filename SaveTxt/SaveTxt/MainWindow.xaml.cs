using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using ZoDream.Helper.Local;

namespace SaveTxt
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ContentTb.Text))
            {
                return;
            }
            ContentTb.Text = ChineseConverter.Convert(ContentTb.Text, ChineseConversionDirection.TraditionalToSimplified);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ContentTb.Text) || string.IsNullOrWhiteSpace(FileTb.Text) ||
                string.IsNullOrWhiteSpace(DirTb.Text))
            {
                return;
            }
            var file = DirTb.Text.TrimEnd('\\') + "\\" + FileTb.Text + ".txt";
            if (File.Exists(file))
            {
                var result = MessageBox.Show("已存在文件，是否替换？", "提示", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        return;
                    case MessageBoxResult.No:
                        file = DirTb.Text.TrimEnd('\\') + "\\" + FileTb.Text + "-" + DateTime.Now.ToLongTimeString()+".txt";
                        break;

                    case MessageBoxResult.None:
                        break;
                    case MessageBoxResult.OK:
                        break;
                    case MessageBoxResult.Yes:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            Open.Writer(file, ContentTb.Text);
            ContentTb.Text = FileTb.Text = string.Empty;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ContentTb.Text = Regex.Replace(Regex.Replace(Regex.Replace(ContentTb.Text, @"(\r\n){2,}|\r{2,}|\n{2,}", "     "), @"(\r|\n|\r\n)+", ""), @"\s+", "\r\n    ").Replace("\\r\\n", "\r\n");
        }
    }
}
