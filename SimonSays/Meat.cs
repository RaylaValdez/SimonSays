using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud;
using XivCommon;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Configuration;
using XivCommon.Functions;
using Dalamud.Game.Gui;

namespace SimonSays
{
    internal class Meat
    {
        public static void Command(XivChatType type, string message, bool forceForTesting = false)
        {
            if (!Plugin.Configuration.IsListening)
            {
                return;
            }

            Plugin.Configuration.EnabledChannels.TryGetValue((int)type, out bool Enabled);
            if (!Enabled && !forceForTesting)
            {
                return;
            }

            string catchPhrase = Plugin.Configuration.CatchPhrase;

            if (!message.StartsWith(catchPhrase))
            {
                return;
            }

            string emote = message.Substring(catchPhrase.Length).TrimStart();
            // Removes any of the characters from the end string
            emote = emote.Replace("(", "").Replace(")", "").Replace("/", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "");

            if (!Service.Emotes.Contains("/" + emote.ToLower()))
            {
                Service.ChatGui.Print("You haven't specified a correct emote.");
                return;
            }

            var Chat = new XivCommonBase().Functions.Chat;

            if (Plugin.Configuration.MotionOnly)
            {
                emote = emote + " motion";
            }

            Chat.SendMessage("/" + emote);
        }

        public static void OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (isHandled)
                return;
            Command(type, message.ToString());
        }
    }
}
