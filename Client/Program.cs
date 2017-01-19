using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Client
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main(string[] args)
        {
            //hide console
            //ShowWindow(GetConsoleWindow(), 0);

            //start connection with the server
            Logic.Start();
        }
    }
}
