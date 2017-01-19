using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Updater
{
    class Program
    {
        static void Main(string[] args)
        {
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
