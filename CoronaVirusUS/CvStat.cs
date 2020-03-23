using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaVirusUS
{
    internal class CvStat : IEquatable<CvStat>, IComparable<CvStat>
    {
        public string Loc { get; set; }
        public int DailyCount { get; set; }
        public int TotalCount { get; set; }
        public int DailyDCount { get; set; }
        public int TotalDCount { get; set; }

        public int CompareTo(CvStat other)
        {
            int result = TotalCount.CompareTo(other.TotalCount);
            if (result == 0)
                result = Loc.CompareTo(other.Loc);
            if (result == 0)
                result = DailyCount.CompareTo(other.DailyCount);
            if (result == 0)
                result = TotalDCount.CompareTo(other.TotalDCount);
            if (result == 0)
                result = DailyDCount.CompareTo(other.DailyDCount);
            return result;
        }

        public override string ToString()
        {
            return $"{Loc} - {TotalCount}, {TotalDCount}";
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as CvStat);
        }

        public bool Equals(CvStat other)
        {
            CvStat me = this;
            var result = other != null &&
                   Loc == other.Loc &&
                   //DailyCount == other.DailyCount &&
                   TotalCount == other.TotalCount;
            //DailyDCount == other.DailyDCount &&
            //TotalDCount == other.TotalDCount;
            if (!result)
            {
                string bp = "";
            }
            return result;
        }

        public override int GetHashCode()
        {
            var hashCode = 465562093;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Loc);
            hashCode = hashCode * -1521134295 + DailyCount.GetHashCode();
            hashCode = hashCode * -1521134295 + TotalCount.GetHashCode();
            hashCode = hashCode * -1521134295 + DailyDCount.GetHashCode();
            hashCode = hashCode * -1521134295 + TotalDCount.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(CvStat left, CvStat right)
        {
            return EqualityComparer<CvStat>.Default.Equals(left, right);
        }

        public static bool operator !=(CvStat left, CvStat right)
        {
            return !(left == right);
        }
    }
}
