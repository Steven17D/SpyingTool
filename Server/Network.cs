using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Core;

namespace Server
{
    class Network
    {
        public static Dictionary<string, Socket> Clients = new Dictionary<string, Socket>();
        private const int MaxClients = 20;

        static Socket serverSocket;
        static byte[] buffer;

        internal static void Start()
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                buffer = new byte[65536];
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, 8989));
                serverSocket.Listen(0);
                serverSocket.BeginAccept(AcceptCallback, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("Start failed: {0}",e.Message);
            }
        }

        internal static void CloseAllSockets()
        {
            CloseClients();
            serverSocket.Close();
        }

        private static void CloseClients()
        {
            foreach (KeyValuePair<string,Socket> client in Clients)
            {
                client.Value.Close();
                Console.WriteLine("Connection with {0} closed", client.Key);
            }
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = serverSocket.EndAccept(ar);

                //Continue accepting clients
                if (Clients.Count < MaxClients) serverSocket.BeginAccept(AcceptCallback, null);

                //Send ping to get ClientID
                //Recieve result containing ClientID
                //On recieve of Ping result the client is added to database
                SendRecieve(clientSocket, new PingCommand(Database.CreateTask()));

            }
            catch (SocketException e)
            {
                Console.WriteLine("Socket failed to accept client: {0}",e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to accept client: {0}", e.Message);
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = ar.AsyncState as Socket;
                socket.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine("Send faild: ",e.Message);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            Socket clientSocket = null;
            try
            {   clientSocket = ar.AsyncState as Socket;
                int recieved = clientSocket.EndReceive(ar);
                if (recieved == 0) return;
                byte[] recieveBuffer = new byte[recieved];
                Buffer.BlockCopy(buffer, 0, recieveBuffer, 0, recieved);
                Result result = Serializer.Deserialize(recieveBuffer) as Result;
                Database.SaveResult(result, clientSocket);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Socket to recieve failed", e.Message);
                //release socket
                Clients.Remove(Clients.FirstOrDefault(x => x.Value == clientSocket).Key);
            }
            catch (Exception e)
            {
                Console.WriteLine("Receice failed: {0}",e.Message);
            }
        }

        internal static void SendRecieve(Socket clientSocket, Command command)
        {
            try
            {
                byte[] sendBuffer = Serializer.Serialize(command);
                clientSocket.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None,
                    new AsyncCallback(SendCallback), clientSocket);
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), clientSocket);
            }
            catch (Exception e)
            {
                string clientID = Clients.FirstOrDefault(x => x.Value == clientSocket).Key;
                Console.WriteLine("Client {0} is disconnected.\n" + e.Message, clientID);
                Clients.Remove(clientID);
            }
        }
    }
}
