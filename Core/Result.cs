using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    [Serializable]
    public class Result
    {
        public string TaskID { get; }
        public string ClientID { get; set; }
        public object Data { get; }

        public Result(string TaskID, object Data)
        {
            this.TaskID = TaskID;
            this.Data = Data;
        }
    }

    [Serializable]
    public class PingResult : Result
    {
        public PingResult(string TaskID) : base(TaskID, null) { }
    }

    [Serializable]
    public class UpgradeResult : Result
    {
        public UpgradeResult(string TaskID, object Data = null) : base(TaskID, Data) { }
    }

    [Serializable]
    public class EndConnectionResult : Result
    {
        public EndConnectionResult() : base(null, null) { }
    }
}
