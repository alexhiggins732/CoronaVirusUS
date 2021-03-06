﻿using HtmlAgilityPack;
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
                    result.Columns.Add("Cases");
                    result.Columns.Add("New");
                    result.Columns.Add("Deaths");
                    result.Columns.Add("New Deaths");
                    result.Columns.Add("Recovered");
                    result.Columns.Add("Active");
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
                        row["New Deaths"] = ParseInt(cols[4].InnerText);
                        row["Recovered"] = ParseInt(cols[5].InnerText);
                        row["Active"] = ParseInt(cols[6].InnerText);
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
    }
}
