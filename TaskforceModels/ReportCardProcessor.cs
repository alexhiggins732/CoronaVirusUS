using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace TaskforceModels
{
    class ReportCardProcessor
    {
        public static void Run()
        {

            var reportCard = ReportCard.GetReportCard();

            var header = @"US Coronavirus Covid-19 State By State Report Card 4/7 [Thread]

Here's a breakdown of how the U.S. and each state are doing compared to the #covid19 outbreak models referenced by the US #Coronavirus Task Force.

Thread: With report card and graphs for each state.";

            var gradingImage = @"C:\Users\alexander.higgins\source\repos\CoronaVirusUS\TaskforceModels\images\model grading.png";
            var tParams = GetParams(gradingImage, null);
            var current = Tweet.PublishTweet(header, tParams);

            var gradingTweet = @"First, here's how each #Coronavirus model was graded:

A+ to B+: Below the models margin of error. 
B to C+: Between the lower bound and mean forecast
C to D+: Between the mean forecast and upper bound.
D to F: Exceeding the model's upper bound.

Here U.S. new deaths have a C.";
            //var gradingImage = @"C:\Users\alexander.higgins\source\repos\CoronaVirusUS\TaskforceModels\images\model grading.png";
            tParams = GetParams(gradingImage, current);
            current = Tweet.PublishTweet(gradingTweet, tParams);

            var reportCardTweet = @"Here is a breakdown of all of the grades assigned to each state's performance compared to the latest April 6th model.

The charts for individual locations follow, including model performance for total #Coronavirus deaths and model performance for new daily #covid19 deaths.";

            var reportImage = @"C:\Users\alexander.higgins\source\repos\CoronaVirusUS\TaskforceModels\images\Report-Card-4-7-2018.png";
            tParams = GetParams(reportImage, current);
            current = Tweet.PublishTweet(reportCardTweet, tParams);


            var usReport = reportCard.Reports.First(x => x.location == "US");
            var stateReports = reportCard.Reports.Where(x => x.location != "US").ToList();
            var l = new List<String>();


            var totalDeathsTweet = usReport.TotalDeathReportTweetText();
        


            tParams = GetParams(usReport.TotalDeathsReportImage, current);
            current = Tweet.PublishTweet(totalDeathsTweet, tParams);

            current = Tweet.GetTweet(1248049719001186304);
            tParams = GetParams(usReport.NewDeathsReportImage, current);
            var newDeathsTweet = usReport.NewDeathReportTweetText();
            current = Tweet.PublishTweet(newDeathsTweet, tParams);

            foreach (var stateReport in stateReports)
            {

                Console.WriteLine(Console.Title = $"Publishing {stateReport.location} Totals");
                totalDeathsTweet = stateReport.TotalDeathReportTweetText();
               
                tParams = GetParams(stateReport.TotalDeathsReportImage, current);
                current = Tweet.PublishTweet(totalDeathsTweet, tParams);
                System.Threading.Thread.Sleep(10000);

                Console.WriteLine(Console.Title = $"Publishing {stateReport.location} New");
                newDeathsTweet = stateReport.NewDeathReportTweetText();
                tParams = GetParams(stateReport.NewDeathsReportImage, current);
                current = Tweet.PublishTweet(newDeathsTweet, tParams);
                System.Threading.Thread.Sleep(10000);
                
            }
        }

        public static PublishTweetOptionalParameters GetParams(string reportImage, ITweet inReplyTo)
        {
            var binary = File.ReadAllBytes(reportImage);
            var media = Upload.UploadBinary(binary);
            var tParams = new PublishTweetOptionalParameters()
            {
                Medias = new List<IMedia>() { media },
                InReplyToTweet = inReplyTo
            };
            return tParams;
        }

        Dictionary<string, string> GetLocations()
        {
            var locations = new Dictionary<string, string>();
            locations.Add("US", "US");
            var states = StateProvider.States();
            foreach (var state in states)
            {
                locations.Add(state.Key, state.Value);
            }
            return locations;
        }
    }

    public class ReportCard
    {
        public List<LocationReport> Reports { get; set; }

        private static string connString = "Data Source=.;Initial Catalog=CVTracker;Integrated Security=true";
        public static ReportCard GetReportCard()
        {
            using (var conn = new SqlConnection(connString))
            {
                List<LocationReport> reports =
                    conn.Query<LocationReport>("spGetModelReport", commandType: CommandType.StoredProcedure)
                    .ToList();
                var result = new ReportCard
                {
                    Reports = reports
                };
                return result;
            }

        }
    }

    public class LocationReport
    {
        public string location { get; set; }
        public string NewDeathsGrade { get; set; }
        public string DeathsGrade { get; set; }
        public int newdeaths { get; set; }
        public int deaths { get; set; }
        public int confirmed { get; set; }
        public decimal deaths_lower { get; set; }
        public decimal deaths_mean { get; set; }
        public decimal deaths_upper { get; set; }
        public decimal totdea_lower { get; set; }
        public decimal totdea_mean { get; set; }
        public decimal totdea_upper { get; set; }

        private static string root = @"C:\Users\alexander.higgins\source\repos\CoronaVirusUS\TaskforceModels\images";
        public string TotalDeathsReportImage => Path.Combine(root, $"{location}-Total-Deaths.png");
        public string NewDeathsReportImage => Path.Combine(root, $"{location}-New-Deaths.png");

        public string NewDeathReportTweetText()
        {
            var text = $@"{location} New #Coronavirus #Covid19 Deaths Grade: {NewDeathsGrade}

– New Deaths Reported: {newdeaths.ToString("N0")}
– Model Lower Bound: {((int)deaths_lower).ToString("N0")}
– Model Mean: {((int)deaths_mean).ToString("N0")}
– Model Upper Bound: {((int)deaths_upper).ToString("N0")}
";
            return text;
        }
        public string TotalDeathReportTweetText()
        {
            var text = $@"{location} Total #Coronavirus #Covid19 Deaths Grade: {DeathsGrade}

– Total Deaths Reported: {deaths.ToString("N0")}
– Model Lower Bound: {((int)totdea_lower).ToString("N0")}
– Model Mean: {((int)totdea_mean).ToString("N0")}
– Model Upper Bound: {((int)totdea_upper).ToString("N0")}
";
            return text;
        }
    }

}
