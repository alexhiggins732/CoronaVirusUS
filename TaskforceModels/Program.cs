using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitCreds;

namespace TaskforceModels
{
    class Program
    {

        static Program()
        {
            TwitHelper.SetCreds();
            AsposeCreds.AsposeLicenseHelper.SetLicense();
        }
        static void Main(string[] args)
        {
            //ReportCardProcessor.Run();
            //UWLocations.RunAdjustedForecastByDailyDeathsSum();
            //WorkbookProcessor.CreateHtmlDoc();
            WorkbookProcessor.Run();

        }
    }

    class WorkbookProcessor
    {
        public static void CreateHtmlDoc()
        {
            var file = @"C:\Users\alexander.higgins\source\repos\CoronaVirusUS\TaskforceModels\images\index.html";
            var states = StateProvider.States();
            var locations = new Dictionary<string, string>();
            locations.Add("US", "US");
            var sb = new StringBuilder();
            foreach (var kpv in states)
            {
                locations.Add(kpv.Key, kpv.Value);
            }
            foreach (var location in locations)
            {
                sb.AppendLine($"<span class='state'>");
                sb.AppendLine($"<span class='statename'>{location.Key}</span>");
                var fileName = $"{location.Key}-New-Deaths.png";
                sb.AppendLine($"<span class='chart newdeaths'><img src='{fileName}' /></span>");
                var fileName2 = $"{location.Key}-Total-Deaths.png";
                sb.AppendLine($"<span class='chart totaldeaths'><img src='{fileName2}' /></span>");
                sb.AppendLine($"</span>");
            }
           File.WriteAllText(file, $@"<!DOCTYPE html>

<html lang='en' xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta charset='utf-8' />
    <title>Global Coronavirus COVID-19</title>
</head>
<body>
{sb.ToString()}
</body>
</html>");

        }
        public static void Run()
        {
            var path = @"\\homer\users\alexander.higgins\Documents\UWModelTemplate - Copy.xlsx";
            bool exists = File.Exists(path);

            File.Copy(path, @"\\homer\users\alexander.higgins\Documents\UWModelTemplate-processed.xlsx", true);
            var wb = new Workbook(path);
            var designer = new WorkbookDesigner(wb);

            var templateTotal = wb.Worksheets[0];
            var templateNew = wb.Worksheets[1];
            var states = StateProvider.States();
            var locations = new Dictionary<string, string>();
            locations.Add("US", "US");
            foreach (var kpv in states)
            {
                locations.Add(kpv.Key, kpv.Value);
            }

            var imageFolder = @"C:\Users\alexander.higgins\source\repos\CoronaVirusUS\TaskforceModels\images";
            Directory.CreateDirectory(imageFolder);
            var options = new Aspose.Cells.Rendering.ImageOrPrintOptions
            {
                SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias | System.Drawing.Drawing2D.SmoothingMode.AntiAlias,
                DefaultFont = "Calibri",
                HorizontalResolution = 300,
                TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit,
                VerticalResolution = 300,
                ChartImageType = ImageFormat.Png
            };

            foreach (var location in locations)
            {
                Console.WriteLine($"[{DateTime.Now} Processing {location.Key}");
                {

                    var newDeathData = ModelProvider.GetNewDeathData(location.Key);
                    var sheetName = $"{location.Value} New Deaths";
                    var sheet = designer.Workbook.Worksheets.Add(sheetName);
                    sheet = designer.Workbook.Worksheets[sheet.Index];
                    var copyOptions = new CopyOptions();
                    copyOptions.ReferToDestinationSheet = true;
                    sheet.Copy(templateNew, copyOptions);
                    int idx = 1;
                    foreach (DataRow row in newDeathData.Rows)
                    {
                        sheet.Cells.Rows[idx][0].PutValue(row[0]);
                        sheet.Cells.Rows[idx][1].PutValue(row[1]);
                        sheet.Cells.Rows[idx][2].PutValue(row[2]);
                        sheet.Cells.Rows[idx][3].PutValue(row[3]);
                        var val4 = row.Field<int?>(4);
                        sheet.Cells.Rows[idx][4].PutValue(val4);
                        idx++;
                    }
                    var chart = sheet.Charts[0];
                    chart.Title.Text = chart.Title.Text.Replace("State", location.Key);
                    var fileName = $"{location.Key}-New-Deaths.png";

                    var filePath = Path.Combine(imageFolder, fileName);
                    chart.ToImage(filePath, options);


                }
                {
                    var totalDeathData = ModelProvider.GetTotalDeathData(location.Key);
                    var sheetName = $"{location.Value} Total Deaths";
                    var sheet = designer.Workbook.Worksheets.Add(sheetName);
                    sheet = designer.Workbook.Worksheets[sheet.Index];
                    var copyOptions = new CopyOptions();
                    copyOptions.ReferToDestinationSheet = true;
                    sheet.Copy(templateTotal, copyOptions);
                    int idx = 1;
                    foreach (DataRow row in totalDeathData.Rows)
                    {
                        sheet.Cells.Rows[idx][0].PutValue(row[0]);
                        sheet.Cells.Rows[idx][1].PutValue(row[1]);
                        sheet.Cells.Rows[idx][2].PutValue(row[2]);
                        sheet.Cells.Rows[idx][3].PutValue(row[3]);
                        sheet.Cells.Rows[idx][4].PutValue(row[4]);
                        var val5 = row.Field<int?>(5);
                        sheet.Cells.Rows[idx][5].PutValue(val5);
                        idx++;
                    }
                    var chart = sheet.Charts[0];
                    chart.Title.Text = chart.Title.Text.Replace("State", location.Key);
                    var fileName = $"{location.Key}-Total-Deaths.png";
                    var filePath = Path.Combine(imageFolder, fileName);
                    chart.ToImage(filePath, options);
                }



            }

            wb.Save(@"\\homer\users\alexander.higgins\Documents\UWModelTemplate-processed.xlsx");
        }

        private static List<string> GetLocations()
        {
            return StateProvider.States().Select(x => x.Key).ToList();
        }
    }

    public class ModelProvider
    {
        static string connString = "Data Source=.;Initial Catalog=CVTracker;Integrated Security=true";
        public static DataTable GetNewDeathData(string location)
        {
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "spModelNewDeathsByDate";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@state", location);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
        public static DataTable GetTotalDeathData(string location)
        {
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "spModelTotalDeathsByState";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@state", location);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
    }

    public class StateProvider
    {
        public static Dictionary<string, string> States()
        {
            return new Dictionary<string, string>
            {
                { "Alabama", "AL" },
                { "Alaska", "AK" },
                { "Arizona", "AZ" },
                { "Arkansas", "AR" },
                { "California", "CA" },
                { "Colorado", "CO" },
                { "Connecticut", "CT" },
                { "Delaware", "DE" },
                { "District of Columbia", "DC" },
                { "Florida", "FL" },
                { "Georgia", "GA" },
                { "Hawaii", "HI" },
                { "Idaho", "ID" },
                { "Illinois", "IL" },
                { "Indiana", "IN" },
                { "Iowa", "IA" },
                { "Kansas", "KS" },
                { "Kentucky", "KY" },
                { "Louisiana", "LA" },
                { "Maine", "ME" },
                { "Montana", "MT" },
                { "Nebraska", "NE" },
                { "Nevada", "NV" },
                { "New Hampshire", "NH" },
                { "New Jersey", "NJ" },
                { "New Mexico", "NM" },
                { "New York", "NY" },
                { "North Carolina", "NC" },
                { "North Dakota", "ND" },
                { "Ohio", "OH" },
                { "Oklahoma", "OK" },
                { "Oregon", "OR" },
                { "Maryland", "MD" },
                { "Massachusetts", "MA" },
                { "Michigan", "MI" },
                { "Minnesota", "MN" },
                { "Mississippi", "MS" },
                { "Missouri", "MO" },
                { "Pennsylvania", "PA" },
                { "Rhode Island", "RI" },
                { "South Carolina", "SC" },
                { "South Dakota", "SD" },
                { "Tennessee", "TN" },
                { "Texas", "TX" },
                { "Utah", "UT" },
                { "Vermont", "VT" },
                { "Virginia", "VA" },
                { "Washington", "WA" },
                { "West Virginia", "WV" },
                { "Wisconsin", "WI" },
                { "Wyoming", "WY" }

            };
        }
    }
}
