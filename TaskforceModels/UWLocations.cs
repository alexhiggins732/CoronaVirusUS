using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TaskforceModels
{
    class UWLocations
    {
        public static void RunTotalDeaths()
        {
            var coll = GetLocations();
            var us = coll.UWLocations.First(x => x.local_id == "USA");
            var states = us.children;
            var d = new Dictionary<string, int>();
            foreach (var state in states)
            {
                var hospitalizations = state.GetHospitalizations().HospitalizationProjection;
                var max = hospitalizations.Where(x => x.covid_measure_name == "total_death").Max(x => x.mean);
                d.Add(state.location_name, max);
            }
            var lines = d.Select(kvp => $"{kvp.Key}\t{kvp.Value}");
            var tsv = string.Join("\r\n", lines);
        }

        public static void RunForecastedDeaths()
        {
            var forecastDate = DateTime.Parse("4/12/2020");
            var coll = GetLocations();
            var us = coll.UWLocations.First(x => x.local_id == "USA");
            var states = us.children;
            var d = new Dictionary<string, int>();
            foreach (var state in states)
            {
                var hospitalizations = state.GetHospitalizations().HospitalizationProjection;
                var max = hospitalizations.Where(x => x.covid_measure_name == "total_death" && DateTime.Parse(x.date_reported) == forecastDate).Max(x => x.mean);
                d.Add(state.location_name, max);
            }
            var lines = d.Select(kvp => $"{kvp.Key}\t{kvp.Value}");
            var tsv = string.Join("\r\n", lines);
        }

        public static Dictionary<string, int> ObservedDeaths()
        {
            var tsv = @"Alabama	93
Alaska	8
Arizona	115
Arkansas	27
California	641
Colorado	289
Connecticut	554
Delaware	35
District of Columbia	50
Florida	461
Georgia	433
Hawaii	9
Idaho	27
Illinois	720
Indiana	343
Iowa	41
Kansas	56
Kentucky	97
Louisiana	840
Maine	19
Maryland	236
Massachusetts	756
Michigan	1479
Minnesota	70
Mississippi	96
Missouri	118
Montana	6
Nebraska	17
Nevada	112
New Hampshire	23
New Jersey	2350
New Mexico	26
New York	9385
North Carolina	89
North Dakota	7
Ohio	253
Oklahoma	96
Oregon	52
Pennsylvania	557
Rhode Island	63
South Carolina	82
South Dakota	6
Tennessee	106
Texas	283
Utah	18
Vermont	27
Virginia	141
Washington	506
West Virginia	6
Wisconsin	144
Wyoming	0";
            return tsv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split('\t'))
                .ToDictionary(x => x[0], x => int.Parse(x[1]));
        }
        public static void RunAdjustedForecastByDailyDeathsSum()
        {
            var forecastDate = DateTime.Parse("4/12/2020");
            var observed = ObservedDeaths();
            var coll = GetLocations();
            var us = coll.UWLocations.First(x => x.local_id == "USA");
            var states = us.children;
            var d = new Dictionary<string, decimal>();

            foreach (var state in states)
            {
                var hospitalizations = state.GetHospitalizations().HospitalizationProjection;
                var maxDeaths = hospitalizations.Where(x => x.covid_measure_name == "total_death").Max(x => x.mean);
                var todaysForecast = hospitalizations.Where(x => x.covid_measure_name == "total_death" && DateTime.Parse(x.date_reported) == forecastDate).Max(x => x.mean);
                var todayObserved = observed[state.location_name];

                var error = todayObserved == 0 ? 1M : (decimal)todayObserved / todaysForecast;
                var daily = hospitalizations.Where(x => x.covid_measure_name == "deaths").ToList();
                var sum = daily.Sum(x => x.mean * error);

                d.Add(state.location_name, sum);
            }
            var lines = d.Select(kvp => $"{kvp.Key}\t{kvp.Value.ToString("N8")}");
            var tsv = string.Join("\r\n", lines);
        }

        public static void Run()
        {
            var coll = GetLocations();
            var us = coll.UWLocations.First(x => x.local_id == "USA");
            var states = us.children;
            var d = new Dictionary<string, InterventionCollection>();
            foreach (var state in states)
            {
                var intervention = state.GetInterventions();
                d.Add(state.location_name, intervention);
            }

            var names = d.Values.SelectMany(x => x.Interventions.Select(i => i.covid_intervention_measure_name)).Distinct().ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"State\tStay-At-Home\tNon-Essential\tTravel\tSchools");
            foreach (var ivpair in d)
            {
                var csvValues = new List<string>();
                csvValues.Add(ivpair.Key);
                var pairs = new List<DateTime>();
                var ivs = ivpair.Value.Interventions;
                csvValues.Add(ivs.FirstOrDefault(x => x.covid_intervention_measure_name == "People instructed to stay at home")?.date_reported);
                csvValues.Add(ivs.FirstOrDefault(x => x.covid_intervention_measure_name == "Non-essential services closed (i.e., bars/restaurants)")?.date_reported);
                csvValues.Add(ivs.FirstOrDefault(x => x.covid_intervention_measure_name == "Travel severely limited")?.date_reported);
                csvValues.Add(ivs.FirstOrDefault(x => x.covid_intervention_measure_name == "Educational facilities closed")?.date_reported);
                sb.AppendLine(string.Join("\t", csvValues));
            }

            var csvData = sb.ToString();
            System.IO.File.WriteAllText("Interventions.csv", csvData);
            var pops = $@"Alabama	4908621
Alaska	734002
Arizona	7378494
Arkansas	3038999
California	39937489
Colorado	5845526
Connecticut	3563077
Delaware	982895
Florida	21992985
Georgia	10736059
Hawaii	1412687
Idaho	1826156
Illinois	12659682
Indiana	6745354
Iowa	3179849
Kansas	2910357
Kentucky	4499692
Louisiana	4645184
Maine	1345790
Maryland	6083116
Massachusetts	6976597
Michigan	10045029
Minnesota	5700671
Mississippi	2989260
Missouri	6169270
Montana	1086759
Nevada	3139658
Nebraska	1952570
New Hampshire	1371246
New Jersey	8936574
New Mexico	2096640
New York	19440469
North Carolina	10611862
North Dakota	761723
Ohio	11747694
Oklahoma	3954821
Oregon	4301089
Pennsylvania	12820878
Puerto Rico	3032165
Rhode Island	1056161
South Carolina	5210095
South Dakota	903027
Tennessee	6897576
Texas	29472295
Utah	3282115
Virginia	8626207
Vermont	628061
District of Columbia	720687
Washington	7797095
West Virginia	1778070
Wisconsin	5851754
Wyoming	567025";

            var popDict = pops.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split('\t')).ToDictionary(x => x[0].Trim(), x => x.Length > 1 ? x[1].Trim() : "");

            var lockDownsByDate = new Dictionary<DateTime, List<string[]>>();
            var lockdDownPopsByDate = new Dictionary<DateTime, int>();
            var startDate = DateTime.Parse("3/18/2020");
            while (startDate < DateTime.Now)
            {
                var lockdowns = new List<string[]>();
                var lockPop = 0;
                lockDownsByDate.Add(startDate, lockdowns);
                foreach (var ivpair in d)
                {
                    string stateName = ivpair.Key;
                    var ivs = ivpair.Value.Interventions;
                    foreach (var iv in ivs)
                    {
                        if (iv.covid_intervention_measure_name == "People instructed to stay at home")
                        {
                            if (!string.IsNullOrEmpty(iv.date_reported))
                            {
                                var dt = DateTime.Parse(iv.date_reported);
                                if (dt == startDate)
                                {

                                    var pop = popDict[stateName];
                                    lockPop += int.Parse(pop);
                                    lockdowns.Add(new[] { stateName, pop.ToString() });

                                }
                            }
                        }
                    }
                }
                lockdDownPopsByDate.Add(startDate, lockPop);
                startDate = startDate.AddDays(1);

            }

            var popsCsvLines = lockdDownPopsByDate.Select(x => $"{x.Key.ToShortDateString()}\t{x.Value}").ToArray();
            var popsCsv = string.Join("\r\n", popsCsvLines);


        }
        public static UWLocationCollection GetLocations()
        {
            var json = new WebClient().DownloadString(UWEpConstants.LocationEp);
            UWLocation[] result = JsonConvert.DeserializeObject<UWLocation[]>(json);
            return new UWLocationCollection() { UWLocations = result };

        }
    }

    public class UWApi
    {
        public static T Get<T>(string endpoint)
        {
            var json = new WebClient().DownloadString(endpoint);
            var result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }


    }

    public class UWEpConstants
    {
        public const string version = "7";
        public const string LocationEp = "https://covid19.healthdata.org/api/metadata/location?v" + version;
        const string HospitalizationEPRoot = "https://covid19.healthdata.org/api/data/hospitalization?location=";
        const string PeakDeathsEPRoot = "https://covid19.healthdata.org/api/data/peak_death?location=";
        const string InterventionsEPRoot = "https://covid19.healthdata.org/api/data/intervention?location=";
        const string BedsEPRoot = "https://covid19.healthdata.org/api/data/bed?location=";

        public static string HospitalizationEP(int locationId) => $"{HospitalizationEPRoot}{locationId}";
        public static string PeakDeathsEP(int locationId) => $"{PeakDeathsEPRoot}{locationId}";

        public static string InterventionsEP(int locationId) => $"{InterventionsEPRoot}{locationId}";

        public static string BedsEP(int locationId) => $"{BedsEPRoot}{locationId}";
    }
    public class UWLocationCollection
    {
        public UWLocation[] UWLocations { get; set; }
    }

    public class UWLocation
    {
        public int location_id { get; set; }
        public string local_id { get; set; }
        public string location_name { get; set; }
        public string location_type { get; set; }
        public int path_to_top_parent { get; set; }
        public int parent_id { get; set; }
        public int level { get; set; }
        public object region_id { get; set; }
        public int sort_order { get; set; }
        public ChildLocation[] children { get; set; }
    }

    public class ChildLocation
    {
        public int location_id { get; set; }
        public string local_id { get; set; }
        public string location_name { get; set; }
        public string location_type { get; set; }
        public string path_to_top_parent { get; set; }
        public int parent_id { get; set; }
        public int level { get; set; }
        public object region_id { get; set; }
        public int sort_order { get; set; }
        public object[] children { get; set; }

        public HospitalizationProjections GetHospitalizations()
        {
            var ep = UWEpConstants.HospitalizationEP(location_id);
            var projs = UWApi.Get<HospitalizationProjection[]>(ep);
            return new HospitalizationProjections { HospitalizationProjection = projs };
        }
        public PeakDeathsProjections GetPeakDeaths()
        {
            var ep = UWEpConstants.PeakDeathsEP(location_id);
            var proj = UWApi.Get<PeakDeathProjection[]>(ep);
            return new PeakDeathsProjections { PeakDeathProjections = proj };
        }

        public InterventionCollection GetInterventions()
        {
            var ep = UWEpConstants.InterventionsEP(location_id);
            var interventions = UWApi.Get<Intervention[]>(ep);
            return new InterventionCollection() { Interventions = interventions };
        }

        public BedsCollection GetBeds()
        {
            var ep = UWEpConstants.BedsEP(location_id);
            var proj = UWApi.Get<BedsProjection[]>(ep);
            return new BedsCollection { BedsProjections = proj };
        }
    }


    public class HospitalizationProjections
    {
        public HospitalizationProjection[] HospitalizationProjection { get; set; }
    }

    public class HospitalizationProjection
    {
        public string date_reported { get; set; }
        public string data_type_name { get; set; }
        public int location_id { get; set; }
        public int mean { get; set; }
        public int upper { get; set; }
        public int lower { get; set; }
        public string dt_mean { get; set; }
        public string dt_upper { get; set; }
        public string dt_lower { get; set; }
        public string covid_measure_name { get; set; }
    }



    public class PeakDeathsProjections
    {
        public PeakDeathProjection[] PeakDeathProjections { get; set; }
    }

    public class PeakDeathProjection
    {
        public string deaths_date { get; set; }
        public int location_id { get; set; }
        public int peak_deaths_mean { get; set; }
    }



    public class InterventionCollection
    {
        public Intervention[] Interventions { get; set; }
    }

    public class Intervention
    {
        public string date_reported { get; set; }
        public int covid_intervention_id { get; set; }
        public int location_id { get; set; }
        public int covid_intervention_measure_id { get; set; }
        public string covid_intervention_measure_name { get; set; }
    }



    public class BedsCollection
    {
        public BedsProjection[] BedsProjections { get; set; }
    }

    public class BedsProjection
    {
        public int covid_bed_capacity_id { get; set; }
        public string date_reported { get; set; }
        public int location_id { get; set; }
        public int available_all_nbr { get; set; }
        public int available_icu_nbr { get; set; }
        public int all_bed_usage { get; set; }
        public int icu_bed_usage { get; set; }
    }

}

