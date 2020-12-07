using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SwitchKonfiguration.Implementation
{
    public class ExcelConverter
    {
        public Dictionary<string, List<string>> Table;



        public bool Convert(string filePath)
        {
            if (!filePath.EndsWith(".csv"))
            {
                MessageBox.Show("Datei muss eine .csv Datei sein!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            string content = File.ReadAllText(filePath);

            Table = new Dictionary<string, List<string>>();

            int rowCounter = 0;
            int columnCounter = 0;

            foreach (string row in content.Split('\n'))
            {
                foreach (string cell in row.Split(';'))
                {
                    if (rowCounter == 0)
                    {
                        Table.Add(cell, new List<string>());
                    }
                    else
                    {
                        try
                        {
                            switch (columnCounter)
                            {
                                case 0:
                                    Table["hostname"].Add(cell);
                                    break;

                                case 1:
                                    Table["switchIP"].Add(cell);
                                    break;

                                case 2:
                                    Table["zeitserverIP"].Add(cell);
                                    break;

                                case 3:
                                    Table["serverIP"].Add(cell);
                                    break;

                                case 4:
                                    Table["Passwort"].Add(cell);
                                    break;

                                case 5:
                                    Table["NeuesPasswort"].Add(cell);
                                    break;

                                default:
                                    MessageBox.Show("Datei enthält zu viele Spalten!", "Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                    return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Datei enthält fehlerhafte Spalten!\n\n" + ex.Message, "Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                        ++columnCounter;
                    }
                }
                ++rowCounter;
            }

            return true;
        }
    }
}
