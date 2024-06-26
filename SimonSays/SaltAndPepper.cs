using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimonSays
{
    /// <summary>
    /// Chat Channel Types class, storing Channels as ints.
    /// </summary>
    internal class SaltAndPepper
    {
        /// <summary>
        /// Dictionary mapping numeric values to chat type strings.
        /// </summary>
        public static Dictionary<int, string> ChatTypes = new()
        {
            { 37, "CWLS1" },
            { 101, "CWLS2" },
            { 102, "CWLS3" },
            { 103, "CWLS4" },
            { 104, "CWLS5" },
            { 105, "CWLS6" },
            { 106, "CWLS7" },
            { 107, "CWLS8" },
            { 16, "LS1" },
            { 17, "LS2" },
            { 18, "LS3" },
            { 19, "LS4" },
            { 20, "LS5" },
            { 21, "LS6" },
            { 22, "LS7" },
            { 23, "LS8" },
            { 13, "Tell" },
            { 10, "Say" },
            { 14, "Party" },
            { 30, "Yell" },
            { 11, "Shout" },
            { 24, "Free Company" },
            { 15, "Alliance" }
        };
    }
}
