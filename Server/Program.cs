using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //start the listening for client
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"),8989);
            listener.Start();
            Console.Write("Waiting for a connection...\n");

            // Perform a blocking call to accept requests.
            // You could also user server.AcceptSocket() here.
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Connected!");
        }
    }
}
