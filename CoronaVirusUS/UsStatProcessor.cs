using AsposeCreds;
using TwitCreds;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace CoronaVirusUS
{
    class UsStatProcessor
    {
        static UsStatProcessor()
        {
            TwitHelper.SetCreds();
            AsposeLicenseHelper.SetLicense();
        }
        public static void Run()
        {
            DataTable GetData() => UsDataProcessor.GetData();
            var data = GetData();
            var coll = CvCollection.Create(data);


            Console.Title = "Getting data";


            System.Console.Title = $"[{DateTime.Now}] Sleeping 30 seconds";
            if (!System.Diagnostics.Debugger.IsAttached)
                System.Threading.Thread.Sleep(30000);
            bool pubbedAggr = false;
            int lastCTotal = 0;
            int lastDTotal = 0;
            int lastCtRowCount = 0;
            DateTime lastUpdated = DateTime.Now;


            int sleepMinutes = Properties.Settings.Default.CheckTimeoutMinutes;
            DateTime lastTableDate = DateTime.Now.AddMinutes(-sleepMinutes);
            int sleep = sleepMinutes * 60 * 1000;
            int globalChartTimeout = Properties.Settings.Default.GlobalChartTimeout;
            while (true)
            {
                restart:
                var start = DateTime.Now;
                System.Console.Title = $"[{DateTime.Now}] Refreshing data";
                data = GetData();
                var temp = data.Clone();

                if (data.Rows.Count == 0)
                {
                    System.Console.Title = $"[{DateTime.Now}] Failed to retrieve data";
                    System.Threading.Thread.Sleep(sleep);
                    continue;

                }
                var newColl = CvCollection.Create(data);
                var dCount = newColl.Stats.Sum(x => x.TotalDCount);
                var cCount = newColl.Stats.Sum(x => x.TotalCount);
                Console.Title = $"[{DateTime.Now.ToLongTimeString()}] {cCount} - {dCount} [{lastUpdated.ToLongTimeString()}] ";
                var dRotwCount = newColl.Stats.Where(x => x.Loc != "China").Sum(x => x.TotalDCount);
                var cRotwCount = newColl.Stats.Where(x => x.Loc != "China").Sum(x => x.TotalCount);

                var newRotwCCount = newColl.Stats.Where(x => x.Loc != "China").Sum(x => x.DailyCount);
                var newRotwDCount = newColl.Stats.Where(x => x.Loc != "China").Sum(x => x.DailyDCount);

                var newCCount = newColl.Stats.Sum(x => x.DailyCount);
                var newDCount = newColl.Stats.Sum(x => x.DailyDCount);


                var changes = newColl.GetChanges(coll);
                if (changes.Count > 0)
                {
                    if (HasBadData(changes))
                    {
                        System.Threading.Thread.Sleep(30000);
                        goto restart;
                    }
                }

                int topLimit = Properties.Settings.Default.TopLimit;
                var top = newColl.Stats.OrderByDescending(x => x.TotalCount).Take(topLimit).Select(x => x.Loc);
                var topChanges = changes.Where(x => top.Contains(x.New.Loc));
                foreach (var change in topChanges)
                {

                    var changeText = change.ToString().Replace(", with reported today.", "."); ;
                    if (changeText.Contains("for the first time"))
                    {
                        string bp = "";
                    }
                    var Processor = new TemplateProcessor(changeText);
                    var tagged = Processor.GetProcessedText();
                    Console.WriteLine($"[{ DateTime.Now}] {tagged} ");
                    //var tParams = new Tweetinvi.Parameters.PublishTweetOptionalParameters()
                    //{
                    //    Medias = new List<IMedia>() { UpdateMedia }
                    //};
                    Tweet.PublishTweet(tagged);
                    System.Threading.Thread.Sleep(10000);
                    lastUpdated = DateTime.Now;
                }
                coll = newColl;
                if (!pubbedAggr || DateTime.Now.Subtract(lastTableDate).TotalMinutes >= globalChartTimeout && changes.Count > 0)
                {
                    if (bool.Parse(bool.FalseString))
                    {
                        data = GetData();
                    }

                    lastTableDate = DateTime.Now;
                    var formatted = $"US #CoronaVirus: To date a total of {dCount.ToString("N0")} total deaths and {cCount.ToString("N0")} total #covid19 cases have been confirmed in the United States.";
                    //if()
                    if (changes.Any(x => x.New.Loc == "China") && cRotwCount != lastCtRowCount)
                        formatted = $"#CoronaVirus Outside of China - {cRotwCount.ToString("N0")} cases and {dRotwCount.ToString("N0")} deaths.\n\nTo date a total of {dCount.ToString("N0")} deaths and {cCount.ToString("N0")} total #covid19 cases have been confirmed worldwide.\n\n#CoronaVirusOutbreak";

                    var Processor = new TemplateProcessor(formatted);
                    formatted = Processor.GetProcessedText();

                    data.Columns[0].ColumnName = $"Locations ({data.Rows.Count})";
                    Console.WriteLine($"[{ DateTime.Now}] {formatted} ");


                    var totalGlobalRow = data.NewRow();
                    totalGlobalRow[0] = "Total";

                    totalGlobalRow[1] = cCount;
                    totalGlobalRow[2] = newCCount;
                    totalGlobalRow[3] = dCount;
                    totalGlobalRow[4] = newDCount;
                    data.Rows.Add(totalGlobalRow);

                    var rows = data.Rows.Cast<DataRow>().ToList();

                   

                    //data.AcceptChanges();
                    var doc = AsposeHelper.GetDataTableDocument(data);
                    var fileNamePng = $"covus-table-{DateTime.Now.ToFileTimeUtc()}.png";
                    var fileNameDocx = $"covus-table-{DateTime.Now.ToFileTimeUtc()}.docx";
                    IMedia media = null;
                    using (var ms = new System.IO.MemoryStream())
                    {
                        doc.Save(ms, Aspose.Words.SaveFormat.Png);
                        doc.Save(fileNamePng, Aspose.Words.SaveFormat.Png);
                        //doc.Save(fileNameDocx, Aspose.Words.SaveFormat.Docx);
                        media = Upload.UploadBinary(ms.ToArray());
                    }
                    var tParams = new Tweetinvi.Parameters.PublishTweetOptionalParameters()
                    {
                        Medias = new List<IMedia>() { media }
                    };

                    Tweet.PublishTweet(formatted, tParams);
                    System.Threading.Thread.Sleep(10000);
                    pubbedAggr = true;
                }
                int diff = (int)DateTime.Now.Subtract(start).TotalMilliseconds;

                if (diff < sleep)
                {
                    var sleepTimeOut = sleep - diff;
                    System.Threading.Thread.Sleep(sleepTimeOut);
                }
                lastCTotal = cCount;
                lastDTotal = dCount;
                lastCtRowCount = cRotwCount;

            }
        }

        private static bool HasBadData(List<ChangedStat> changes)
        {

            var withOld = changes.Where(x => x.Old != null);
            var withoutOld = changes.Where(x => x.Old == null);
            Func<ChangedStat, bool> badWithOldTotal = (x) =>
            {
                if (x.New.TotalCount > 100)
                {
                    return (x.New.TotalCount / x.Old.TotalCount) > 2;
                }
                return false;
            };
            Func<ChangedStat, bool> badWithOldDTotal = (x) =>
            {
                if (x.New.DailyDCount > 20)
                {
                    return (x.New.TotalDCount / x.Old.TotalDCount) > 2;
                }
                return false;
            };
            Func<ChangedStat, bool> badWithoutOldTotal = (x) => x.New.TotalCount > 100;

            Func<ChangedStat, bool> badWithoutOldDTotal = (x) => x.New.DailyDCount > 20;



            var result = withOld.Any(x => badWithOldTotal(x) || badWithOldDTotal(x))
                || withoutOld.Any(x => badWithoutOldTotal(x) || badWithoutOldDTotal(x));
            return result;


        }
    }
}
