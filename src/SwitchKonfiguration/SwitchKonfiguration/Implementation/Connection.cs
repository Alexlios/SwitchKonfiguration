using SwitchKonfiguration.Types;
using System;
using System.IO.Ports;
using System.Threading;
using System.Windows;

namespace SwitchKonfiguration.Implementation
{
    class Connection
    {
        #region PrivateFields

        private SerialPort _serialPort;
        private Commander _commander;
        private string _password;
        private bool _loggedIn;
        private string _port;

        #endregion

        #region Properties

        public string TmpError
        {
            get
            {
                return _commander.TmpError;
            }
        }

        public bool IsConnected
        {
            get
            {
                return _serialPort.IsOpen;
            }
        }

        #endregion

        #region PublicMethods

        /// <summary>
        /// Opens the connection to a SerialPort
        /// </summary>
        /// <param name="comPort">The port to connect to</param>
        /// <returns>if it succeeded (true) or not (false)</returns>
        public bool Connect(string comPort)
        {
            _port = comPort;

            //tries to establish a connection to the Switch
            try
            {
                _serialPort = new SerialPort(comPort);

                _loggedIn = false;

                _serialPort.Close();

                _serialPort.BaudRate = 115200;
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.One;

                _serialPort.Open();

                _serialPort.DataReceived += _serialPort_DataReceived;

                _commander = new Commander(_serialPort);
                _commander.ExecuteCommand("");
            }
            catch (Exception)
            {
                return false; ;
            }
            return true;
        }
        
        /// <summary>
        /// Closes the current connection
        /// </summary>
        public void Close()
        {
            _serialPort.Close();
        }

        /// <summary>
        /// Logs into the Switch
        /// </summary>
        /// <param name="pwd">The password to be used</param>
        /// <returns>if it succeeded (true) or not (false)</returns>
        public bool Login(string pwd)
        {
            //waits for the EventHandler to log in
            _password = pwd;
            for (int i = 60; i > 0; --i)
            {
                Thread.Sleep(1000);
            }
            return _loggedIn;
        }

        /// <summary>
        /// Downgrades the firmware of the switch
        /// </summary>
        /// <param name="tftpIP">The IP of the TFTP-Server it will load the firmware file from</param>
        /// <param name="updateFile">The name of the firmware file</param>
        /// <returns>if it succeeded (true) or not (false)</returns>
        public bool Downgrade(IPv4 tftpIP, string updateFile)
        {
            //lets the commander execute all the commands, then reboots
            if (_commander.Downgrade(tftpIP, updateFile))
            {
                return Reboot();
            }
            return false;
        }

        /// <summary>
        /// Resets the Switch to its factory default configuration
        /// </summary>
        /// <returns>if it succeeded (true) or not (false)</returns>
        public bool FactoryReset()
        {
            //lets the commander execute all the commands, then reboots
            if (_commander.FactoryReset())
            {
                return Reboot();
            }
            return false;
        }

        /// <summary>
        /// Configures the Switch acording to the specifications needed
        /// </summary>
        /// <param name="switchIP">The IP the Switch will have</param>
        /// <param name="hostname">The hostname the Switch will have</param>
        /// <param name="timeSrvIP">The IP of the timeserver from which the switch pulls its time</param>
        /// <param name="srvIP">The IP of the Server it will be connected to</param>
        /// <param name="newPwd">The new password that will be given</param>
        /// <returns>if it succeeded (true) or not (false)</returns>
        public bool Configs(IPv4 switchIP, string hostname, IPv4 timeSrvIP, IPv4 srvIP, string newPwd)
        {
            //lets the commander execute all the commands
            return _commander.Configs(switchIP, hostname, timeSrvIP, srvIP, newPwd);
        }

        /// <summary>
        /// Reboots the Switch
        /// </summary>
        /// <returns>if it succeeded (true) or not (false)</returns>
        public bool Reboot()
        {
            //removes the EventHandler so it doesnt break out of the routine
            _serialPort.DataReceived -= _serialPort_DataReceived;
            
            //executes the commands to reboot
            _commander.canSend = true;
            if (!_commander.ExecuteCommand("reload")) return false;
            _commander.canSend = true;
            if (!_commander.ExecuteCommand("y")) return false;
            
            //waits for the switch to boot again
            Thread.Sleep(5000);
            _loggedIn = false;
            _commander.canSend = false;
            Close();
            Thread.Sleep(120000);

            //re-establishes the connection
            Connect(_port);

            //if canSend is false => Switch didnt send Data yet
            if (!_commander.canSend)
            {
                //try to send a <ENTER> so the switch starts to respond
                _commander.canSend = true;
                if (!_commander.ExecuteCommand(""))
                {
                    //if canSend is still false wait a bit and try a second and last time
                    if (!_commander.canSend)
                    {
                        Thread.Sleep(60000);
                        _commander.canSend = true;
                        if (!_commander.ExecuteCommand("")) return false;
                    }
                }
            }

            //switch is booted again and EventHandler probably logged in
            return true;
        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// EventHandler for when the switch sends data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string receivedData = string.Empty;

            //wait a second so all the data gets transmitted
            Thread.Sleep(1000);

            SerialPort receiver = (SerialPort)sender;

            if (!receiver.IsOpen) return;

            //get the data that was send
            receivedData = receiver.ReadExisting();

            //if the data contains a "error" show an error
            if (receivedData.Contains("Error"))
            {
                Error();
                return;
            }

            //if the data contains any of the following we can send a command again
            if ((receivedData.Contains("Console#") && !receivedData.Contains("Restarting System")) ||
                receivedData.Contains("Console(config)") || receivedData.Contains("Console(config-if") || receivedData.Contains("Continue <y/n>?") ||
                receivedData.Contains("TFTP server IP address") || receivedData.Contains("Source file name:") || receivedData.Contains("Choose file type:") ||
                receivedData.Contains("Destination file name:") || receivedData.Contains("Startup configuration file name [startup1.cfg]:"))
            {
                _commander.canSend = true;
            }
            else//else do specific commands
            {
                if (receivedData.Contains("Username:"))
                {
                    _commander.canSend = true;
                    _commander.ExecuteCommand("admin");
                }
                else
                {
                    if (receivedData.Contains("Password:"))
                    {
                        _commander.canSend = true;
                        _commander.ExecuteCommand(_password);
                        _loggedIn = true;
                        _commander.canSend = true;
                    }
                    else
                    {
                        if (receivedData.Contains("Press ENTER to start session"))
                        {
                            _commander.canSend = true;
                            _commander.ExecuteCommand("");
                        }
                        else//data cant be understood wait for understandable data
                        {
                            _commander.canSend = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Displays an Error-MessageBox
        /// </summary>
        private void Error()
        {
            Close();
            MessageBox.Show("An Error occured!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}
