using System;
using System.Collections.Generic;
using System.IO;
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

namespace TxtToDpl
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string file;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            file = Helper.ChooseFile();
            SaveBtn.IsEnabled = true;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(file))
            {
                MessageBox.Show("请选择文件");
                return;
            }
            if (!File.Exists(file))
            {
                MessageBox.Show("请选择文件");
                return;
            }
            var name = System.IO.Path.GetFileNameWithoutExtension(file);
            var saveFile = Helper.ChooseSaveFile(name + ".dpl", "Potplayer直播源|*.dpl");
            if (string.IsNullOrEmpty(saveFile))
            {
                MessageBox.Show("请选择要保存的路径");
                return;
            }
            Helper.Converter(saveFile, file);
            MessageBox.Show("保存成功");
        }
    }
}
