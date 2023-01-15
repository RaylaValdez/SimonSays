using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SimonSays
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

        public string TriggerPhrase { get; set; } = "please do";

        public bool AllowSit { get; set; }

        public bool MotionOnly { get; set; } = true;

        public bool AllowAllCommands { get; set; }

        public bool UseRegex { get; set; }

        public string CustomPhrase { get; set; } = string.Empty;

        public string ReplaceMatch { get; set; } = string.Empty;

        public string TestInput { get; set; } = string.Empty;

        public List<ChatChannelGrab> EnabledChannels { get; set; } = new List<ChatChannelGrab>();

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
