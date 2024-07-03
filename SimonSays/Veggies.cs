using Dalamud.Interface.ImGuiNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XivCommon;
using Dalamud.Interface;
using Dalamud.Game.ClientState.Objects.Types;

namespace SimonSays
{
    internal class Veggies
    {
        public static void SendChatMessageAsIfPlayer(string message)
        {
            var Chat = new XivCommonBase(Potatoes.PluginInterfaceStatic!).Functions.Chat;
            Chat.SendMessage(message);
        }

        public static void SendNotification(string message)
        {
            var notif = new Notification();
            notif.Content = message;
            Sausages.NotificationManager.AddNotification(notif);
        }

        public static IGameObject? GetNearestGameObjectByName(string name)
        {
            var gameObjects = Sausages.ObjectTable;
            return gameObjects.Where(obj => string.Equals(obj.Name.ToString(), name, StringComparison.CurrentCultureIgnoreCase)) // where the name is the same
                .OrderBy(obj => (obj.YalmDistanceX * obj.YalmDistanceX) + (obj.YalmDistanceZ * obj.YalmDistanceZ)) // order by distance squared
                .FirstOrDefault(); // get first, which is nearest
        }
    }
}
