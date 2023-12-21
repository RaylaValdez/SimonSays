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

        public static void Setup()
        {
            if (movement == null)
            {
                movement = new OverrideMovement();
                movement.Enabled = true;
            }
            if (camera == null)
            {
                camera = new OverrideCamera();
            }
        }

        public static void Dispose()
        {
            movement?.Dispose();
            camera?.Dispose();
        }

        // returns true if the emote is valid
        // the emote string becomes sanitized after calling
        public static bool SanitizeEmote(ref string emote)
        {
            // Removes any of the characters from the end string
            emote = emote.Replace("(", "").Replace(")", "").Replace("/", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "");

            return Service.Emotes.Contains("/" + emote.ToLower());
        }

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

            if (!SanitizeEmote(ref emote))
            {
                Service.ChatGui.Print("You haven't specified a correct emote.");
                return;
            }

            var Chat = new XivCommonBase(Plugin.PluginInterfaceStatic).Functions.Chat;

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

        public static float ToRad(float degrees)
        {
            return degrees * (float.Pi / 180);
        }

        public static float ToDeg(float radians)
        {
            return radians * (180 / float.Pi);
        }

        public static void StartScooch()
        {
            GameObject? target = Service.TargetManager.Target;
            if (target != null)
            {
                ScoochOnOver(target);
            }
            else
            {
                movement.SoftDisable = true;
                Service.ChatGui.Print("You haven't got a target numpty");
            }
        }

        public static void StopScooch()
        {
            movement.SoftDisable = true;
        }

        public static void ScoochOnOver(GameObject target)
        {
            var character = Service.ClientState.LocalPlayer;
            /*
            Determine distance to target and close in on the target, but fucking HOW?
            point at cutie (/yes)
            go to cutie
            match cuties rotation
            begin emote
            */
            Vector3 tarPos = target.Position;
            float tarRot = target.Rotation;

            if (float.IsNaN(tarRot) || float.IsInfinity(tarRot))
            {
                tarRot = character.Rotation;
            }

            movement.DesiredPosition = tarPos;
            movement.DesiredRotation = tarRot;
            movement.SoftDisable = false;
        }

        public static void SimonSays(string Emote, bool ShouldSyncPosition)
        {
            if (!SanitizeEmote(ref Emote))
            {
                Service.ChatGui.Print("You have not specified a valid emote");
                return;
            }

            GameObject? target = Service.TargetManager.Target;
            var Chat = new XivCommonBase(Plugin.PluginInterfaceStatic).Functions.Chat;

            if (ShouldSyncPosition)
            {
                Chat.SendMessage("/sync");

                if (target != null)
                {
                    Chat.SendMessage("/tell <t> " + Plugin.Configuration.CatchPhrase + " " + Emote);
                }
                Chat.SendMessage("/" + Emote);
            }
            else
            {
                if (target != null)
                {
                    Chat.SendMessage("/tell <t> " + Plugin.Configuration.CatchPhrase + " " + Emote);
                }
                Chat.SendMessage("/" + Emote);
            }
        }



        /*command entered 'Simon Says : hum'
         *emote sent to target '/tell <t> Simon Says : hum'
         *emote done on client '/hum'
         * 
         * 
         */
    }
}
