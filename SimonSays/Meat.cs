using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud;
using XivCommon;
using Dalamud.Game.Text;
// using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Common.Math;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Configuration;
using XivCommon.Functions;
using Dalamud.Game.Gui;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System.Threading;
using SimonSays.Helpers;

namespace SimonSays
{
    internal class Meat
    {
        public static OverrideMovement? movement;
        public static OverrideCamera? camera;

        /// <summary>
        /// Sets up character movement and camera resources if they are not already initialized.
        /// </summary>
        public static void Setup()
        {
            // Check if character movement is already set up
            if (movement == null)
            {
                // If not, create a new instance of OverrideMovement
                movement = new OverrideMovement();

                // Enable character movement
                movement.Enabled = true;
            }

            // Check if camera is already set up
            if (camera == null)
            {
                // If not, create a new instance of OverrideCamera
                camera = new OverrideCamera();
            }
        }


        /// <summary>
        /// Disposes of resources related to character movement and camera.
        /// </summary>
        public static void Dispose()
        {
            // Dispose of movement and camera resources if they exist
            movement?.Dispose();
            movement = null;
            camera?.Dispose();
            camera = null;
        }


        // returns true if the Emote is valid
        // the Emote string becomes sanitized after calling
        /// <summary>
        /// Cleans and sanitizes the emote string by removing specified characters and checks if it's a valid emote.
        /// </summary>
        /// <param name="Emote">The emote string to sanitize.</param>
        /// <returns>True if the sanitized emote is valid; otherwise, false.</returns>
        public static bool SanitizeEmote(ref string Emote)
        {
            // Remove specified characters from the beginning and end of the emote string
            Emote = Emote.Replace("(", "").Replace(")", "").Replace("/", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").ToLower();

            // Check if the sanitized emote is in the list of valid emotes
            return Service.Emotes.Contains("/" + Emote.ToLower());
        }


        /// <summary>
        /// Processes chat commands, executing emotes based on the configuration settings and message content.
        /// </summary>
        /// <param name="Type">The type of chat message.</param>
        /// <param name="Message">The content of the chat message.</param>
        /// <param name="ForceForTesting">Flag to force command execution for testing purposes.</param>
        public static void Command(XivChatType Type, string Message, bool ForceForTesting = false)
        {
            // Check if the plugin is listening for commands
            if (Plugin.Configuration == null)
            {
                return;
            }

            if (!Plugin.Configuration.IsListening)
            {
                return;
            }
            // Check if the channel is enabled for the command, unless forced for testing
            Plugin.Configuration.EnabledChannels.TryGetValue((int)Type, out bool Enabled);
            if (!Enabled && !ForceForTesting)
            {
                return;
            }

            // Get the configured catchphrase
            string CatchPhrase = Plugin.Configuration.CatchPhrase;

            // Check if the message starts with the catchphrase
            if (!Message.StartsWith(CatchPhrase))
            {
                return;
            }

            // Extract the emote from the message
            string Emote = Message.Substring(CatchPhrase.Length).TrimStart();

            // Validate and sanitize the emote
            if (!SanitizeEmote(ref Emote))
            {
                Service.ChatGui.Print("You haven't specified a correct Emote.");
                return;
            }

            var Chat = new XivCommonBase(Plugin.PluginInterfaceStatic!).Functions.Chat;

            // Optionally add "motion" if configured for motion-only emotes
            if (Plugin.Configuration.MotionOnly)
            {
                Emote = Emote + " motion";
            }

            // Execute the emote
            Chat.SendMessage("/" + Emote);
        }


        /// <summary>
        /// Handles incoming chat messages and delegates them to the Command method.
        /// </summary>
        /// <param name="type">The type of chat message.</param>
        /// <param name="senderId">The ID of the message sender.</param>
        /// <param name="sender">The sender's name.</param>
        /// <param name="message">The content of the chat message.</param>
        /// <param name="isHandled">A flag indicating whether the message is already handled.</param>
        public static void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            // If the message is already handled, return
            if (isHandled)
                return;

            // Delegate the message to the Command method for processing
            Command(type, message.ToString());
        }




        /// <summary>
        /// Initiates character movement towards the current target or prints a message if no target is selected.
        /// </summary>
        public static void StartScooch(Vector3? Offset = null, OverrideMovement.OnCompleteDelegate? callback = null)
        {
            // Get the target object from the TargetManager service
            var Target = Service.TargetManager.Target;

            if (movement == null)
            {
                return;
            }
            // Clear any existing movement callbacks
            movement.ClearCallback();

            // Check if a target is selected
            if (Target != null)
            {
                if (Target.TargetObject != null)
                {
                    // Initiate character movement towards the target with an offset
                    ScoochOnOver(Offset, Target.TargetObject);
                }
            }
            else
            {
                // Disable character movement and notify the user of the absence of a target
                movement.SoftDisable = true;
                Service.ChatGui.Print("You haven't got a Target, numpty");
            }

            // Check if a callback function is provided
            if (callback != null)
            {
                // Set the provided callback function for movement
                movement.SetCallback(callback);
            }
        }


        /// <summary>
        /// Stops the character's movement initiated by the ScoochOnOver method.
        /// </summary>
        public static void StopScooch()
        {
            if (movement != null)
            {
                // Disable character movement
                movement.SoftDisable = true;
            }

        }


        /// <summary>
        /// Moves the local player's character towards the specified target GameObject.
        /// </summary>
        /// <param name="Target">The target GameObject to move towards.</param>
        public static void ScoochOnOver(Vector3? Offset, IGameObject Target)
        {
            // Get the local player character
            var Character = Service.ClientState.LocalPlayer;

            if (Character == null)
            {
                return;
            }

            // Check if the offset is null
            if (Offset == null)
            {
                // Set the offset to zero vector
                Offset = Vector3.Zero;
            }

            // Get the target position and rotation
            Vector3 TarPos = Target.Position;
            float TarRot = Target.Rotation;

            // Handle invalid rotation values by using the character's rotation
            if (float.IsNaN(TarRot) || float.IsInfinity(TarRot))
            {
                // Use the character's rotation as the target rotation
                TarRot = Character.Rotation;
            }

            // Calculate the offset relative to the target
            float offsetX = Offset.Value.X * MathF.Sin(TarRot);
            float offsetZ = Offset.Value.Z * MathF.Sin(TarRot);

            // Apply the offset to the target position
            TarPos.X += offsetX;
            TarPos.Z += offsetZ;

            // Apply the offset rotation to the target rotation
            TarRot += Offset.Value.Y.Degrees().Rad;
            
            if (movement != null)
            {

                // Set the desired position and rotation for character movement
                movement.DesiredPosition = TarPos;
                movement.DesiredRotation = TarRot;

                // Enable character movement
                movement.SoftDisable = false;
            }
        }


        /// <summary>
        /// Executes the specified emote with optional position synchronization.
        /// </summary>
        /// <param name="Emote">The emote to execute.</param>
        /// <param name="ShouldSyncPosition">Flag indicating whether to synchronize positions.</param>
        public static void SimonSays(string Emote, string OtherEmote, bool ShouldSyncPosition)
        {
            // Validate and sanitize the provided emote
            if (!SanitizeEmote(ref Emote))
            {
                Service.ChatGui.Print("You have not specified a valid emote");
                return;
            }
            if (!SanitizeEmote(ref OtherEmote))
            {
                Service.ChatGui.Print("You have not specified a valid other emote");
                return;
            }

            IGameObject? Target = Service.TargetManager.Target;
            var Chat = new XivCommonBase(Plugin.PluginInterfaceStatic!).Functions.Chat;

            // Synchronize positions if requested
            if (ShouldSyncPosition)
            {
                Vector3 TempOffset = EmoteHasOffset(Emote);
                // Start scooching with the callback that executes the emote once we have arrived at our destination
                StartScooch(TempOffset, () =>
                {
                    // Send emote to the target if it exists
                    if (Target != null)
                    {
                        Service.Log.Information("Telling target to do emote " + OtherEmote);
                        Chat.SendMessage("/tell <t> " + Plugin.Configuration!.CatchPhrase + " " + OtherEmote);
                    }

                    // Execute the emote globally
                    Chat.SendMessage("/" + Emote);
                });
            }
            else
            {
                // Send emote to the target if it exists
                if (Target != null)
                {
                    Service.Log.Information("Telling target to do emote " + OtherEmote);
                    Chat.SendMessage("/tell <t> " + Plugin.Configuration!.CatchPhrase + " " + OtherEmote);
                }

                // Execute the emote globally
                Chat.SendMessage("/" + Emote);
            }
        }

        /// <summary>
        /// Checks if an emote has an offset and returns the offset as a Vector3 object.
        /// </summary>
        /// <param name="Emote">The emote string to check for an offset.</param>
        /// <returns>The offset as a Vector3 object. If the emote is null or empty, returns a Vector3 with all values set to 0.</returns>
        public static Vector3 EmoteHasOffset(string Emote)
        {
            // Create a new Vector3 object called Offset
            Vector3 Offset = new Vector3();

            // Check if the Emote string is null or empty
            if (string.IsNullOrEmpty(Emote))
            {
                // If it is, return the Offset as it is
                return Offset;
            }

            // Search for the first EmoteOffset in the EmoteOffsets list that is enabled and matches the Emote string
            EmoteOffsets? emoteOffset = Plugin.Configuration!.EmoteOffsets.FirstOrDefault((offset) => offset.Enabled && offset.Emote == Emote);

            // Check if an EmoteOffset was found
            if (emoteOffset != null)
            {
                // If an EmoteOffset was found, log the information about the offset being used
                Service.Log.Information($"Using emote offset {emoteOffset.Label} for emote {Emote}");

                // Set the X, Y, and Z values of the Offset to the values from the EmoteOffset
                Offset.X = emoteOffset.X;
                Offset.Y = emoteOffset.R;
                Offset.Z = emoteOffset.Z;
            }

            // Return the Offset
            return Offset;
        }

    }
}
