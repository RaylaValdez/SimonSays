using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.ImGuiNotification;
using SimonSays.Helpers;

namespace SimonSays;

internal class Veggies
{
    /// <summary>
    /// Sends a chat message as if the player typed it.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public static void SendChatMessageAsIfPlayer(string message)
    {
        try
        {
            ChatSender.Instance.SendMessage(message);
        }
        catch (Exception ex)
        {
            Sausages.Log.Error(ex, $"Failed to send chat message: {message}");
            SendNotification("SimonSays failed to send a chat message. " + ex.Message);
        }
    }

    /// <summary>
    /// Shows an in-game notification.
    /// </summary>
    /// <param name="message">The notification content.</param>
    public static void SendNotification(string message)
    {
        var notif = new Notification
        {
            Content = message,
        };
        Sausages.NotificationManager.AddNotification(notif);
    }

    /// <summary>
    /// Gets the nearest game object with the given name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>The nearest matching game object, or null.</returns>
    public static IGameObject? GetNearestGameObjectByName(string name)
    {
        return Sausages.ObjectTable
            .Where(obj => string.Equals(obj.Name.TextValue, name, StringComparison.CurrentCultureIgnoreCase))
            .MinBy(obj => obj.CurrentDistance);
    }
}
