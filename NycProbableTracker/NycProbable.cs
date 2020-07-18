using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;

namespace NycProbableTracker
{
    public class NycProbableProvider
    {

        static string URLRequest(string url) { return new WebClient().DownloadString(url); }
        public static NycProbable GetNyProbable()
        {
            var csv = URLRequest("https://raw.githubusercontent.com/nychealth/coronavirus-data/master/summary.csv");
            var csvLines= csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var probableDataLine = csvLines.First(x => x.StartsWith("Probable,"));
            var probableDataLineDelim = probableDataLine.IndexOf(",");
            var probableDataText = probableDataLine.Substring(probableDataLineDelim + 1);
            var probableDataDeaths = ParseInt(probableDataText);
            var csvResult = NycProbable.Assure(DateTime.Today, probableDataDeaths);
            return csvResult;

            var html = URLRequest("http://datawrapper.dwcdn.net/MBAaS/13/");
            var lines = html.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var dataLine = lines.First(x => x.Trim().StartsWith("chartData:")).Trim();
            var l = dataLine.Split(new[] { "\\r\\n" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var probableData = l.First(x => x.StartsWith("Probable,"));
            var probableDelim = probableData.IndexOf(",");
            var probableText = probableData.Substring(probableDelim + 1);
            var probableDeaths = ParseInt(probableText);
            var result = NycProbable.Assure(DateTime.Today, probableDeaths);
            return result;

        }
        private static int ParseInt(string value)
        {
            value = (value ?? "").Trim().TrimStart('+');
            if (string.IsNullOrEmpty(value)) value = "0";
            return int.Parse(value, System.Globalization.NumberStyles.AllowThousands);
        }


    }

    [Table("NycProbable")]
    public class NycProbable
    {
        private static string connString = "Data Source=.;Initial Catalog=CVTracker;Integrated Security=true";

        [ExplicitKey]
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public int Deaths { get; set; }
        public int NewDeaths { get; set; }

        public NycProbable() { }

        public NycProbable(DateTime date, int deaths, int newDeaths)
        {
            Date = date;
            Deaths = deaths;
            NewDeaths = newDeaths;
        }

        public static List<NycProbable> GetLatest()
        {
            using (var conn = new SqlConnection(connString))
            {
                DateTime StartDate = DateTime.Today.AddDays(-1);
                DateTime EndDate = DateTime.Today.AddDays(1);
                var query = "select * from NycProbable where date between @startDate and @endDate";
                return conn.Query<NycProbable>(query, new { StartDate, EndDate }).ToList();
            }
        }
        public static NycProbable Assure(DateTime Date, int Deaths)
        {
            using (var conn = new SqlConnection(connString))
            {
                // Get Today's stat
                var probable = conn.QueryFirstOrDefault<NycProbable>("select * from NycProbable where Date=@Date",
                    new { Date }
                    );
                // If we don't have one for today get yesterdays;
                if (probable == null)
                {
                    probable = conn.QueryFirstOrDefault<NycProbable>("select * from NycProbable where Date=@Date",
                    new { Date = Date.AddDays(-1) });
                }
                // If the deaths changed
                if (probable.Deaths != Deaths)
                {
                    if (probable.Date == Date)
                    {
                        //deaths changed for the current date.
                        probable.Deaths = Deaths;
                        int YesterdayDeaths = conn.QuerySingle<int>("select deaths from NycProbable where Date=@Date"
                            , new { Date = Date.AddDays(-1) });
                        probable.NewDeaths = Deaths - YesterdayDeaths;
                        conn.Update(probable);
                    }
                    else
                    {
                        // new date and deaths have change
                        int NewDeaths = Deaths - probable.Deaths;
                        var query = $@"insert into NYCProbable (Date, Deaths, NewDeaths) values (@Date, @Deaths, @NewDeaths) 
                            select @@identity";
                      
                        var newResult = new NycProbable(Date, Deaths, NewDeaths);
                        newResult.Id = conn.QueryFirst<int>(query, new { Date, Deaths, NewDeaths });
                        probable = newResult;
                    }
                    
                }
                else if (probable.Date != Date)
                {
                    var query = $@"insert into NYCProbable (Date, Deaths, NewDeaths) values (@Date, @Deaths, @NewDeaths) 
                            select @@identity";
                    var NewDeaths = 0;
                    var newResult = new NycProbable(Date, Deaths, NewDeaths);
                    newResult.Id = conn.QueryFirst<int>(query, new { Date, Deaths, NewDeaths });
                    probable = newResult;
                }
                return probable;


            }
        }
    }
}
