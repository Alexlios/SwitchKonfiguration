using SwitchKonfiguration.Types;
using System;
using System.IO.Ports;
using System.Threading;

namespace SwitchKonfiguration.Implementation
{
    class Commander
    {
        #region PrivateFields

        private string _tmpError;
        private SerialPort _output;

        #endregion

        #region PublicFields

        public bool canSend;

        #endregion

        #region Properties

        public string TmpError
        {
            get
            {
                return _tmpError;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a Commander object
        /// </summary>
        /// <param name="output">The SerialPort the Commander will write to</param>
        public Commander(SerialPort output)
        {
            canSend = true;
            _output = output;
        }

        #endregion

        #region PublicMethods

        /// <summary>
        /// Downgrades the firmware of the switch
        /// </summary>
        /// <param name="tftpIP">The IP of the TFTP-Server it will load the firmware file from</param>
        /// <param name="updateFile">The name of the firmware file</param>
        /// <returns>if it succeeded (true) or not (false)</returns>
        public bool Downgrade(IPv4 tftpIP, string updateFile)
        {
            //commands to download the file
            if (!ExecuteCommand("copy tftp file")) return false;
            if (!ExecuteCommand(tftpIP.ToString())) return false;
            if (!ExecuteCommand("2")) return false;
            if (!ExecuteCommand(updateFile)) return false;
            if (!ExecuteCommand(updateFile)) return false;

            //commands to install the file
            if (!ExecuteCommand("config", 600)) return false;
            if (!ExecuteCommand("boot system opcode: " + updateFile)) return false;
            if (!ExecuteCommand("end")) return false;

            return true;
        }

        /// <summary>
        /// Resets the Switch to its factory default configuration
        /// </summary>
        /// <returns>if it succeeded (true) or not (false)</returns>
        public bool FactoryReset()
        {
            //commands to reset the switch
            if (!ExecuteCommand("config")) return false;
            if (!ExecuteCommand("boot system config:Factory_Default_Config.cfg")) return false;
            if (!ExecuteCommand("end")) return false;
            if (!ExecuteCommand("delete file name startup1.cfg")) return false;

            return true;

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
            //many configuration commands
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

        /// <summary>
        /// Executes a command on the Switch via the given output Port
        /// </summary>
        /// <param name="command">the command that will be executed</param>
        /// <returns>if it succeeded (true) or not (false)</returns>
        public bool ExecuteCommand(string command)
        {
            //runs the proper method with 30 timeouts (=> ca 30 seconds)
            return ExecuteCommand(command, 30);
        }

        /// <summary>
        /// Executes a command on the Switch via the given output Port
        /// </summary>
        /// <param name="command">the command that will be executed</param>
        /// <param name="timeouts">the amount of timeouts allowed (roughly 1 timeout = 1 second</param>
        /// <returns>if it succeeded (true) or not (false)</returns>
        public bool ExecuteCommand(string command, int timeouts)
        {
            try
            {
                Thread.Sleep(1000);
                if (canSend)
                {
                    _output.WriteLine(command);
                    canSend = false;
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
                _tmpError = ex.Message + "\n" + canSend;
            }
            return false;
        }

        #endregion
    }
}
