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

    public class Helper
    {
        static Socket serverSocket;

        public struct Configurations
        {
            public string ClientID; //need to set just a getter
            public IPAddress ServerIp;
            public int ServerPort;

            public Configurations(string ClientID, IPAddress ServerIp, int ServerPort)
            {
                this.ClientID = ClientID;
                this.ServerIp = ServerIp;
                this.ServerPort = ServerPort;

                Console.WriteLine("Configuration loaded:\n" + this.ToString());
            }

            public override string ToString()
            {
                return String.Format(
                    "Client ID: {0}\n" +
                    "Server IP: {1}\n" +
                    "Server Port: {2}\n",
                    ClientID, ServerIp.ToString(), ServerPort);
            }
        }

        public static object GetConfigurationByKey(string key)
        {
            #pragma warning disable CS0618 // ConfigurationSettings was deprecated
            return ConfigurationSettings.AppSettings[key];
            #pragma warning restore CS0618
        }

        public static bool Connect(Configurations config)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Connect(config.ServerIp, config.ServerPort);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            finally
            {
                Console.WriteLine("Connection ended successfully");
            }
            return true;
        }

        internal static void AcceptCommands()
        {
            
        }
    }
}
