using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarGameServer
{
    public static class Constants
    {
        public static byte SERVER_OPCODE_SPAWN = 0x02;
        public static byte SERVER_OPCODE_MOVE = 0x03;
        public static byte SERVER_OPCODE_DISCONNECT = 0x09;
    }
}
