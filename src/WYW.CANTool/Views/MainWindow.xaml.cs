using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Shapes;
using WYW.CANTool.ViewModes;

namespace WYW.CANTool.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = "CAN调试助手 V1.0.1";
            if (SystemParameters.PrimaryScreenHeight < 800)
            {
                this.Height = SystemParameters.WorkArea.Height;
                this.Width = this.Height * 4 / 3;
                this.Top = 0;
                this.Left = (SystemParameters.WorkArea.Width - this.Width) / 2;
            }
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Environment.Exit(Environment.ExitCode);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
