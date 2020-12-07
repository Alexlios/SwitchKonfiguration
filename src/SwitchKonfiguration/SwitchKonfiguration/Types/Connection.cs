using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SwitchKonfiguration.Types
{
    public class Out
    {
        public static string output;
    }

    class Connection
    {
        private SerialPort _serialPort;
        private string _tmpError;
        private readonly int copyWaitTime = 600;
        private bool _canSend;
        private string _password;
        private bool _loggedIn;
        private string _port;

        public string TmpError
        {
            get
            {
                return _tmpError;
            }
        }

        public bool IsConnected
        {
            get
            {
                return _serialPort.IsOpen;
            }
        }



        public void ErrorIsHandled()
        {
            _tmpError = string.Empty;
        }

        public bool Connect(string comPort)
        {
            _port = comPort;

            try
            {
                _serialPort = new SerialPort(comPort);

                _canSend = true;
                _loggedIn = false;

                _serialPort.Close();

                _serialPort.BaudRate = 115200;
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.One;

                _serialPort.Open();

                _serialPort.DataReceived += _serialPort_DataReceived;

                ExecuteCommand("");
            }
            catch (Exception ex)
            {
                _tmpError = ex.Message;
                return false; ;
            }
            return true;
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string receivedData = string.Empty;

            Thread.Sleep(1000);

            SerialPort receiver = (SerialPort)sender;

            if (!receiver.IsOpen) return;

            receivedData = receiver.ReadExisting();

            Out.output += receivedData;

            if (receivedData.Contains("Error"))
            {
                Error();
                return;
            }

            if ((receivedData.Contains("Console#") && !receivedData.Contains("Restarting System")) ||
                receivedData.Contains("Console(config)") || receivedData.Contains("Console(config-if") || receivedData.Contains("Continue <y/n>?") ||
                receivedData.Contains("TFTP server IP address") || receivedData.Contains("Source file name:") || receivedData.Contains("Choose file type:") ||
                receivedData.Contains("Destination file name:") || receivedData.Contains("Startup configuration file name [startup1.cfg]:"))
            {
                _canSend = true;
            }
            else
            {
                if (receivedData.Contains("Username:"))
                {
                    _canSend = true;
                    ExecuteCommand("admin");
                }
                else
                {
                    if (receivedData.Contains("Password:"))
                    {
                        _canSend = true;
                        ExecuteCommand(_password);
                        _loggedIn = true;
                        _canSend = true;
                    }
                    else
                    {
                        if (receivedData.Contains("Press ENTER to start session"))
                        {
                            _canSend = true;
                            ExecuteCommand("");
                        }
                        else
                        {
                            _canSend = false;
                        }
                    }
                }
            }
        }

        public bool Login(string pwd)
        {
            _password = pwd;
            for(int i = 60;i > 0;--i)
            {
                Thread.Sleep(1000);
            }
            return _loggedIn;
        }

        public bool ExecuteCommand(string command, int timeouts = 60)
        {
            try
            {
                Thread.Sleep(1000);
                if (_canSend)
                {
                    _serialPort.WriteLine(command);
                    _canSend = false;
                }
                else
                {
                    throw new TimeoutException();
                }
                return true;
            }
            catch (TimeoutException tex)
            {
                if (!(timeouts < 0) && ExecuteCommand(command, timeouts - 1))
                {
                    return true;
                }
                else
                {
                    _tmpError = tex.Message + "\nFailed Command: " + command;
                }
            }
            catch (Exception ex)
            {
                _tmpError = ex.Message + "\n" + _canSend;
            }
            return false;
        }

        public bool Downgrade(IPv4 tftpIP, string updateFile)
        {
            if (!ExecuteCommand("copy tftp file")) return false;
            if (!ExecuteCommand(tftpIP.ToString())) return false;
            if (!ExecuteCommand("2")) return false;
            if (!ExecuteCommand(updateFile)) return false;
            if (!ExecuteCommand(updateFile)) return false;

            if (!ExecuteCommand("config", copyWaitTime)) return false;
            if (!ExecuteCommand("boot system opcode: " + updateFile)) return false;
            if (!ExecuteCommand("end")) return false;


            return Reboot();
        }

        public bool FactoryReset()
        {
            if (!ExecuteCommand("config")) return false;
            if (!ExecuteCommand("boot system config:Factory_Default_Config.cfg")) return false;
            if (!ExecuteCommand("end")) return false;
            if (!ExecuteCommand("delete file name startup1.cfg")) return false;

            return Reboot();

        }

        public bool Reboot()
        {
            _serialPort.DataReceived -= _serialPort_DataReceived;
            _canSend = true;
            if (!ExecuteCommand("reload")) return false;
            _canSend = true;
            if (!ExecuteCommand("y")) return false;
            Thread.Sleep(5000);
            _loggedIn = false;
            _canSend = false;
            Close();
            Thread.Sleep(120000);
            Connect(_port);
            if (!_canSend)
            {
                _canSend = true;
                if (!ExecuteCommand(""))
                {
                    if(!_canSend)
                    {
                        Thread.Sleep(60000);
                        _canSend = true;
                        if (!ExecuteCommand("")) return false;
                    }
                }
            }

            return true;
        }

        public void Close()
        {
            _serialPort.Close();
        }

        public bool Configs(IPv4 switchIP, string hostname, IPv4 timeSrvIP, IPv4 srvIP, string newPwd)
        {
            if (!ExecuteCommand("config")) return false;
            if (!ExecuteCommand("interface vlan 1")) return false;
            if (!ExecuteCommand($"ip address {switchIP} 255.255.255.0")) return false;
            if (!ExecuteCommand("end")) return false;
            if (!ExecuteCommand("config")) return false;
            if (!ExecuteCommand($"hostname {hostname}")) return false;
            if (!ExecuteCommand("no ip http server")) return false;
            if (!ExecuteCommand("no ip http secure-server")) return false;
            if (!ExecuteCommand("no ip telnet server")) return false;
            if (!ExecuteCommand("no ip mdns")) return false;
            if (!ExecuteCommand("no ip domain-lookup")) return false;
            if (!ExecuteCommand("end")) return false;
            if (!ExecuteCommand("ip ssh crypto host-key generate dsa")) return false;
            if (!ExecuteCommand("ip ssh crypto host-key generate rsa")) return false;
            if (!ExecuteCommand("ip ssh save host-key")) return false;
            if (!ExecuteCommand("config")) return false;
            if (!ExecuteCommand("ip ssh server")) return false;
            if (!ExecuteCommand("no lldp")) return false;
            if (!ExecuteCommand("web-auth system-auth-control")) return false;
            if (!ExecuteCommand("web-auth session-timeout 600")) return false;
            if (!ExecuteCommand($"sntp server {timeSrvIP}")) return false;
            if (!ExecuteCommand("sntp poll 3600")) return false;
            if (!ExecuteCommand("sntp client")) return false;
            if (!ExecuteCommand("no snmp-server group private v1")) return false;
            if (!ExecuteCommand("no snmp-server group public v1")) return false;
            if (!ExecuteCommand("snmp-server community EUA_SWITCH ro")) return false;
            if (!ExecuteCommand("no snmp-server community public")) return false;
            if (!ExecuteCommand("no snmp-server community private")) return false;
            if (!ExecuteCommand("snmp-server enable traps authentication")) return false;
            if (!ExecuteCommand($"snmp-server host {srvIP} EUA_SWITCH version 2c udp-port 162")) return false;
            if (!ExecuteCommand($"snmp-server notify-filter {srvIP}.EUA_SWITCH remote {srvIP}")) return false;
            if (!ExecuteCommand("logging trap level 5")) return false;
            if (!ExecuteCommand($"logging host {srvIP}")) return false;
            if (!ExecuteCommand("logging trap")) return false;
            if (!ExecuteCommand("no logging sendmail")) return false;
            if (!ExecuteCommand("interface vlan 1")) return false;
            if (!ExecuteCommand("no ipv6 enable")) return false;
            if (!ExecuteCommand("no ip proxy-arp")) return false;
            if (!ExecuteCommand("no ip dhcp relay server")) return false;
            if (!ExecuteCommand("end")) return false;
            if (!ExecuteCommand("config")) return false;
            if (!ExecuteCommand($"username guest password 0 {newPwd}")) return false;
            if (!ExecuteCommand($"username admin password 0 {newPwd}")) return false;
            if (!ExecuteCommand("jumbo frame")) return false;
            if (!ExecuteCommand("no spanning-tree")) return false;
            if (!ExecuteCommand("dos-protection echo-chargen")) return false;
            if (!ExecuteCommand("dos-protection smurf")) return false;
            if (!ExecuteCommand("dos-protection tcp-flooding")) return false;
            if (!ExecuteCommand("dos-protection tcp-null-scan")) return false;
            if (!ExecuteCommand("dos-protection tcp-syn-fin-scan")) return false;
            if (!ExecuteCommand("dos-protection tcp-xmas-scan")) return false;
            if (!ExecuteCommand("dos-protection win-nuke")) return false;
            if (!ExecuteCommand("no dos-protection udp-flooding")) return false;
            if (!ExecuteCommand("end")) return false;
            if (!ExecuteCommand("copy running-config startup-config")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("reload")) return false;
            if (!ExecuteCommand("y")) return false;

            return true;
        }

        private void Error()
        {
            Close();
            MessageBox.Show("An Error occured!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
