using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvatarGameServer
{
    class GameServer
    {
        public List<GameClient> clientList;
        public List<Thread> threadList;

        private TcpListener tcpListener;
        private Thread listenThread;
        private bool mainThreadWorking;

        private static readonly object padlock = new object();
        private static GameServer instance = null;

        private int clientConnectCounter = 0;

        public static GameServer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new GameServer();
                        }
                    }
                }

                return instance;
            }
        }

        private GameServer()
        {
            // Threads storage
            this.mainThreadWorking = true;
            this.clientList = new List<GameClient>();
            this.threadList = new List<Thread>();
            this.tcpListener = new TcpListener(IPAddress.Any, 8888);
            Console.WriteLine("Game server set and ready at port 8888");
            this.listenThread = new Thread(ListenForClients);
        }

        public void startServer()
        {
            this.listenThread.Start();
        }

        public void stopServer()
        {
            this.mainThreadWorking = false;
            for (int i = 0; i < clientList.Count; i++)
            {
                GameClient temp = (GameClient) clientList[i];
                temp.forceClose();
            }

            this.tcpListener.Stop();
            for (int i = 0; i < threadList.Count; i++)
            {
                Thread tempThread = threadList[i];
                if (tempThread.IsAlive)
                {
                    Console.WriteLine("Margin thread " + i + " is alive, closing it");
                    tempThread.Abort();
                }
            }

            this.listenThread.Abort();
        }


        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (mainThreadWorking)
            {
                // Create a new object when a client arrives
                TcpClient client = this.tcpListener.AcceptTcpClient();
                string playerName = "Guest" + (clientConnectCounter++).ToString();
                GameClient gameClient = new GameClient(client.GetHashCode(), playerName, 0, 0, 0, 0);
                Console.WriteLine("Client " + playerName + " connected");

                // Define a new thread with the handling method as main loop and start it
                Thread clientThread = new Thread(new ParameterizedThreadStart(gameClient.HandleClientComm));
                threadList.Add(clientThread);

                clientThread.Start(client);

                gameClient.clientThreadId = clientThread.ManagedThreadId;
                // Add it to the clients list
                clientList.Add(gameClient);
            }
        }

        public void SendPacketToAllClients(byte[] packet)
        {
            foreach (GameClient client in clientList)
            {
                client.SendPaket(packet);
            }
        }

        public void SendPacketToAllOtherClients(byte[] packet, int hashId)
        {
            foreach (GameClient client in clientList)
            {
                if (!client.hashId.Equals(hashId))
                {
                    client.SendPaket(packet);
                }
            }
        }

        private void RemoveThreadFromThreadList(int ThreadId)
        {
            for (int i = 0; i < threadList.Count; i++)
            {
                Thread tempThread = (Thread) threadList[i];
                if (tempThread.ManagedThreadId == ThreadId)
                {
                    tempThread.Abort();
                }
            }
        }
    }
}