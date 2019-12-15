using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NetworkGameHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> remotePlayerViews = new List<GameObject>();
    public TCPClient client;
    private Player player;
    private object cacheLock = new object();
    private List<IReceivedMessage> messageQueue = new List<IReceivedMessage>();

    // UI
    public Text chatText;
    public TMPro.TextMeshProUGUI inputText;

    public GameObject playerViewPrefab;

    void Awake()
    {
        client = GetComponent<TCPClient>();
        player = GetComponent<Player>();
        client.OnConnected += OnClientConnected;
        client.OnDisconnected += OnClientDisconnected;
        client.OnMessageReceived += HandleMessage;
        if (!client.IsConnected)
        {
            client.ConnectToTcpServer();
        }
    }

    private void HandleMessage(byte[] packet)
    {
        PacketReader reader = new PacketReader(packet);
        byte opCode = (byte) reader.readUint8();

        lock (cacheLock)
        {
            switch (opCode)
            {
                case 0x01:
                    Debug.Log("Should Set Name");
                    GetPlayerNameMessages message = new GetPlayerNameMessages();
                    message.playerName = reader.readSizedString();
                    messageQueue.Add(message);
                    break;
                case 0x02:
                    Debug.Log("A new player arrives");
                    PlayerViewSpawnMessage spawnMessage = new PlayerViewSpawnMessage();
                    spawnMessage.playerName = reader.readSizedString();
                    spawnMessage.xPos = reader.readFloat(1);
                    spawnMessage.yPos = reader.readFloat(1);
                    spawnMessage.zPos = reader.readFloat(1);
                    spawnMessage.yRot = reader.readFloat(1);
                    messageQueue.Add(spawnMessage);
                    break;
                case 0x04:
                    Debug.Log("PlayerView Moves to Position");
                    MovePlayerViewMessage movePlayerViewMessage = new MovePlayerViewMessage();
                    movePlayerViewMessage.playerName = reader.readSizedString();
                    movePlayerViewMessage.xPos = reader.readFloat(1);
                    movePlayerViewMessage.yPos = reader.readFloat(1);
                    movePlayerViewMessage.zPos = reader.readFloat(1);
                    movePlayerViewMessage.yRot = reader.readFloat(1);
                    messageQueue.Add(movePlayerViewMessage);
                    break;
                case 0x06:
                    Debug.Log("Incoming Chatmessage");
                    ChatMessage chatMessage = new ChatMessage();
                    chatMessage.playerName = reader.readSizedString();
                    chatMessage.message = reader.readSizedString();
                    messageQueue.Add(chatMessage);
                    break;
                case 0x09:
                    Debug.Log("Deleting PlayerView");
                    DeconstructPlayerViewMessage deconstructPlayerViewMessage = new DeconstructPlayerViewMessage();
                    deconstructPlayerViewMessage.playerName = reader.readSizedString();
                    messageQueue.Add(deconstructPlayerViewMessage);
                    break;
            }
        }
    }

    private void Update()
    {
        lock (cacheLock)
        {
            if (messageQueue.Count > 0)
            {
                List<IReceivedMessage> messageToDelete = new List<IReceivedMessage>();
                foreach (var message in messageQueue)
                {
                    Debug.Log("MEssage " + message.GetType());
                    HandleMessageQueue(message);
                    messageToDelete.Add(message);
                }

                foreach (IReceivedMessage toDeleteMessage in messageToDelete)
                {
                    messageQueue.Remove(toDeleteMessage);
                }
            }
        }
    }

    private void HandleMessageQueue(IReceivedMessage message)
    {
        if (message.GetType() == typeof(GetPlayerNameMessages))
        {
            GetPlayerNameMessages finalMessage = (GetPlayerNameMessages) message;
            player.SetPlayerName(finalMessage.playerName);
        }

        if (message.GetType() == typeof(PlayerViewSpawnMessage))
        {
            PlayerViewSpawnMessage finalMessage = (PlayerViewSpawnMessage) message;
            SpawnPlayerView(finalMessage);
        }
        
        if (message.GetType() == typeof(MovePlayerViewMessage))
        {
            MovePlayerViewMessage finalMessage = (MovePlayerViewMessage)message;
            UpdatePlayerMove(finalMessage);

        }

        if (message.GetType() == typeof(ChatMessage))
        {
            ChatMessage finalMessage = (ChatMessage) message;
            HandleChatMessage(finalMessage);
        }

        if (message.GetType() == typeof(DeconstructPlayerViewMessage))
        {
            DeconstructPlayerViewMessage finalMessage = (DeconstructPlayerViewMessage) message;
            DeconstructPlayerView(finalMessage);
        }
    }

    public void SpawnPlayerView(PlayerViewSpawnMessage playerViewSpawnMessage)
    {
        bool isSpawned = false;
        // We safetly check if player isnt already spawned
        foreach (GameObject playerView in remotePlayerViews)
        {
            if (playerView.GetComponent<PlayerView>().playerName.Equals(playerViewSpawnMessage.playerName))
            {
                isSpawned = true;
            }
        }

        if (!isSpawned && playerViewPrefab != null)
        {
            
            Vector3 position = new Vector3(playerViewSpawnMessage.xPos, playerViewSpawnMessage.yPos,
                playerViewSpawnMessage.zPos);
            GameObject playerView = Instantiate(playerViewPrefab);
            playerView.transform.position = position;
            Quaternion rotation = new Quaternion(playerView.transform.rotation.x, playerViewSpawnMessage.yRot,
                playerView.transform.rotation.z, playerView.transform.rotation.w);

            playerView.transform.rotation = rotation;
            Debug.Log("Set Playername for spawn " +playerViewSpawnMessage.playerName);
            playerView.GetComponent<PlayerView>().SetPlayerName(playerViewSpawnMessage.playerName);
            string chatTextMessage = string.Format("<color=blue>{0}</color> left the game\n", playerViewSpawnMessage.playerName);
            chatText.text = chatText.text + chatTextMessage;
            remotePlayerViews.Add(playerView);
        }
    }

    public void HandleChatMessage(ChatMessage chatMessage)
    {
        if (chatMessage.message.Length > 0)
        {
            string chatTextMessage = string.Format("<color=green>{0}</color>{1}\n", chatMessage.playerName, chatMessage.message);
            chatText.text = chatText.text + chatTextMessage;    
        }
    }

    public void UpdatePlayerMove(MovePlayerViewMessage movePlayerViewMessage)
    {
        foreach (GameObject playerView in remotePlayerViews)
        {
            if (playerView.GetComponent<PlayerView>().playerName.Equals(movePlayerViewMessage.playerName))
            {
                playerView.GetComponent<PlayerView>().MoveToPosition(movePlayerViewMessage.xPos,
                    movePlayerViewMessage.yPos, movePlayerViewMessage.zPos, movePlayerViewMessage.yRot);
            }
        }
    }

    public void DeconstructPlayerView(DeconstructPlayerViewMessage deconstructPlayerViewMessage)
    {
        List<GameObject> playerViewToDelete = new List<GameObject>();
        foreach (GameObject playerView in remotePlayerViews)
        {
            if (playerView.GetComponent<PlayerView>().playerName.Equals(deconstructPlayerViewMessage.playerName))
            {
                ShowDecostructMessage(deconstructPlayerViewMessage.playerName);
                playerViewToDelete.Add(playerView);
            }
        }

        if (playerViewToDelete.Count>0)
        {
            foreach (GameObject deletedPlayerView in playerViewToDelete)
            {
                remotePlayerViews.Remove(deletedPlayerView);
                Destroy(deletedPlayerView);
            }
        }
        
    }

    public void ShowDecostructMessage(string playerName)
    {
        string chatTextMessage = string.Format("<color=red>{0}</color> left the game\n", playerName);
        chatText.text = chatText.text + chatTextMessage;
    }

    private void OnClientConnected(TCPClient client)
    {
        SendRequestNamePacket();
    }

    private void OnClientDisconnected(TCPClient client)
    {
        SendDisconnectPacket();
    }

    public void SendRequestNamePacket()
    {
        PacketContent pak = new PacketContent();
        pak.addByte(0x01);
        pak.addByte(0x00);
        client.SendMessage(pak.returnFinalPacket());
    }

    public void SendDisconnectPacket()
    {
        PacketContent pak = new PacketContent();
        pak.addByte(0x09);
        client.SendMessage(pak.returnFinalPacket());
    }

    public void SendChatMessage()
    {
        PacketContent pak = new PacketContent();
        pak.addByte(0x06);
        pak.addSizedString(player.playerName);
        pak.addSizedString(inputText.text);
        client.SendMessage(pak.returnFinalPacket());
    }

    public void SendMoveMessage(Vector3 position, float yPos)
    {
        PacketContent pak = new PacketContent();
        pak.addByte(0x04);
        pak.addSizedString(player.playerName);
        pak.addFloat(position.x, 1);
        pak.addFloat(position.y, 1);
        pak.addFloat(position.z, 1);
        pak.addFloat(yPos, 1);
        client.SendMessage(pak.returnFinalPacket());
    }
}