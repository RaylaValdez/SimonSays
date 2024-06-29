using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SimonSays
{
    /// <summary>
    /// Configuration class, handling all information to be saved and used.
    /// </summary>
    [Serializable]
    public class Cauliflower : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

        public bool IsListening { get; set; } = false;

        public string CatchPhrase { get; set; } = "Simon Says :";

        public bool MotionOnly { get; set; } = true;

        public bool PosSync { get; set; } = false;

        public bool UseGamepad { get; set; } = false;
        public Dictionary<int, bool> EnabledChannels { get; set; } = [];

        public List<EmoteOffsets> EmoteOffsets { get; set; } = [];

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private IDalamudPluginInterface? pluginInterface;

        /// <summary>
        /// Initializes the plugin with the provided DalamudPluginInterface instance.
        /// </summary>
        /// <param name="PluginInterface">The DalamudPluginInterface instance to initialize the plugin with.</param>
        public void Initialize(IDalamudPluginInterface PluginInterface)
        {
            // Set the DalamudPluginInterface instance for the plugin
            this.pluginInterface = PluginInterface;
        }


        /// <summary>
        /// Saves the plugin configuration.
        /// </summary>
        public void Save()
        {
            // Save the plugin configuration
            this.pluginInterface!.SavePluginConfig(this);
        }

    }

    [Serializable]
    public class EmoteOffsets
    {
        public string Emote = string.Empty;
        public string Label = string.Empty;

        public bool Enabled;
        public float X;
        public float Z;
        public float R;

    }
}
