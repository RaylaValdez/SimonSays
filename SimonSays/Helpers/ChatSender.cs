using System;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using SimonSays;

namespace SimonSays.Helpers;

/// <summary>
/// Sends chat messages as if the player typed them, using the game's own
/// chat box processing function exposed by FFXIVClientStructs.
/// </summary>
internal sealed class ChatSender : IDisposable
{
    private const AllowedEntities ChatSanitiseFlags = (AllowedEntities)0x27F;

    private static ChatSender? instance;

    /// <summary>
    /// Gets the shared instance, creating it on first use.
    /// </summary>
    public static ChatSender Instance => instance ??= new ChatSender();

    private ChatSender()
    {
    }

    /// <summary>
    /// Sends a message as if the player typed it into the chat box.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <exception cref="ArgumentException">If the message is empty, too long, or contains invalid characters.</exception>
    public unsafe void SendMessage(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        if (bytes.Length == 0)
        {
            throw new ArgumentException("Message is empty.", nameof(message));
        }

        if (bytes.Length > 500)
        {
            throw new ArgumentException("Message is longer than 500 bytes.", nameof(message));
        }

        if (message.Length != this.SanitiseText(message).Length)
        {
            throw new ArgumentException("Message contained invalid characters.", nameof(message));
        }

        var uText = Utf8String.FromString(message);
        try
        {
            UIModule.Instance()->ProcessChatBoxEntry(uText);
        }
        finally
        {
            uText->Dtor(true);
        }
    }

    /// <summary>
    /// Sanitises a string using the game's own chat sanitiser.
    /// </summary>
    /// <param name="text">The text to sanitise.</param>
    /// <returns>The sanitised text.</returns>
    public unsafe string SanitiseText(string text)
    {
        var uText = Utf8String.FromString(text);
        try
        {
            uText->SanitizeString(ChatSanitiseFlags);
            return uText->ToString();
        }
        finally
        {
            uText->Dtor(true);
        }
    }

    /// <summary>
    /// Releases the shared instance. Called on plugin dispose.
    /// </summary>
    public void Dispose()
    {
        instance = null;
    }
}
