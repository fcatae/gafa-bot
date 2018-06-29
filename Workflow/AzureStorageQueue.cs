using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Workflow
{
    class AzureStorageQueue : IWorkflowQueue
    {
        CloudQueueClient _client;
        CloudQueue _queue;

        public AzureStorageQueue()
        {
            _client = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudQueueClient();
            _queue = _client.GetQueueReference("workflow");
            _queue.CreateIfNotExistsAsync().Wait();
        }

        public WorkflowMessage Dequeue()
        {
            var queueMessage = _queue.GetMessageAsync().Result;

            if (queueMessage == null)
                return null;

            return WorkflowMessage.CreateFrom(queueMessage.Id, queueMessage.AsString, queueMessage.PopReceipt);
        }

        public void Enqueue(WorkflowMessage message)
        {
            var queueMessage = new CloudQueueMessage(message.GetContent());
            _queue.AddMessageAsync(queueMessage).Wait();
        }

        public void Complete(WorkflowMessage message)
        {
            _queue.DeleteMessageAsync(message.Id, message.ReceiptId).Wait();
        }
    }
}
