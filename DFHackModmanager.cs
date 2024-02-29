using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ModHearth
{
    public class DFHackMod
    {
        public string id { get; set; }
        public int version { get; set; }

        public string ToString()
        {
            return id + " | " + version;
        }
    }

    public class DFHackModlist
    {
        public bool @default { get; set; }
        public List<DFHackMod> modlist { get; set; }
        public string name { get; set; }

        public static DFHackModlist copy(DFHackModlist other) 
        {
            DFHackModlist newlist = new DFHackModlist();
            newlist.@default = other.@default;
            newlist.modlist = new List<DFHackMod>(other.modlist);
            newlist.name = other.name;
            return newlist;
        }
    }
}
