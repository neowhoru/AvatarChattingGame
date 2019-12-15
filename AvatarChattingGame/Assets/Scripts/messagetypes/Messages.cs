using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GetPlayerNameMessages : IReceivedMessage
{
    public string playerName;
}

public class PlayerViewSpawnMessage: IReceivedMessage
{
    public string playerName;
    public float xPos, yPos, zPos, yRot;
}

public class DeconstructPlayerViewMessage: IReceivedMessage
{
    public string playerName;

}

public class ChatMessage: IReceivedMessage
{
    public string playerName;
    public string message;
}

public class MovePlayerViewMessage : IReceivedMessage
{
    public string playerName;
    public float xPos, yPos, zPos, yRot;
}

public class MoveStopPlayerViewMessage: IReceivedMessage
{
    public string playerName;
    public float xPos, yPos, zPos, yRot;
}



