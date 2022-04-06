using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using ZoDream.ControlBrowser.Comparers;

namespace ZoDream.ControlBrowser.ViewModels
{
    public class MainViewModel: BindableBase
    {

        public MainViewModel()
        {
            LoadAsync();
        }


        private List<Type> controlItems = new();

        public List<Type> ControlItems
        {
            get => controlItems;
            set => Set(ref controlItems, value);
        }

        private string controlContent = string.Empty;

        public string ControlContent
        {
            get => controlContent;
            set => Set(ref controlContent, value);
        }


        public void LoadAsync()
        {
            var controlType = typeof(Control);
            var assembly = Assembly.GetAssembly(typeof(Control));
            if (assembly == null)
            {
                return;
            }
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(controlType) && !type.IsAbstract && type.IsPublic)
                {
                    ControlItems.Add(type);
                }
            }
            ControlItems.Sort(new TypeComparer());
        }

        public void TypeChange(Type item, Panel panel)
        {
            try
            {
                // Instantiate the type.
                var info = item.GetConstructor(Type.EmptyTypes);
                if (info == null)
                {
                    throw new ArgumentException();
                }
                var control = (Control)info.Invoke(null);
                var win = control as Window;
                if (win != null)
                {
                    // Create the window (but keep it minimized).
                    win.WindowState = WindowState.Minimized;
                    win.ShowInTaskbar = false;
                    win.Show();
                }
                else
                {
                    // Add it to the grid (but keep it hidden).
                    control.Visibility = Visibility.Collapsed;
                    panel.Children.Add(control);
                }

                // Get the template.
                var template = control.Template;

                // Get the XAML for the template.
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                var sb = new StringBuilder();
                var writer = XmlWriter.Create(sb, settings);
                XamlWriter.Save(template, writer);

                // Display the template.
                ControlContent = sb.ToString();

                // Remove the control from the grid.
                if (win != null)
                {
                    win.Close();
                }
                else
                {
                    panel.Children.Remove(control);
                }
            }
            catch (Exception err)
            {
                ControlContent = "<< Error generating template: " + err.Message + ">>";
            }
        }

        public void TypeChange(int index, Panel panel)
        {
            TypeChange(controlItems[index], panel);
        }


    }
}
