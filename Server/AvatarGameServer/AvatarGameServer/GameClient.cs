using AvatarGameServer.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AvatarGameServer
{
    public class GameClient
    {
        public int hashId;
        private bool working;
        public string playerName = "Guest000";
        public float xPos, yPos, zPos, yRot;
        public int clientThreadId;

        private TcpClient tcpClient;
        private NetworkStream clientStream;

        public GameClient(int hashId, string playerName, float xPos, float yPos, float zPos, float yRot)
        {
            this.hashId = hashId;
            this.playerName = playerName;
            this.xPos = xPos;
            this.yPos = yPos;
            this.zPos = zPos;
            this.yRot = yRot;

            working = true;
        }

        public void forceClose()
        {
            this.tcpClient.Close();
            this.clientStream.Close();
        }

        public void HandleClientComm(object client)
        {
            tcpClient = (TcpClient)client;
            clientStream = tcpClient.GetStream();

            // Define an auth server processor per thread

            byte[] message = new byte[2048];
            int bytesRead;

            Console.WriteLine("Client Connected.");
            SendPlayerSpawn();
            SendOtherPlayerSpawns();


            // Receive TCP auth packets from the connected client.
            while (working)
            {
                bytesRead = 0;

                try
                {
                    bytesRead = clientStream.Read(message, 0, 2048);
                }
                catch { break; }

                if (bytesRead == 0)
                {
                    break;
                }

                // Parse the received packet data
                try
                {

                    byte[] packet = new byte[bytesRead];
                    ArrayUtils.copy(message, 0, packet, 0, bytesRead);
                    packetHandler(packet, clientStream);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    break;
                }

            }
            working = false;
            Console.WriteLine("Client Disconnected");
            SendPlayerDeconstructed();
            tcpClient.Close();
            clientStream.Close();
        }

        public void packetHandler(byte[] packet, NetworkStream client)
        {
            Console.WriteLine("Handle Packet " + StringUtils.bytesToString(packet));
            byte opcode = packet[0];

            PacketReader pak = new PacketReader(packet);
            pak.incrementOffsetByValue(1);

            switch (opcode)
            {
                case 0x01:
                    // Get PlayerName Request
                    Console.WriteLine("[HANDLE] GetPlayerNameRequest");
                    GetPlayerNameRequest(packet);
                    break;
                case 0x02:
                    break;

                case 0x04:
                    // Move Request
                    GetMovementRequest(packet);
                    break;
                case 0x09:
                    SendPlayerDeconstructed();
                    break;
                case 0x06:
                    // ChatRequest (sizeofUsername+Username, sizeOfMessage+Message)
                    GetChatMessageRequest(packet);
                    break;
            }

        }

        public void SendPlayerSpawn()
        {
            PacketContent pak = new PacketContent();
            pak.addByte(Constants.SERVER_OPCODE_SPAWN);
            pak.addSizedString(playerName);
            pak.addFloat(xPos,1);
            pak.addFloat(yPos,1);
            pak.addFloat(zPos,1);
            pak.addFloat(yRot, 1);
            GameServer.Instance.SendPacketToAllOtherClients(pak.returnFinalPacket(), hashId);
        }

        public void SendOtherPlayerSpawns()
        {
            foreach(GameClient client in GameServer.Instance.clientList)
            {
                if (!client.playerName.Equals(playerName))
                {
                    PacketContent pak = new PacketContent();
                    pak.addByte(Constants.SERVER_OPCODE_SPAWN);
                    pak.addSizedString(client.playerName);
                    pak.addFloat(client.xPos, 1);
                    pak.addFloat(client.yPos, 1);
                    pak.addFloat(client.zPos, 1);
                    pak.addFloat(client.yRot, 1);
                    SendPaket(pak.returnFinalPacket());
                }
            }
        }

        public void SendPlayerDeconstructed()
        {
            PacketContent pak = new PacketContent();
            pak.addByte(Constants.SERVER_OPCODE_DISCONNECT);
            pak.addSizedString(playerName);
            GameServer.Instance.SendPacketToAllOtherClients(pak.returnFinalPacket(), hashId);
        }


        public void GetPlayerNameRequest(byte[] packet)
        {
            // Just playername as repsonse
            PacketContent pak = new PacketContent();
            pak.addByte(0x01);
            pak.addSizedString(playerName);
            SendPaket(pak.returnFinalPacket());

        }

        public void GetMovementRequest(byte[] packet)
        {
            // Playername + floatx,y,z and rotation y
            // But we just simple take the paket and resend it to all other clients
            GameServer.Instance.SendPacketToAllOtherClients(packet, hashId);
        }

        public void GetChatMessageRequest(byte[] packet)
        {
            //ToDo: Take the packet and send it to all players as it is :)
            GameServer.Instance.SendPacketToAllClients(packet);
        }

        public void SendPaket(byte[] packetData)
        {
            
            try
            {
                clientStream.Write(packetData, 0, packetData.Length);
                clientStream.Flush();
            } catch (Exception ex)
            {
                Console.WriteLine("ERROR ex: " + ex.ToString());
                throw;
            }
            
        }
    }
}
