﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Workflow
{
    class AzureStorageQueue : IWorkflowQueue
    {
        CloudQueue _queue;

        public AzureStorageQueue(CloudQueue queue)
        {
            _queue = queue;
        }

        public WorkflowMessage Dequeue()
        {
            TimeSpan visibility = TimeSpan.FromSeconds(5);

            var queueMessage = _queue.GetMessageAsync(visibility, null, null).Result;

            if (queueMessage == null)
                return null;

            return WorkflowMessage.CreateFrom(this, queueMessage.Id, queueMessage.AsString, queueMessage.PopReceipt);
        }

        public void Enqueue(WorkflowMessage message)
        {
            var queueMessage = new CloudQueueMessage(message.GetContent());
            _queue.AddMessageAsync(queueMessage).Wait();
        }

        public void UpdateTimeout(WorkflowMessage message, int timeout)
        {
            var cloudMessage = new CloudQueueMessage(message.Id, message.ReceiptId);
            var visibilityTimeout = TimeSpan.FromSeconds(timeout);

            _queue.UpdateMessageAsync(cloudMessage, visibilityTimeout, MessageUpdateFields.Visibility).Wait();
        }

        public void Complete(WorkflowMessage message)
        {
            _queue.DeleteMessageAsync(message.Id, message.ReceiptId).Wait();
        }
    }
}
