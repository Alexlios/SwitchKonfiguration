using SwitchKonfiguration.Implementation;
using SwitchKonfiguration.Types;
using System;
using System.Windows;
using System.Windows.Input;

namespace SwitchKonfiguration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void KonfigureECS2100_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            IPv4 switchIP = new IPv4(SwitchIPECS2100.Text);
            IPv4 timeSrvIP = new IPv4(ZeitserverIPECS2100.Text);
            IPv4 SrvIP = new IPv4(ServerIPECS2100.Text);
            IPv4 tftpIP = new IPv4(TFTPIPECS2100.Text);
            try
            {

                ECS2100.Configure(SwitchNameECS2100.Text, switchIP, timeSrvIP, SrvIP, tftpIP, OldPwdECS2100.Text, NewPwdECS2100.Text, COMPortECS2100.Text);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
            }
            Cursor = Cursors.Arrow;
        }

        private void ExcelView_Drop(object sender, DragEventArgs e)
        {
            //((string[])e.Data.GetData(DataFormats.FileDrop))[0]

        }
    }
}
