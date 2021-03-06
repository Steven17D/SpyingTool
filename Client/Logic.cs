﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.IO;
using Core;
using System.Threading;

namespace Client
{
    class Logic
    {
        static TcpClient serverSocket;
        static NetworkStream stream;
        static byte[] buffer;
        static ManualResetEvent connectionDone;

        internal class Configurations
        {
            public string ClientID { get; private set; }
            public IPAddress ServerIp { get; private set; }
            public int ServerPort { get; private set; }

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

            internal static string GetConfigurationByKey(string key)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                return ConfigurationSettings.AppSettings[key].ToString();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        internal static void ForceExit()
        {
            if (serverSocket.Connected)
            {
                SendResult(new EndConnectionResult(null));
                CloseClient();
            }
        }

        public static void Start()
        {
            //Signal to close the program
            connectionDone = new ManualResetEvent(false);

            //Load configuration from config file
            Configurations config = new Configurations(
                Configurations.GetConfigurationByKey("ClientID"),
                IPAddress.Parse(Configurations.GetConfigurationByKey("ServerIP")),
                int.Parse(Configurations.GetConfigurationByKey("ServerPort")));

            if (Connect(config)) //if connected successfully
            {
                AcceptCommands();
                connectionDone.WaitOne();
                System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            else
            {
                //restart the program to try again
                Thread.Sleep(3000);
                System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            Environment.Exit(0);
        }

        internal static bool Connect(Configurations config)
        {
            try
            {
                serverSocket = new TcpClient(AddressFamily.InterNetwork);
                serverSocket.Connect(config.ServerIp, config.ServerPort);
                buffer = new byte[serverSocket.ReceiveBufferSize];
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to connect to server: \n" + e.Message);
                return false;
            }
            return true;
        }

        internal static void AcceptCommands()
        {
            try
            {
                stream = serverSocket.GetStream();
                stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, stream);
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptCommand failed: ", e.Message);
                CloseClient(); //Close client because cant get commands from server
            }
        }

        private static void CloseClient()
        {
            if (serverSocket.Connected) serverSocket.Close(); //Close the socket
            connectionDone.Set(); //Signal to exit
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            Command command = null;
            try
            {
                int received = stream.EndRead(ar);
                if (received == 0) { return; }
                // Continue waiting for command from server
                stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, stream);
                byte[] receiveBuffer = new byte[received];
                Buffer.BlockCopy(buffer, 0, receiveBuffer, 0, received);
                // The received data is deserialized
                command = Serializer.Deserialize(receiveBuffer) as Command;
                // Send back the result as serialized Result
                SendResult(command.Execute());
            }
            catch(SafeCloseException e)
            {
                SendResult(new EndConnectionResult(e.Message)); //Send result to the server about end of connection
                CloseClient();
            }
            catch (Exception e)
            {
                Console.WriteLine("ReadCallback failed: {0}", e.Message);
                CloseClient();
            }
        }

        private static void WriteCallback(IAsyncResult ar)
        {
            try
            {
                stream.EndWrite(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine("WriteCallback failed: {0}",e.Message);
            }
        }

        private static void SendResult(Result result)
        {
            try
            {
                result.ClientID = Configurations.GetConfigurationByKey("ClientID") as string;
                byte[] resultBuffer = Serializer.Serialize(result);
                stream.BeginWrite(resultBuffer, 0, resultBuffer.Length, WriteCallback, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("SendResult failed: {0}",e.Message);
            }
        }
    }
}
