using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoronaVirusUS
{
    internal class ChangedStat
    {
        public CvStat Old;
        public CvStat New;
        public ChangedStat(CvStat oldStat, CvStat newStat)
        {
            this.Old = oldStat ?? new CvStat();
            this.New = newStat;
        }

        public override string ToString()
        {
            int cdiff = New.TotalCount - Old.TotalCount;
            int ddiff = New.TotalDCount - Old.TotalDCount;

            int dailyC = New.DailyCount;
            int dailyD = New.DailyDCount;

            var tokens = new System.Collections.Generic.List<string>();
            tokens.Add($"{New.Loc} reports");
            if (cdiff > 0)
            {
                tokens.Add($"{cdiff.ToString("N0")} new");
                tokens.Add(cdiff == 1 ? "case" : "cases");
            }

            if (ddiff > 0)
            {
                if (cdiff > 0) tokens.Add("and");
                tokens.Add(ddiff.ToString("N0"));
                tokens.Add("new");
                tokens.Add(ddiff == 1 ? "death" : "deaths");
            }
            if (Old.TotalCount > 0 || Old.TotalDCount > 0)
            {
                tokens.Add($"bringing total confirmed cases there to {New.TotalCount.ToString("N0")}");

                if (New.TotalDCount > 0)
                {
                    tokens.Add($"and {New.TotalDCount.ToString("N0")} total");
                    tokens.Add(New.TotalDCount == 1 ? "death" : "deaths");
                }
            }
            else
            {
                //tokens.Add($"confirming the presence of the virus there for the first time");
            }
            var withTokens = new System.Collections.Generic.List<string>();
            if (Old.DailyCount > 0 || Old.DailyDCount > 0)
            {
                withTokens.Add("with");
                if (dailyC > 0)
                {

                    withTokens.Add($"{dailyC.ToString("N0")} new");
                    withTokens.Add(dailyC == 1 ? "case" : "cases");
                }
                if (dailyD > 0 && ddiff != dailyD)
                {
                    if (dailyC > 0) withTokens.Add("and");
                    withTokens.Add($"{dailyD.ToString("N0")} new");
                    withTokens.Add(dailyD == 1 ? "death" : "deaths");
                }
                withTokens.Add("reported today");
            }
            var result = $"{string.Join(" ", tokens)}";
            if (withTokens.Count > 0)
                result += $", {string.Join(" ", withTokens)}";
            result += ".";
            return result;
        }
        public DataTable ToDataTable()
        {
            var result = new DataTable();
            var d = new Dictionary<string, object>()
            {
                {$"{nameof(Old)}_{nameof(Old.DailyCount)}", Old.DailyCount },
                {$"{nameof(Old)}_{nameof(Old.DailyCount)}", Old.DailyDCount },
                {$"{nameof(Old)}_{nameof(Old.DailyCount)}", Old.Loc },
                {$"{nameof(Old)}_{nameof(Old.DailyCount)}", Old.TotalCount },
                {$"{nameof(Old)}_{nameof(Old.DailyCount)}", Old.TotalDCount },
                {$"{nameof(New)}_{nameof(Old.DailyCount)}", New.DailyCount },
                {$"{nameof(New)}_{nameof(Old.DailyCount)}", New.DailyDCount },
                {$"{nameof(New)}_{nameof(Old.DailyCount)}", New.Loc },
                {$"{nameof(New)}_{nameof(Old.DailyCount)}", New.TotalCount },
                {$"{nameof(New)}_{nameof(Old.DailyCount)}", New.TotalDCount },
            };

            foreach (var key in d.Keys)
            {
                result.Columns.Add(key);
            }
            var row = result.NewRow();
            foreach (var kvp in d)
            {
                row[kvp.Key] = kvp.Value;
            }
            result.Rows.Add(row);
            return result;
        }

    }
}
