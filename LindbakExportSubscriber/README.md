This client is an **example** implementation of a job that connects to Lindbak systems and listens for messages.
It takes care of authenticating and refreshing tokens.

Once a message is receieved, it will filter it and parse it and finally print it.
If a certain folder exists, it will ouput the message and batch to that folder.