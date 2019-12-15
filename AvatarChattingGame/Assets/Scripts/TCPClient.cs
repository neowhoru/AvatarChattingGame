using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class TCPClient : MonoBehaviour
{
    // Start is called before the first frame
    public Action<TCPClient> OnConnected = delegate { };
    public Action<TCPClient> OnDisconnected = delegate { };
    public Action<byte[]> OnMessageReceived = delegate { };
    public string IPAddress = "localhost";
    public int Port = 8888;
    private bool running;

    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    private NetworkStream stream;

    public bool IsConnected
    {
        get { return socketConnection != null && socketConnection.Connected; }
    }

    public void ConnectToTcpServer()
    {
        try
        {
            Debug.Log(string.Format("Connecting to {0}:{1}", IPAddress, Port));
            clientReceiveThread = new Thread(ListenForData);
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    /// <summary> 	
    /// Runs in background clientReceiveThread; Listens for incoming data. 	
    /// </summary>     
    private void ListenForData()
    {
        Debug.Log("Wait for Data");
        try
        {
            socketConnection = new TcpClient(IPAddress, Port);
            OnConnected(this);

            Byte[] bytes = new Byte[1024];
            running = true;
            while (running)
            {
                // Get a stream object for reading
                using (stream = socketConnection.GetStream())
                {
                    int length;
                    // Read incoming stream into byte array. 					
                    while (running && stream.CanRead)
                    {
                        length = stream.Read(bytes, 0, bytes.Length);
                        if (length != 0)
                        {
                            var incomingData = new byte[length];
                            Array.Copy(bytes, 0, incomingData, 0, length);

                            MessageReceived(incomingData);
                        }
                    }
                }
            }

            socketConnection.Close();
            Debug.Log("Disconnected from server");
            OnDisconnected(this);
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }


    public void CloseConnection()
    {
        SendMessage("!disconnect");
        running = false;
    }

    public void MessageReceived(byte[] serverMessage)
    {
        Debug.Log("Receive Server message : " + StringUtils.bytesToString(serverMessage));
        OnMessageReceived(serverMessage);
    }

    /// <summary> 	
    /// Send message to server using socket connection. 	
    /// </summary> 	
    public bool SendMessage(byte[] clientMessage)
    {
        if (socketConnection != null && socketConnection.Connected)
        {
            try
            {
                // Get a stream object for writing. 			
                NetworkStream stream = socketConnection.GetStream();
                if (stream.CanWrite)
                {
                    // Write byte array to socketConnection stream.                 
                    stream.Write(clientMessage, 0, clientMessage.Length);
                    OnSentMessage(clientMessage);
                    return true;
                }
            }
            catch (SocketException socketException)
            {
                Debug.Log("Socket exception: " + socketException);
            }
        }

        return false;
    }

    public virtual void OnSentMessage(byte[] message)
    {
    }
}