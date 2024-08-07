using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimonSays.Helpers;

namespace SimonSays
{
    internal class Gravy // now im hungry.
    {
        // pp: - preset position
        // pe: - preset emote

        public const string Separator = "#";
        public const string Ending = ";";

        private static string EncodeNumber(float number)
        {
            return number.ToString("N2", CultureInfo.GetCultureInfo("en-US")).Replace(",", "");
        }

        private static string EncodeNumber(double number)
        {
            return number.ToString("N2", CultureInfo.GetCultureInfo("en-US")).Replace(",", "");
        }

        private static string EncodeMemberPosition(PresetMember member)
        {
            var characterName = member.CharacterName;

            if (member.isAnchor)
            {
                return characterName + Ending;
            }

            return characterName + Separator + EncodeNumber(member.X) + Separator + EncodeNumber(member.Y) + Separator + EncodeNumber(member.ROT) + Ending;
        }

        private static string EncodeMemberEmote(PresetMember member)
        {
            var characterName = member.CharacterName;

            if (member.isAnchor)
            {
                return characterName + Separator + member.emote + Ending;
            }

            return characterName + Separator + member.emote + Ending;
        }

        /// <summary>
        /// Encodes a preset into a party chat string, including "/p "
        /// </summary>
        /// <param name="preset">Preset to encode</param>
        /// <param name="catchphrase">Catchphrase to use (own catchphrase)</param>
        /// <param name="doEmote"></param>
        /// <returns>A string suitable for party sending</returns>
        /// <exception cref="Exception">Exception if there is no anchor in the preset</exception>
        /// <exception cref="OverflowException">Exception if there is too much information encoded (more than 474 - catchphrase length)</exception>
        public static string CreatePartyChatPresetString(Preset preset, string catchphrase, bool doEmote)
        {
            var anchorMember = preset.Members.FirstOrDefault(m => m.isAnchor) ?? throw new Exception("Cannot send party message with no anchor in preset");

            var outString = doEmote ? EncodeMemberEmote(anchorMember) : EncodeMemberPosition(anchorMember);
            foreach (var member in preset.Members.Where(m => m != anchorMember))
            {
                outString += doEmote ? EncodeMemberEmote(member) : EncodeMemberPosition(member);
            }

            if (outString.Length > 500 - 3 - catchphrase.Length - 3) // -3 because "/p " is 3 characters, minus an extra 3 because of the "pr:"
            {
                throw new OverflowException($"Cannot send party message as too much information is encoded ({outString.Length + 3 + catchphrase.Length + 3} characters is greater than 500). Try shortening your catchphrase?");
            }

            return "/p " + catchphrase + (doEmote ? "pe:" : "pp:") + outString;
        }

        private static PresetMember? DecodeAnchorPartyChatMember(string memberString)
        {
            var memberSplit = memberString.Split(Separator);
            if (memberSplit.Length != 1 && memberSplit.Length != 2) // Character name and optional emote
            {
                return null;
            }

            var emote = memberSplit.Length >= 2 ? memberSplit[1] : string.Empty;

            return new PresetMember()
            {
                CharacterName = memberSplit[0],
                emote = emote,
                isAnchor = true,
                X = 0f,
                Y = 0f,
                ROT = 0f
            };
        }

        private static double DecodeNumberString(string numberString)
        {
            return double.TryParse(numberString, out var result) ? result : 0.0;
        }

        private static PresetMember? DecodePartyChatMemberPosition(string memberString)
        {
            var memberSplit = memberString.Split(Separator);

            if (memberSplit.Length != 4) // Need character name, X, Y and ROT
            {
                return null;
            }

            var characterName = memberSplit[0];
            var xString = memberSplit[1];
            var yString = memberSplit[2];
            var rotString = memberSplit[3];

            var x = DecodeNumberString(xString);
            var y = DecodeNumberString(yString);
            var rot = (float)DecodeNumberString(rotString);

            return new PresetMember()
            {
                CharacterName = characterName,
                isAnchor = false,
                emote = string.Empty,
                X = x,
                Y = y,
                ROT = rot
            };
        }

        private static PresetMember? DecodePartyChatMemberEmote(string memberString)
        {
            var memberSplit = memberString.Split(Separator);

            if (memberSplit.Length != 2) // Need character name and emote
            {
                return null;
            }

            var characterName = memberSplit[0];
            var emote = memberSplit[1];

            return new PresetMember()
            {
                CharacterName = characterName,
                emote = emote,
                isAnchor = false,
                X = 0f,
                Y = 0f,
                ROT = 0f
            };
        }

        /// <summary>
        /// Decodes a received preset via party chat message
        /// </summary>
        /// <param name="chatString">Received chat string to check</param>
        /// <returns>null when not a preset chat string. Otherwise returns the received preset</returns>
        private static Preset? DecodePartyChatPresetString(string chatString)
        {
            var doEmote = false;
            var lastColon = chatString.LastIndexOf("pp:", StringComparison.InvariantCulture);
            if (lastColon < 0)
            {
                lastColon = chatString.LastIndexOf("pe:", StringComparison.InvariantCulture);
                doEmote = true;
            }

            if (lastColon < 0)
            {
                return null;
            }

            var presetString = chatString[(lastColon + "pp:".Length)..]; // range indexer. Equivalent to Substring(lastColon)
            var presetSplit = presetString.Split(Ending);

            var preset = new Preset
            {
                PresetName = "Received preset"
            };

            var isFirst = true;
            foreach (var split in presetSplit.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                var member = isFirst ? DecodeAnchorPartyChatMember(split) : doEmote ? DecodePartyChatMemberEmote(split) : DecodePartyChatMemberPosition(split);
                if (member != null)
                {
                    preset.Members.Add(member);
                }

                isFirst = false;
            }

            return preset;
        }

        /// <summary>
        /// Sanitized by Meat.ProccessChatCommands, already checks the catchphrase
        /// </summary>
        /// <param name="presetMessage">Does not contain catchphrase, starts with either "pp:" or "pe:"</param>
        public static void HandlePartyChatPreset(string presetMessage)
        {
            var preset = DecodePartyChatPresetString(presetMessage);

            if (preset == null)
            {
                Sausages.Log.Debug("Party Chat Preset failed: Preset null");
                return;
            }

            // Check if the player is part of the received party preset
            var ownName = Sausages.ClientState.LocalPlayer!.Name.ToString();
            var playerPreset = preset.Members.FirstOrDefault(m => m.CharacterName == ownName && !m.isAnchor);
            if (playerPreset == null)
            {
                // Don't notify if the player is not part of the received preset
                Sausages.Log.Debug("Party Chat Preset cancelled: Not part of received preset");
                return;
            }

            // Get the anchor of the party
            var anchor = preset.Members.FirstOrDefault(m => m.isAnchor);
            if (anchor == null)
            {
                Sausages.Log.Debug("Party Chat Preset failed: anchor cannot be found in the received preset");
                // Don't do anything if the anchor cannot be found in the received preset
                return;
            }

            var anchorObject = Veggies.GetNearestGameObjectByName(anchor.CharacterName);
            if (anchorObject == null)
            {
                // Send a notification informing the player that they might not be in the same area as the preset's anchor
                Sausages.Log.Debug("Party Chat Preset failed: Anchor object was null");
                Veggies.SendNotification("Couldn't find the preset anchor member named " + anchor.CharacterName + ". Are you in the same area?");
                return;
            }

            // If this is a preset emote command or a preset position command
            var doEmote = presetMessage.StartsWith("pe:");
            if (doEmote)
            {
                var emote = playerPreset.emote;
                if (Meat.SanitizeEmote(ref emote))
                {
                    Veggies.SendChatMessageAsIfPlayer("/" + emote);
                }
            }
            else // Otherwise positional sync
            {
                Meat.ScoochPresetOffset(new FFXIVClientStructs.FFXIV.Common.Math.Vector3((float)playerPreset.X, 0.0f, (float)playerPreset.Y), anchorObject, playerPreset.ROT);
            }
        }
    }
}
