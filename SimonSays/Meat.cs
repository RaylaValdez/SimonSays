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

        // returns true if the Emote is valid
        // the Emote string becomes sanitized after calling
        public static bool SanitizeEmote(ref string Emote)
        {
            // Removes any of the Characters from the end string
            Emote = Emote.Replace("(", "").Replace(")", "").Replace("/", "").Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "");

            return Service.Emotes.Contains("/" + Emote.ToLower());
        }

        public static void Command(XivChatType Type, string Message, bool ForceForTesting = false)
        {
            if (!Plugin.Configuration.IsListening)
            {
                return;
            }

            Plugin.Configuration.EnabledChannels.TryGetValue((int)Type, out bool Enabled);
            if (!Enabled && !ForceForTesting)
            {
                return;
            }

            string CatchPhrase = Plugin.Configuration.CatchPhrase;

            if (!Message.StartsWith(CatchPhrase))
            {
                return;
            }

            string Emote = Message.Substring(CatchPhrase.Length).TrimStart();

            if (!SanitizeEmote(ref Emote))
            {
                Service.ChatGui.Print("You haven't specified a correct Emote.");
                return;
            }

            var Chat = new XivCommonBase(Plugin.PluginInterfaceStatic).Functions.Chat;

            if (Plugin.Configuration.MotionOnly)
            {
                Emote = Emote + " motion";
            }

            Chat.SendMessage("/" + Emote);
        }

        public static void OnChatMessage(XivChatType Type, uint SenderId, ref SeString Sender, ref SeString Message, ref bool IsHandled)
        {
            if (IsHandled)
                return;
            Command(Type, Message.ToString());
        }

        public static float ToRad(float Degrees)
        {
            return Degrees * (float.Pi / 180);
        }

        public static float ToDeg(float Radians)
        {
            return Radians * (180 / float.Pi);
        }

        public static void StartScooch()
        {
            GameObject? Target = Service.TargetManager.Target;
            if (Target != null)
            {
                ScoochOnOver(Target);
            }
            else
            {
                movement.SoftDisable = true;
                Service.ChatGui.Print("You haven't got a Target numpty");
            }
        }

        public static void StopScooch()
        {
            movement.SoftDisable = true;
        }

        public static void ScoochOnOver(GameObject Target)
        {
            var Character = Service.ClientState.LocalPlayer;
            /*
            Determine distance to Target and close in on the Target, but fucking HOW?
            point at cutie (/yes)
            go to cutie
            match cuties rotation
            begin Emote
            */
            Vector3 TarPos = Target.Position;
            float TarRot = Target.Rotation;

            if (float.IsNaN(TarRot) || float.IsInfinity(TarRot))
            {
                TarRot = Character.Rotation;
            }

            movement.DesiredPosition = TarPos;
            movement.DesiredRotation = TarRot;
            movement.SoftDisable = false;
        }

        public static void SimonSays(string Emote, bool ShouldSyncPosition)
        {
            if (!SanitizeEmote(ref Emote))
            {
                Service.ChatGui.Print("You have not specified a valid Emote");
                return;
            }

            GameObject? Target = Service.TargetManager.Target;
            var Chat = new XivCommonBase(Plugin.PluginInterfaceStatic).Functions.Chat;

            if (ShouldSyncPosition)
            {
                Chat.SendMessage("/sync");

                if (Target != null)
                {
                    Chat.SendMessage("/tell <t> " + Plugin.Configuration.CatchPhrase + " " + Emote);
                }
                Chat.SendMessage("/" + Emote);
            }
            else
            {
                if (Target != null)
                {
                    Chat.SendMessage("/tell <t> " + Plugin.Configuration.CatchPhrase + " " + Emote);
                }
                Chat.SendMessage("/" + Emote);
            }
        }



        /*Command entered 'Simon Says : hum'
         *Emote sent to Target '/tell <t> Simon Says : hum'
         *Emote done on client '/hum'
         * 
         * 
         */
    }
}
