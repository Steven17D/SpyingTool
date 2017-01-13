using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //Load configuration from config file
            Helper.Configurations config = new Helper.Configurations(
                Helper.GetConfigurationByKey("ClientID").ToString(),
                IPAddress.Parse(Helper.GetConfigurationByKey("ServerIP").ToString()),
                int.Parse(Helper.GetConfigurationByKey("ServerPort").ToString()));

            //start connection with the server
            if (Helper.Connect(config)) //if connected successfully
            {
                Helper.AcceptCommands();
            }
            else
            {
                //restart the program to try again
                System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }
    }
}
