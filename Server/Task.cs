using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
#pragma warning disable IDE1006 // Naming Styles
    public class Task
    {
        public Task()
        {
            _id = string.Empty;
            clientID = string.Empty;
            command = new CommandInfo(string.Empty, string.Empty);
            result = new ResultInfo(string.Empty, new string[0]);
        }

        [MongoDB.Bson.Serialization.Attributes.BsonId]
        public string _id { get; set; }
        public string clientID { get; set; }
        public CommandInfo command { get; set; }
        public ResultInfo result { get; set; }
    }

    public class CommandInfo
    {
        public CommandInfo(string commandType, string executionArgumen)
        {
            this.commandType = commandType;
            this.executionArgument = executionArgument;
        }
        public string commandType { get; set; }
        public string executionArgument { get; set; }
    }

    public class ResultInfo
    {
        public ResultInfo(string resultType, string[] resultData)
        {
            this.resultType = resultType;
            this.resultData = resultData;
        }

        public string resultType { get; set; }
        public string[] resultData { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}
