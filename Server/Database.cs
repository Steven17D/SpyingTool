using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using MongoDB.Bson;
using MongoDB.Driver;
using Core;

namespace Server
{
    class Database
    {
        private static IMongoClient mongoClient;
        private static IMongoDatabase database;
        private static IMongoCollection<Task> collection;

        internal static void Start()
        {
            mongoClient = new MongoClient();
            database = mongoClient.GetDatabase("local");
            collection = database.GetCollection<Task>("Tasks");
        }

        internal static void SaveCommand(string clientID, Command command)
        {
            string _id = command.TaskID;
            string commandClientID = clientID;
            CommandInfo commandInfo = new CommandInfo(command.GetType().ToString(), command.executionArgument);

            collection.FindOneAndUpdateAsync<Task>(Builders<Task>.Filter.Eq("_id", _id),
                Builders<Task>.Update.Set(
                    "clientID", clientID)); //insert client id to task
            collection.FindOneAndUpdateAsync<Task>(Builders<Task>.Filter.Eq("_id", _id),
                Builders<Task>.Update.Set(
                    "command", commandInfo)); //insert command info to task
        }

        internal static string CreateTask()
        {
            Task newTask = new Task();
            collection.InsertOne(newTask);
            return newTask._id;
        }

        internal static void SaveResult(Result result, Socket clientSocket)
        {
            //Ping result adds the client to clients list
            if (result is PingResult)
            {
                HandlePingResult(clientSocket, result);
                return;
            }
            
            //Save the result to database by task id
            string _id = result.TaskID;

            if (result is EndConnectionResult)
            {
                if (_id != null)
                {
                    collection.FindOneAndUpdateAsync(Builders<Task>.Filter.Eq("_id", _id),
                        Builders<Task>.Update.Set(
                            "result", new ResultInfo(result.GetType().ToString(), new string[0]))); //save result
                }
                
                Network.Clients.Remove(result.ClientID);
                Interpreter.RefreshClients();
                return;
            }
            else
            {
                string[] data = DataConverter(result.Data);
                collection.FindOneAndUpdateAsync(Builders<Task>.Filter.Eq("_id", _id),
                Builders<Task>.Update.Set(
                    "result", new ResultInfo(result.GetType().ToString(), data))); //save result
            }
        }

        private static string[] DataConverter(object data) //conver data to string array
        {
            if (data is byte[])
            {
                return new[] { Convert.ToBase64String(data as byte[]) };
            }
            else if (data is List<string>)
            {
                return (data as List<string>).ToArray();
            }
            else if (data is string)
            {
                return new[] { data as string };
            }
            else { return new string[0]; }
        }

        private static void HandlePingResult(Socket clientSocket, Result result)
        {
            Network.Clients.Add(result.ClientID, clientSocket); //add client to list
            Interpreter.RefreshClients();
        }
    }
}
