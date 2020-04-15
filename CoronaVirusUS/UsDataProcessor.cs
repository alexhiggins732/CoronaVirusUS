using HtmlAgilityPack;
using System;
using System.Data;
using System.Linq;
using System.Net;

namespace CoronaVirusUS
{
    internal class UsDataProcessor
    {

        internal static DataTable GetData()
        {
            bool success = false;
            var result = new DataTable();
            while (!success)
            {
                retry:
                try
                {


                    HtmlDocument htmlDoc = new HtmlDocument();
                    string url = "https://www.worldometers.info/coronavirus/country/us/";

                    string urlResponse = URLRequest(url);

                    //Convert the Raw HTML into an HTML Object
                    htmlDoc.LoadHtml(urlResponse);

                    //for (var i = 0; i < 7; i++)
                    //    result.Columns.Add();
                    result.Columns.Add("Location");
                    result.Columns.Add("Cases", typeof(int));
                    result.Columns.Add("New", typeof(int));
                    result.Columns.Add("Deaths", typeof(int));
                    result.Columns.Add("New Deaths", typeof(int));
                    //result.Columns.Add("Recovered");
                    result.Columns.Add("Active", typeof(int));
                    var table = htmlDoc.QuerySelectorAll("table").First();

                    var dataTableRows = table.QuerySelectorAll("tbody tr");

                    foreach (var tr in dataTableRows)
                    {
                        var row = result.NewRow();
                        var cols = tr.QuerySelectorAll("td");
                        var loc = (cols.First().InnerText ?? "").Trim();
                        var active = int.Parse((cols[5].InnerText?.Trim() != "" ? cols[5].InnerText?.Trim() : null) ?? "0", System.Globalization.NumberStyles.AllowThousands);
                        //if (active == 0) continue;

                        if (loc.IndexOf("total", StringComparison.OrdinalIgnoreCase) > -1) continue;

                        row["Location"] = loc;

                        row["Cases"] = ParseInt(cols[1].InnerText);
                        row["New"] = ParseInt(cols[2].InnerText);
                        row["Deaths"] = ParseInt(cols[3].InnerText);
                        row["New Deaths"] = ParseInt(cols[4].InnerText.Replace("-", ""));
                        //row["Recovered"] = ParseInt(cols[5].InnerText);
                        row["Active"] = ParseInt(cols[5].InnerText);
                        result.Rows.Add(row);

                    }
                    success = true;
                }
                catch (WebException ex)
                {
                    Console.WriteLine($"[{DateTime.Now}]: {ex.Message}");
                    System.Threading.Thread.Sleep(10000);
                    goto retry;
                }
            }
            return result;
        }

        private static int ParseInt(string value)
        {
            value = (value ?? "").Trim().TrimStart('+');
            if (string.IsNullOrEmpty(value)) value = "0";
            return int.Parse(value, System.Globalization.NumberStyles.AllowThousands);
        }

        static string URLRequest(string url) { return new WebClient().DownloadString(url); }

        public static int GetNyProbable()
        {
            var html = URLRequest("http://datawrapper.dwcdn.net/MBAaS/13/");
            var lines = html.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var dataLine = lines.First(x => x.Trim().StartsWith("chartData:")).Trim();
            var l = dataLine.Split(new[] { "\\r\\n" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var probableData = l.First(x => x.StartsWith("Probable,"));
            var probableDelim = probableData.IndexOf(",");
            var probableText1 = probableData.Substring(probableDelim + 1);
            var result = ParseInt(probableText1);
            return result;
           
        }
    }
}
