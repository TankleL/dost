using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Diagnostics;

namespace regt
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args == null || args.Length <=0)
            {
                Console.Out.WriteLine("Error: wrong arguments");
                return;
            }

            if(args.Length == 3 && args[0] == "-a")
            {
                AddContextMenu("使用Docstore管理仓库", args[1] + " " + args[2]);
            }
            else if(args.Length == 1 && args[0] == "-r")
            {
                DelContextMenu("使用Docstore管理仓库");
            }
            else
            {
                Console.Out.WriteLine("Warning: wrong arguments");
            }
        }

        private static void AddContextMenu(string itemName, string assoCreatedProgramFullPath)
        {
            try
            {
                RegistryKey shellKey = Registry.ClassesRoot.OpenSubKey("directory", true).OpenSubKey("shell", true);
                if (shellKey == null)
                {
                    shellKey = Registry.ClassesRoot.CreateSubKey(@"*\shell");
                }

                RegistryKey rightCommondKey = shellKey.OpenSubKey(itemName);
                if (rightCommondKey == null)
                {
                    rightCommondKey = shellKey.CreateSubKey(itemName);
                }
                RegistryKey assoCreatedProgramKey = rightCommondKey.CreateSubKey("command");
                assoCreatedProgramKey.SetValue(string.Empty, assoCreatedProgramFullPath);
                assoCreatedProgramKey.Close();
                rightCommondKey.Close();
                shellKey.Close();
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
        }

        private static void DelContextMenu(string itemName)
        {
            try
            {
                RegistryKey shellKey = Registry.ClassesRoot.OpenSubKey("directory", true).OpenSubKey("shell", true);

                RegistryKey rightCommondKey = shellKey.OpenSubKey(itemName, true);
                if (rightCommondKey != null)
                {
                    rightCommondKey.DeleteSubKeyTree("");
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
        }
    }
}
