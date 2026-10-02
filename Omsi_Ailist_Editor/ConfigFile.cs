using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omsi_Ailist_Editor
{
    internal class ConfigFile
    {
        private static string fileName = System.IO.Directory.GetCurrentDirectory() + "\\config.ini";
        public static bool autoBackup = false;
        
        public static bool initConfig()
        {
            try
            {
                if (!System.IO.File.Exists(fileName))
                {
                    System.IO.File.Create(fileName).Close();

                    using (StreamWriter sw = new StreamWriter(fileName))
                    {
                        sw.Write("Backup=true");
                        sw.Close();
                    }
                }

                using (StreamReader sr = new StreamReader(fileName))
                {
                    for (string line = sr.ReadLine(); line != null; line = sr.ReadLine())
                    {
                        switch (line)
                        {
                            case "Backup=true":
                                autoBackup = true;
                                break;
                            case "Backup=false":
                                autoBackup = false;
                                break;
                        }
                        
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            
        }
    }
}
