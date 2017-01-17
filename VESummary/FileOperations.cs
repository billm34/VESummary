using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Win32;

namespace TestSummarizer
{
    public static class FileOperations
    {
        public static List<string> Load(string file)
        {
            List<string> content = new List<string>();
            string line;
            int i = 0;

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
                Console.WriteLine("The file could not be read:" + e.Message);
                return(null);
            }
        }

        public static bool Save(string file, List<string> content)
        {
            FileStream fs = new FileStream(file,FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
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
                Console.WriteLine("Unable to save the file: " + e.Message);
                return (false);
            }
            
            return (true);
        }

        // BackupFile: Backs up a file to the same folder with a ".orig" extension
        public static bool Backup(string path)
        {
            string backupPath = Path.GetDirectoryName(path) +"\\" + Path.GetFileNameWithoutExtension(path) + ".orig";
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

            if (key != null)
            {
                o = key.GetValue(identifier);
                if (o != null)
                {
                    return(o.ToString());
                }
                else
                {
                    Console.WriteLine("Couldn't find file.");
                    ;
                }
            }
            else
            {
                Console.WriteLine("Couldn't find file information registry key.");
            }
            return(null);
        }
    }
}
