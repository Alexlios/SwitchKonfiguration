using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitchKonfiguration.Types
{
    class Connection
    {
        private SerialPort _serialPort;
        private string _tmpError;
        private readonly int reloadWaitTime = 1000;
        private readonly int copyWaitTime = 1000;

        public string TmpError
        {
            get
            {
                return _tmpError;
            }
        }




        public bool Connect(string comPort)
        {
            try
            {
                _serialPort = new SerialPort(comPort);

                _serialPort.Close();

                _serialPort.BaudRate = 115200;
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.One;

                _serialPort.Open();

                if (!ExecuteCommand("")) return false;
            }
            catch (Exception ex)
            {
                _tmpError = ex.Message;
                return false; ;
            }
            return true;
        }

        public bool Login(string pwd)
        {
            if (!ExecuteCommand("admin")) return false;
            if (!ExecuteCommand(pwd)) return false;
            return true;
        }

        public bool ExecuteCommand(string command, int timeouts = 100)
        {
            try
            {
                _serialPort.WriteLine(command);
                return true;
            }
            catch (TimeoutException tex)
            {
                if (ExecuteCommand(command, timeouts - 1))
                {
                    return true;
                }
                else
                {
                    _tmpError = tex.Message;
                }
            }
            catch (Exception ex)
            {
                _tmpError = ex.Message;
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
            if (!ExecuteCommand("reload")) return false;
            if (!ExecuteCommand("y")) return false;

            if (!ExecuteCommand("", reloadWaitTime)) return false;

            return true;
        }

        public bool FactoryReset()
        {
            if (!ExecuteCommand("config")) return false;
            if (!ExecuteCommand("boot system config:Factory_Default_Config.cfg")) return false;
            if (!ExecuteCommand("end")) return false;
            if (!ExecuteCommand("delete file name startup1.cfg")) return false;
            if (!ExecuteCommand("reload")) return false;
            if (!ExecuteCommand("y")) return false;

            if (!ExecuteCommand("", reloadWaitTime)) return false;

            return true;
        }

        public bool Configs(IPv4 switchIP, string hostname, IPv4 timeSrvIP, IPv4 srvIP, string newPwd)
        {
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;
            if (!ExecuteCommand("")) return false;



            return false;
        }
    }
}
