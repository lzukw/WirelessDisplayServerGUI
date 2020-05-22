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
using System.Configuration;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;

namespace WirelessDisplayServerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected string _pathToExecutable { get; }

        protected string _nameOfExecutable { get;  }

        protected Process _backgroundProcess { get; }
        public MainWindow()
        {
            InitializeComponent();
            
            //
            // Find out local IP-Address and put it into labelIp
            //
            string ipv4Address = "unknown";
            string sHostName = System.Net.Dns.GetHostName();
            System.Net.IPHostEntry ipE = System.Net.Dns.GetHostEntry(sHostName); 
            foreach (System.Net.IPAddress ip in ipE.AddressList)
            {
                if ( ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ) //ipv4
                {
                        ipv4Address = ip.ToString();
                }
            }

            labelIp.Content = ipv4Address;

            //
            // Read in Configuration and spawn process
            //
            _pathToExecutable = ConfigurationManager.AppSettings["PathToWirelessDisplayServerExe"];
            FileInfo executable = new FileInfo(_pathToExecutable);
            if ( ! executable.Exists )
            {
                MessageBox.Show( $"Path to WirelessDisplayServer-Executable doesn't exist: '{executable.FullName}'. Consider changing App.config or WirelessDisplayServerGUI.dll.config",
                                "FATAL", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new FileNotFoundException($"Path to WirelessDisplayServer-Executable doesn't exist: '{executable.FullName}'");
            }

            // first kill old processes
            string processName = System.IO.Path.GetFileNameWithoutExtension(executable.FullName);
            Process[] ps = Process.GetProcessesByName(processName);
            foreach (Process p in ps)
            {
                p.Kill();
            }

            // spawn process
            _backgroundProcess = new Process();
            _backgroundProcess.StartInfo.CreateNoWindow = true;
            _backgroundProcess.StartInfo.UseShellExecute = false;
            _backgroundProcess.StartInfo.RedirectStandardInput = true;
            _backgroundProcess.StartInfo.RedirectStandardOutput = true;
            _backgroundProcess.StartInfo.RedirectStandardError = true;
            _backgroundProcess.StartInfo.WorkingDirectory = executable.DirectoryName;
            _backgroundProcess.StartInfo.FileName = executable.FullName;
            _backgroundProcess.OutputDataReceived += backgroundProcess_DataReceived;
            _backgroundProcess.ErrorDataReceived += backgroundProcess_DataReceived;

            bool processStarted = _backgroundProcess.Start();
            if (! processStarted)
            {
                throw new Exception("Could not start WirelessDisplayServer as background-process!");
            }
            // This is necessary to start a thread, that receives the things written to
            // stdout and stderr by the process. 
            _backgroundProcess.BeginOutputReadLine();
            _backgroundProcess.BeginErrorReadLine();

       }

        void backgroundProcess_DataReceived(object sender, DataReceivedEventArgs e)
        {
            // This event-Handler runs in another thread (not the GUI-Thread), so it
            // needs to use Dispatcher.Invoke in order to change GUI-Elements.
            if ( ! String.IsNullOrEmpty(e.Data) )
            {
                Dispatcher.Invoke( ()=>{textblockLog.Text += e.Data + "\n"; }  );
            }
        }
    
        void mainWindow_Closing(object sender, object e)
        {
            // Stop background-process (WirelessDisplayServer)
            _backgroundProcess.Kill();
        }
    }
}
