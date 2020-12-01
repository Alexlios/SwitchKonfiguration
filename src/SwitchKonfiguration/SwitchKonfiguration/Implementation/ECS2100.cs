using SwitchKonfiguration.Interfaces;
using SwitchKonfiguration.Types;
using System;
using System.IO.Ports;
using System.Windows;

namespace SwitchKonfiguration.Implementation
{
    class ECS2100 : ISwitch
    {
        #region PrivateFields

        private string _hostname;
        private IPv4 _switchIP;
        private IPv4 _timeSrvIP;
        private IPv4 _srvIP;
        private IPv4 _tftpIP;
        private string _oldPwd;
        private string _newPwd;
        private string _comPort;
        private Connection _connection;
        private string _updateFile = "ECS2100_V1.2.2.9.bix";

        #endregion

        #region Constructors

        public ECS2100()
        {
            _hostname = string.Empty;
            _switchIP = new IPv4();
            _timeSrvIP = new IPv4();
            _srvIP = new IPv4();
            _tftpIP = new IPv4();
            _oldPwd = string.Empty;
            _newPwd = string.Empty;
            _comPort = string.Empty;
        }

        public ECS2100(string hostname, string switchIP, string timeSrvIP, string srvIP, string tftpIP, string oldPwd, string newPwd, string comPort)
        {
            _hostname = hostname;
            _switchIP = new IPv4(switchIP);
            _timeSrvIP = new IPv4(timeSrvIP);
            _srvIP = new IPv4(srvIP);
            _tftpIP = new IPv4(tftpIP);
            _oldPwd = oldPwd;
            _newPwd = newPwd;
            _comPort = comPort;
        }

        #endregion

        #region PublicMethods

        public bool Configure()
        {
            if (!string.IsNullOrEmpty(_hostname) && !string.IsNullOrEmpty(_oldPwd) && !string.IsNullOrEmpty(_newPwd) && !string.IsNullOrEmpty(_comPort))
            {
                if (_switchIP.checkValidation() && _timeSrvIP.checkValidation() && _srvIP.checkValidation() && _tftpIP.checkValidation())
                {
                    if (_connection.Connect(_comPort))
                    {
                        if (_connection.Login(_oldPwd))
                        {
                            if (_connection.Downgrade(_tftpIP, _updateFile))
                            {
                                if (_connection.Login(_oldPwd))
                                {
                                    if (_connection.FactoryReset())
                                    {
                                        if (_connection.Login(_oldPwd))
                                        {
                                            if (_connection.Configs(_switchIP, _hostname, _timeSrvIP, _srvIP, _newPwd))
                                            {

                                            }
                                            else
                                            {
                                                ShowError("Konfiguration fehlgeschlagen!");
                                            }
                                        }
                                        else
                                        {
                                            ShowError("Login fehlgeschlagen!");
                                        }
                                    }
                                    else
                                    {
                                        ShowError("Konnte nicht auf Werkeinstellungen zuücksetzen!");
                                    }
                                }
                                else
                                {
                                    ShowError("Login fehlgeschlagen!");
                                }
                            }
                            else
                            {
                                ShowError("Downgrade fehlgeschlagen!");
                            }
                        }
                        else
                        {
                            ShowError("Login fehlgeschlagen!");
                        }
                    }
                    else
                    {
                        ShowError($"Konnte keine Verbindung zu Port: {_comPort} herstellen!");
                    }

                }
                else
                {
                    ShowError("Eine der IP-Addressen ist ungueltig!");
                }
            }
            else
            {
                ShowError("Eines der Felder ist leer!");
            }




            return false;
        }

        #endregion

        #region PrivateMethods

        private void ShowError(string msg)
        {
            MessageBox.Show($"{msg}\nError Message:\n{_connection.TmpError}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion

    }
}
