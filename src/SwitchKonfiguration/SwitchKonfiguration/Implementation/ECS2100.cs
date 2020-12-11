using SwitchKonfiguration.Types;
using System.Windows;

namespace SwitchKonfiguration.Implementation
{
    class ECS2100
    {
        #region PublicMethods

        public static Connection connection;


        /// <summary>
        /// Configures a ECS2100 Mini-Switch
        /// </summary>
        /// <param name="hostname">The hostname the Switch will have</param>
        /// <param name="switchIP">The IP the Switch will have</param>
        /// <param name="timeSrvIP">The IP of the timeserver from which the Switch pulls its time</param>
        /// <param name="srvIP">The IP of the Server it will be connected to</param>
        /// <param name="tftpIP">The IP of the TFTP-Server it will load the firmware file from</param>
        /// <param name="oldPwd">The current password of the Switch</param>
        /// <param name="newPwd">The new password that will be given</param>
        /// <param name="comPort">The Port on which the Switch is currently connected</param>
        /// <returns></returns>
        public static bool Configure(string hostname, IPv4 switchIP, IPv4 timeSrvIP, IPv4 srvIP, IPv4 tftpIP, string oldPwd, string newPwd, string comPort)
        {
            connection = new Connection();

            //checking if all fields are valid in theory
            if (!string.IsNullOrEmpty(hostname) && !string.IsNullOrEmpty(oldPwd) && !string.IsNullOrEmpty(newPwd) && !string.IsNullOrEmpty(comPort))
            {
                if (switchIP.checkValidation() && timeSrvIP.checkValidation() && srvIP.checkValidation() && tftpIP.checkValidation())
                {
                    //beginning configuration, connecting
                    if (connection.Connect(comPort))
                    {
                        //logging in
                        if (connection.IsConnected && connection.Login(oldPwd))
                        {
                            //resetting the switch to factory defaults
                            if (connection.IsConnected && connection.FactoryReset())
                            {
                                //logging in after reboot
                            //    if (connection.IsConnected && connection.Login(oldPwd))
                            //    {
                                    //downgrading the firmware
                            //        if (connection.IsConnected && connection.Downgrade(tftpIP, "ECS2100_V1.2.2.9.bix"))
                            //        {
                                        //logging in after reboot
                                        if (connection.IsConnected && connection.Login(oldPwd))
                                        {
                                            //configuring the switch
                                            if (connection.IsConnected && connection.Configs(switchIP, hostname, timeSrvIP, srvIP, newPwd))
                                            {
                                                MessageBox.Show("Konfiguration abgeschlossen. Bitte diese nachprüfen!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                                                connection.Close();
                                                return true;
                                            }
                                            else
                                            {
                                                ShowError("Konfiguration fehlgeschlagen!", connection);
                                            }
                                        }
                                        else
                                        {
                                            ShowError("Login-Konfig fehlgeschlagen!", connection);
                                        }
                            //        }
                            //        else
                            //        {
                            //            ShowError("Downgrade fehlgeschlagen!", connection);
                            //        }
                            //    }
                            //    else
                            //    {
                            //        ShowError("Login-Factory fehlgeschlagen!", connection);
                            //    }
                            }
                            else
                            {
                                ShowError("Konnte nicht auf Werkeinstellungen zuücksetzen!", connection);
                            }

                        }
                        else
                        {
                            ShowError("Login-Down fehlgeschlagen!", connection);
                        }
                    }
                    else
                    {
                        ShowError($"Konnte keine Verbindung zu Port: {comPort} herstellen!", connection);
                    }

                }
                else
                {
                    ShowError("Eine der IP-Addressen ist ungueltig!", connection);
                }
            }
            else
            {
                ShowError("Eines der Felder ist leer!", connection);
            }

            return false;
        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// Shows a Error-MessageBox
        /// </summary>
        /// <param name="msg">The Error message</param>
        /// <param name="connection">The port on which it occured</param>
        private static void ShowError(string msg, Connection connection)
        {
            //if the port has a error message print it too, else not
            if (connection != null && !string.IsNullOrEmpty(connection.TmpError))
            {
                MessageBox.Show($"{msg}\nError Message:\n{connection.TmpError}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show($"{msg}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            connection.Close();
        }

        #endregion

    }
}
