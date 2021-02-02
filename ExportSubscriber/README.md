## Export subscriber example documentation

Any data changes in EG Retail systems are published to a outgoing queue, and anyone with the proper credentials can listen to this queue.

The queue is implemented using Azure ServiceBus and Azure Storage (SB+Blob) for maximum efficiency of data transfer.
- The ServiceBus message notifies you of new data blob/file/batch.
- This message contains metadata including a link to the file in Storage.
- Storage contains the actual data, which could be many MB/GB.

This means that reading from the queue is a three-step process:

1. Connect to our TenantService to obtain temporary secrets used to connect to SB+Blob.
2. Listen to ServiceBus using these temporary secrets.
3. Once a new message arrives, download the blob from Storage and process data.
    - If the secrets expire, go back to 1.

This client is an **example** implementation of a job that listens for and processes messages.
It takes care of authenticating and refreshing secrets.

Once a message is received, it will filter it, parse it and print it.
If a configured folder exists, it will output the message metadata and each line of the data to that folder.

For further documentation, see https://docs.egretail.cloud/