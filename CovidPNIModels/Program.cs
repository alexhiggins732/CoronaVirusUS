using Aspose.Cells;
using AsposeCreds;
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

namespace CovidPNIModels
{
    class Program
    {
        static Program()
        {
            TwitHelper.SetCreds();
            AsposeLicenseHelper.SetLicense();
        }
        static void Main(string[] args)
        {

            UpdatePNIData("national_state_pni_data_week_18.csv");
            WorkbookProcessor.CreateHtmlDoc();
            WorkbookProcessor.Run(18, 19);
        }
        static void UpdatePNIData(string fileName)
        {
            PNIImporter.Run(fileName);
        }
    }
    class WorkbookProcessor
    {
        public static void CreateHtmlDoc()
        {
            var file = @"C:\Users\alexander.higgins\source\repos\CoronaVirusUS\PNIModels\images\index.html";
            var fi = new FileInfo(file);
            fi.Directory.Create();
            if (File.Exists(file)) return;
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
                var fileName = $"{location.Key}-pni-model.png";
                sb.AppendLine($"<span class='chart newdeaths'><img src='{fileName}' /></span>");
                //var fileName2 = $"{location.Key}-Total-Deaths.png";
                //sb.AppendLine($"<span class='chart totaldeaths'><img src='{fileName2}' /></span>");
                sb.AppendLine($"</span>");
            }
            File.WriteAllText(file, $@"<!DOCTYPE html>

<html lang='en' xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <meta charset='utf-8' />
    <title>US Coronavirus COVID-19 Vs PNI</title>
</head>
<body>
{sb.ToString()}
</body>
</html>");

        }
        public static void Run(int skipFluWeek, int skipCoronaWeek)
        {
            //skipFluWeek = 17;
            //skipCoronaWeek = 18;
            var path = @"PNIChartTemplate.xlsx";
            bool exists = File.Exists(path);

            File.Copy(path, "PNIChartTemplate-processed.xlsx", true);
            var wb = new Workbook(path);
            var designer = new WorkbookDesigner(wb);

            var templateTotal = wb.Worksheets[0];
            var templateNew = wb.Worksheets[0];
            var states = StateProvider.States();
            var locations = new Dictionary<string, string>();
            locations.Add("US", "US");
            foreach (var kpv in states)
            {
                locations.Add(kpv.Key, kpv.Value);
            }

            var imageFolder = @"C:\Users\alexander.higgins\source\repos\CoronaVirusUS\PNIModels\images";
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



                    var sheetName = $"{location.Value} PNI";
                    var sheet = designer.Workbook.Worksheets.Add(sheetName);
                    sheet = designer.Workbook.Worksheets[sheet.Index];
                    var copyOptions = new CopyOptions();
                    copyOptions.ReferToDestinationSheet = true;
                    sheet.Copy(templateNew, copyOptions);



                    var newDeathData = ModelProvider.GetPniModelByState(location.Key);
                    int idx = 1;
                    bool hasCorona = false;
                    var lastPni = 0;
                    int sum = 0;
                    int minWeek = 8;
                    var rawRows = newDeathData.Rows.Cast<DataRow>().ToList();
                    var headRows = rawRows.Where(row => row.Field<int>(0) >= 40).OrderBy(row => row.Field<int>(0)).ToList();
                    var tailRows = rawRows.Where(row => row.Field<int>(0) < 40).OrderBy(row => row.Field<int>(0)).ToList();
                    headRows.AddRange(tailRows);
                    newDeathData = headRows.CopyToDataTable();
                    foreach (DataRow row in newDeathData.Rows)
                    {

                        var weekNo = row.Field<int>(0);
                        sheet.Cells.Rows[idx][0].PutValue(weekNo);
                        sheet.Cells.Rows[idx][1].PutValue(row[1]);//MinDeaths
                        sheet.Cells.Rows[idx][2].PutValue(row[2]);//Maximum P&I Deaths
                        sheet.Cells.Rows[idx][3].PutValue(row[3]);//Min and Max P&I Deaths (stacked area)
                        var val4 = row.Field<int?>(4);//2019-2020 P&I Deaths
                        if (!val4.HasValue)
                        {
                            val4 = lastPni;
                            if (weekNo == 53)
                                sheet.Cells.Rows[idx][4].PutValue(val4);
                            else
                                sheet.Cells.Rows[idx][4].PutValue((int?)null);
                        }
                        else
                        {
                            lastPni = val4.Value;
                            sheet.Cells.Rows[idx][4].PutValue(weekNo == skipFluWeek ? null : val4);
                        }

                        sheet.Cells.Rows[idx][5].PutValue(row[5]);//Average P&I Deaths
                        var val6 = row.Field<int?>(6); //Excess Weekly P&I Deaths
                        bool countExcess = (hasCorona || (weekNo >= minWeek && weekNo < skipCoronaWeek));
                        if (countExcess && val6.HasValue && val6.Value > 0)
                        {
                            sheet.Cells.Rows[idx][6].PutValue(val6);
                            sum += countExcess ? val6.Value : 0;
                        }

                        else
                            sheet.Cells.Rows[idx][6].PutValue(null);

                        sheet.Cells.Rows[idx][7].PutValue(row[7]);

                        var val8 = row.Field<int?>(8);
                        if (val8.HasValue && val8.Value > 0 && weekNo != skipCoronaWeek)
                        {
                            hasCorona = true;
                            sheet.Cells.Rows[idx][8].PutValue(row[8]);
                        }
                        else
                        {
                            sheet.Cells.Rows[idx][8].PutValue((int?)null);
                        }

                        idx++;
                    }

                    var chart = sheet.Charts[0];
                    chart.Title.Text = chart.Title.Text.Replace("Florida", location.Key);
                    var shapes = chart.Shapes.ToList();
                    var shape1 = shapes[1];
                    var dt = DateTime.Today.ToShortDateString();
                    shape1.Text = shape1.Text.Replace("4/23/2020", dt);
                    shape1.Text = shape1.Text.Replace("Week 18", "Week " + skipCoronaWeek.ToString());
                    shape1.Text = shape1.Text.Replace("Week 17", "Week " + skipFluWeek.ToString());
                    var shape0 = shapes[0];
                    //var sum = sheet.Cells.Rows[54][6].IntValue;
                    shape0.Text = shape0.Text.Replace("305", sum.ToString("N0"));
                    var fileName = $"{location.Key}-pni-model.png";

                    var filePath = Path.Combine(imageFolder, fileName);
                    chart.ToImage(filePath, options);


                }
                //{
                //    var totalDeathData = ModelProvider.GetTotalDeathData(location.Key);
                //    var sheetName = $"{location.Value} Total Deaths";
                //    var sheet = designer.Workbook.Worksheets.Add(sheetName);
                //    sheet = designer.Workbook.Worksheets[sheet.Index];
                //    var copyOptions = new CopyOptions();
                //    copyOptions.ReferToDestinationSheet = true;
                //    sheet.Copy(templateTotal, copyOptions);
                //    int idx = 1;
                //    foreach (DataRow row in totalDeathData.Rows)
                //    {
                //        sheet.Cells.Rows[idx][0].PutValue(row[0]);
                //        sheet.Cells.Rows[idx][1].PutValue(row[1]);
                //        sheet.Cells.Rows[idx][2].PutValue(row[2]);
                //        sheet.Cells.Rows[idx][3].PutValue(row[3]);
                //        sheet.Cells.Rows[idx][4].PutValue(row[4]);
                //        var val5 = row.Field<int?>(5);
                //        sheet.Cells.Rows[idx][5].PutValue(val5);
                //        idx++;
                //    }
                //    var chart = sheet.Charts[0];
                //    chart.Title.Text = chart.Title.Text.Replace("State", location.Key);
                //    var fileName = $"{location.Key}-Total-Deaths.png";
                //    var filePath = Path.Combine(imageFolder, fileName);
                //    chart.ToImage(filePath, options);
                //}



            }

            wb.Save(@"PNIChartTemplate-processed.xlsx");
        }

        private static List<string> GetLocations()
        {
            return StateProvider.States().Select(x => x.Key).ToList();
        }
    }

    public class ModelProvider
    {
        static string connString = "Data Source=.;Initial Catalog=CVTracker;Integrated Security=true";
        public static DataTable GetPniModelByState(string location)
        {
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "spGetPniModelByState";
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
