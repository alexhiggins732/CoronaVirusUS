using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaVirusUS
{
    internal class CvCollection : IEquatable<CvCollection>, IComparable<CvCollection>
    {
        public System.Collections.Generic.List<CvStat> Stats = new List<CvStat>();


        public List<ChangedStat> GetChanges(CvCollection other)
        {
            var result = new List<ChangedStat>();
            //var dups = Stats.GroupBy(x => x.Loc).Where(x => x.Count() > 1).Select(x => x.ToArray()).ToList();
            //var otherDups = other.Stats.GroupBy(x => x.Loc).Where(x => x.Count() > 1).Select(x => x.ToArray()).ToList();
            //var d = Stats.Where(x => !string.IsNullOrEmpty(x.Loc)).ToDictionary(x => x.Loc, x => x);
            var otherD = other.Stats.Where(x => !String.IsNullOrEmpty(x.Loc))
                .ToLookup(x => x.Loc).ToDictionary(x => x.Key, x => x.First());
            for (var i = 0; i < this.Stats.Count; i++)
            {
                var l = Stats[i].Loc;
                if (otherD.ContainsKey(l))
                {
                    var otherStat = otherD[l];
                    var current = Stats[i];
                    if (current != otherStat)
                        result.Add(new ChangedStat(otherStat, current));
                }
                else
                {
                    result.Add(new ChangedStat(null, Stats[i]));
                }



            }

            return result;
        }
        public static CvCollection Create(DataTable dt)
        {
            var result = new CvCollection();
            //var dt = Hermes.StringExtensions.DatatableFromCsv(source);
            var dataRows = dt.Rows.Cast<DataRow>().ToList();
            //tring[] rows = source.Trim().Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);// new string[] { };
            var rows = dataRows.Select(row => row.ItemArray.Select(x => x.ToString()).ToArray());
            foreach (var row in rows)
            {
                string[] data = row;// row.Split(',');

                var stat = new CvStat();
                stat.Loc = (data[0] ?? "").Trim();
                stat.TotalCount = data[1].ParseAsInt(); // ParseInt(data[1]);
                stat.TotalDCount = data[3].ParseAsInt();
                stat.DailyCount = data[2].ParseAsInt();
                stat.DailyDCount = data[4].ParseAsInt();
                result.Stats.Add(stat);
            }

            return result;
        }
        public int CompareTo(CvCollection other)
        {
            var result = this.Stats.Count.CompareTo(other.Stats.Count);
            if (result == 0)
            {
                for (var i = 0; result == 0 && i < this.Stats.Count; i++)
                    result = Stats[i].CompareTo(other.Stats[i]);
            }
            return result;
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as CvCollection);
        }

        public bool Equals(CvCollection other)
        {
            if (other is null) return false;
            if (Stats.Count != other.Stats.Count) return false;
            for (var i = 0; i < Stats.Count; i++)
            {
                if (Stats[i] != other.Stats[i]) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return -1464643476 + EqualityComparer<List<CvStat>>.Default.GetHashCode(Stats);
        }

        public static bool operator ==(CvCollection left, CvCollection right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CvCollection left, CvCollection right)
        {
            return !(left == right);
        }
    }
}
