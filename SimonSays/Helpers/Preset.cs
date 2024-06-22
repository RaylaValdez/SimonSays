using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimonSays.Helpers
{
    public class Preset
    {
        public string? PresetName { get; set; } = "Change Me";
        public List<PresetMember> Members { get; set; } = new List<PresetMember>();
    }

    public class PresetMember
    {
        public PresetMember()
        {

        }

        public PresetMember(string name)
        {
            this.CharacterName = name;
        }

        public string CharacterName { get; set; } = "Loremius Ipsumdus";
        public double X { get; set; }
        public double Y { get; set; }
        public float ROT { get; set; }
        public bool isAnchor { get; set; } = false;
    }
}
