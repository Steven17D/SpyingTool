using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Core;

namespace Server
{
    class Database
    {
        internal static void ParseResult(Result result, Socket clientSocket)
        {
            // If there is no data, it is a Ping result.
            if (result is PingResult)
            {
                Network.Clients.Add(result.ClientID, clientSocket);
                if (Interpreter.inClientSelection)
                {
                    Console.Clear();
                    Interpreter.PrintClients();
                    Console.WriteLine("Enter a Client: ");
                }
                return;
            }
            else if (result is EndConnectionResult)
            {
                //add to database by taskID
                string id = result.TaskID;
                Network.Clients.Remove(result.ClientID);
                Console.WriteLine("Client {0} disconnected",result.ClientID);
                return;
            }

            Console.WriteLine("Got result from client {0}", result.ClientID);

            if (result.Data is List<string>)
            {
                foreach (string item in result.Data as List<string>)
                {
                    Console.WriteLine(item);
                }
            }
            else if (result.Data is byte[])
            {
                Console.WriteLine("Got file {0} bytes", (result.Data as byte[]).Length);
            }
            else if (result.Data is string)
            {
                Console.WriteLine(result.Data);
            }
        }

        internal static string CreateTask()
        {
            return new Random().Next().ToString();
        }
    }
}
