using Dalamud.Game.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace SimonSays
{
    internal class ChatChannelGrab
    {
        public XivChatType ChatType { get; set; }

        public bool Enabled { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
