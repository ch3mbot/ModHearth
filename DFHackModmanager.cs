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

        public override string ToString()
        {
            return id + " | " + version;
        }

        public static bool operator ==(DFHackMod lhs, DFHackMod rhs)
        {
            if (ReferenceEquals(lhs, rhs)) return true;
            if (ReferenceEquals(lhs, null)) return false;
            if (ReferenceEquals(rhs, null)) return false;
            return lhs.ToString() == rhs.ToString();
        }
        public static bool operator !=(DFHackMod lhs, DFHackMod rhs)
        {
            if (ReferenceEquals(lhs, rhs)) return false;
            if (ReferenceEquals(lhs, null)) return true;
            if (ReferenceEquals(rhs, null)) return true;
            return lhs.ToString() != rhs.ToString();
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(Object? other)
        {
            if(other is DFHackMod dfother)
                return this == dfother;
            return false;
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
