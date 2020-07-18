using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskforceModels
{
    class FluStats
    {
        private static string CsvSource = @"C:\Users\alexander.higgins\Downloads\NCHSData14.csv";

    }

    public class WeeklyMinMax
    {
        public WeeklyMinMax(int week, int min, int max)
        {
            Week = week;
            Min = min;
            Max = max;
        }

        public string Season { get; set; }
        public int Week { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
    }
    public class FluStatProvider
    {
        private static string path = @"C:\Users\alexander.higgins\Downloads\NCHSData14.csv";

        public static string GetMinMaxFlu()
        {
            string result = "";
            var data = GetFluStats();

            var groups = data.ToLookup(x => x.Week);

            var minmax= groups.Select(grp =>
            {
                var week = grp.Key;
                var max = grp.Max(x => x.InfluenzaDeaths);
                var min = grp.Min(x => x.InfluenzaDeaths);
                return new WeeklyMinMax(week, min, max);
            }).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Week\tMin\tMax");
            sb.Append(string.Join("\r\n", minmax.Select(x=> $"{x.Week}\t{x.Min}\t{x.Max}")));
            result = sb.ToString();
            return result;
        }
        public static List<FluStat> GetFluStats()
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.TrimOptions = TrimOptions.Trim;
                try
                {
                    csv.Configuration.RegisterClassMap<FluStatMap>();
                    var records = csv.GetRecords<FluStat>();
                    var result = records.ToList();
                    return result;
                }
                catch (Exception ex)
                {
                    string bp = ex.ToString();
                    throw;
                }

            }
        }
    }
    public class FluStatMap : ClassMap<FluStat>
    {
        public FluStatMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            //"Year","Week","Percent of Deaths Due to Pneumonia and Influenza","Expected","Threshold",
            //"All Deaths","Pneumonia Deaths","Influenza Deaths"
            Map(m => m.PercentofDeathsDuetoPneumoniaandInfluenza).Name("Percent of Deaths Due to Pneumonia and Influenza");
            Map(m => m.PneumoniaDeaths).Name("Pneumonia Deaths");
            Map(m => m.InfluenzaDeaths).Name("Influenza Deaths");
            Map(m => m.AllDeaths).Name("All Deaths");
            //Map(m => m.State).Name("Province/State");
            //Map(m => m.Country).Name("Country/Region");
            //Map(m => m.Confirmed).Name("Confirmed");
            //Map(m => m.Deaths).Name("Deaths");
            //Map(m => m.Latitude).Name("Latitude").Optional();
            //Map(m => m.Longitude).Name("Longitude").Optional();
            //Map(m => m.LastUpdate).Name("LastUpdate").Optional();
            //Map(m => m.Recovered).Name("Recovered").Optional();
        }

    }
    public class Rootobject
    {
        public FluStat[] FluStats { get; set; }
    }

    public class FluStat
    {
        public int Year { get; set; }
        public int Week { get; set; }
        public float PercentofDeathsDuetoPneumoniaandInfluenza { get; set; }
        public float Expected { get; set; }
        public float Threshold { get; set; }
        public int AllDeaths { get; set; }
        public int PneumoniaDeaths { get; set; }
        public int InfluenzaDeaths { get; set; }
    }

}
