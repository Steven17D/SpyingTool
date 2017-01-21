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
    partial class Program
    {
        static void Main(string[] args)
        {
            //hide console
            //ShowWindow(GetConsoleWindow(), 0);


            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            //start connection with the server
            Logic.Start();
        }

        
    }
}
