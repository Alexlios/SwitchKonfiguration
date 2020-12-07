using SwitchKonfiguration.Implementation;
using SwitchKonfiguration.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SwitchKonfiguration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void KonfigureECS2100_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            ECS2100 Switch = new ECS2100(SwitchNameECS2100.Text, SwitchIPECS2100.Text, ZeitserverIPECS2100.Text, ServerIPECS2100.Text, TFTPIPECS2100.Text, OldPwdECS2100.Text, NewPwdECS2100.Text, COMPortECS2100.Text);
            if (!Switch.Configure())
            {
                MessageBox.Show(Out.output);
            }
            Cursor = Cursors.Arrow;
        }
    }
}
