using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Workflow
{
    class AzureStorage
    {
        CloudStorageAccount _account;
        CloudQueueClient _queueClient;

        public AzureStorage() : this(CloudStorageAccount.DevelopmentStorageAccount)
        {
        }

        public AzureStorage(string connectionString) : this(CloudStorageAccount.Parse(connectionString))
        {            
        }

        public AzureStorage(CloudStorageAccount account)
        {
            _queueClient = account.CreateCloudQueueClient();
        }

        public AzureStorageQueue CreateQueue(string queueName)
        {
            var queue = _queueClient.GetQueueReference(queueName);

            queue.CreateIfNotExistsAsync().Wait();

            return new AzureStorageQueue(queue);
        }
    }
}
