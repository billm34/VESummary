using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace TestSummarizer
{
    public static class FileOperations
    {
        private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static List<string> Load(string file)
        {
            List<string> content = new List<string>();
            string line;
            int i = 0;

            log4net.Config.XmlConfigurator.Configure();

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(file))
                {
                    for (i = 0; (line = sr.ReadLine()) != null; i++)
                    {
                        content.Add(line);
                    }

                    sr.Close();
                }
                return(content);
            }
            catch (Exception e)
            {
                log.Error("The file could not be read:" + e.Message);
                return(null);
            }
        }

        public static bool Save(string file, List<string> content)
        {
            FileStream fs = new FileStream(file,FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);

            log4net.Config.XmlConfigurator.Configure();

            try
            {
                foreach (string line in content)
                {
                    sw.WriteLine(line);
                }

                sw.Close();
            }
            catch(Exception e)
            {
                log.Error("Unable to save the file: " + e.Message);
                return (false);
            }
            
            return (true);
        }

        // BackupFile: Backs up a file to the same folder with a ".orig" extension
        public static bool Backup(string path)
        {
            string backupPath = Path.GetDirectoryName(path) +"\\" + Path.GetFileNameWithoutExtension(path) + ".orig";

            log4net.Config.XmlConfigurator.Configure();

            try
            {
                File.Copy(path, backupPath, true);
            }
            catch
            {
                return (false);
            }
            return (true);
        }

        public static void Delete(string path)
        {
            File.Delete(path);
        }

        public static string GetFileName(string identifier)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\willy", false);
            Object o;

            log4net.Config.XmlConfigurator.Configure();

            if (key != null)
            {
                o = key.GetValue(identifier);
                if (o != null)
                {
                    return(o.ToString());
                }
                else
                {
                    log.Error("Couldn't find file.");
                    ;
                }
            }
            else
            {
                log.Error("Couldn't find file information registry key.");
            }
            return(null);
        }
    }
}
