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

        public bool IsListening { get; set; } = false;

        public string CatchPhrase { get; set; } = "Simon Says";

        public bool MotionOnly { get; set; } = true;

        public bool PosSync { get; set; } = false;  

        public Dictionary<int, bool> EnabledChannels { get; set; } = new Dictionary<int, bool>();  

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
