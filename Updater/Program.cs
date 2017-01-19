using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Updater
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static void Main(string[] args)
        {
            ShowWindow(GetConsoleWindow(), SW_HIDE); //hide console

            bool canBeWriten = false;
            FileStream coreFile = null;
            while (!canBeWriten)
            {
                Thread.Sleep(200); //wait between tries
                try
                {
                    coreFile = File.OpenWrite("Core.dll"); //try to open core file
                }
                catch (Exception)
                {
                    continue; //if failed to open core file try again
                }
                coreFile.Close(); //if success close the opened file
                canBeWriten = true; //end loop
            }
            File.Replace("NewCore.dll", "Core.dll", "Core.dll.backup", true);
            File.Delete("NewCore.dll");

            Process.Start("Client.exe");
        }

        
    }
}
