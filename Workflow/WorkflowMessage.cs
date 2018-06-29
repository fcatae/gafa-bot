using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Workflow
{
    class WorkflowMessage
    {
        public readonly string Id;
        public readonly string Module;
        public readonly string Method;
        public readonly object Parameter;
        public readonly string ReceiptId;

        protected WorkflowMessage(string id, string module, string method, object parameter, string receiptId)
        {
            this.Id = id;
            this.Module = module;
            this.Method = method;
            this.Parameter = parameter;
            this.ReceiptId = receiptId;
        }

        public static WorkflowMessage CreateCall(string module, string method, object parameter)
        {
            return new WorkflowMessage(null, module, method, parameter, null);
        }

        public static WorkflowMessage CreateFrom(string id, string body, string receipt)
        {
            string[] comp1 = body.Split(':', 2);
            string[] comp2 = comp1[1].Split('(', 2);

            string module = comp1[0].Trim();
            string method = comp2[0].Trim();
            string jsonParameter = comp2[1].Trim(')', ' ', '\t', '\r', '\n');
            var parameter = JsonConvert.DeserializeObject(jsonParameter);

            return new WorkflowMessage(id, module, method, parameter, receipt);
        }

        public string GetContent()
        {
            string jsonParameter = JsonConvert.SerializeObject(Parameter);

            return $"{Module}:{Method}({jsonParameter})";
        }
    }
}
