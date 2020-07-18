using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;

namespace CovidPNIModels
{
    class PNIImporter
    {
        public static void Run(string fileName)
        {
            var filePath = @"c:\users\alexander.higgins\downloads";
            var fullFileName = Path.Combine(filePath, fileName);
            var records = GetData(fullFileName);
            ImportRecords(records);
        }

        private static void ImportRecords(List<PNIRow> records)
        {
            var dt = new DataTable();

            ////AREA,SUB AREA,AGE GROUP,SEASON,WEEK,PERCENT P&I,NUM INFLUENZA DEATHS,NUM PNEUMONIA DEATHS,TOTAL DEATHS,PERCENT COMPLETE
            var m = new PNIRow();

            dt.Columns.Add(nameof(m.AREA), typeof(string));
            dt.Columns.Add(nameof(m.SUB_AREA), typeof(string));
            dt.Columns.Add(nameof(m.AGE_GROUP), typeof(string));
            dt.Columns.Add(nameof(m.SEASON), typeof(string));
            dt.Columns.Add(nameof(m.WEEK), typeof(int));
            dt.Columns.Add(nameof(m.PERCENT_P_I), typeof(float));
            dt.Columns.Add(nameof(m.NUM_INFLUENZA_DEATHS), typeof(int));
            dt.Columns.Add(nameof(m.NUM_PNEUMONIA_DEATHS), typeof(int));
            dt.Columns.Add(nameof(m.TOTAL_DEATHS), typeof(int));
            dt.Columns.Add(nameof(m.PERCENT_COMPLETE), typeof(string));

            foreach (var record in records)
            {
                var row = dt.NewRow();

                row[nameof(m.AREA)] = record.AREA;
                row[nameof(m.SUB_AREA)] = record.SUB_AREA;
                row[nameof(m.AGE_GROUP)] = record.AGE_GROUP;
                row[nameof(m.SEASON)] = record.SEASON;
                row[nameof(m.WEEK)] = record.WEEK;
                row[nameof(m.PERCENT_P_I)] = record.PERCENT_P_I;
                row[nameof(m.NUM_INFLUENZA_DEATHS)] = record.NUM_INFLUENZA_DEATHS;
                row[nameof(m.NUM_PNEUMONIA_DEATHS)] = record.NUM_PNEUMONIA_DEATHS;
                row[nameof(m.TOTAL_DEATHS)] = record.TOTAL_DEATHS;
                row[nameof(m.PERCENT_COMPLETE)] = record.PERCENT_COMPLETE;

                dt.Rows.Add(row);
            }

            using (var conn = new SqlConnection(connString))
            {
                var ts = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
                string destTable = "CDCStateMortData";
                var backup = $"CDCStateMortData_{ts}";
                conn.Execute($"select * into [{backup}] from [{destTable}]");
                conn.Execute($"truncate table [{destTable}]");
                using (var copy = new SqlBulkCopy(conn))
                {
                    conn.Open();
                    copy.DestinationTableName = destTable;
                    try
                    {
                        copy.WriteToServer(dt);
                    }
                    catch (Exception ex)
                    {
                        string bp = ex.Message;
                        throw;
                    }
                   
                }
            }
        }

        static string connString = "Data Source=.;Initial Catalog=CVTracker;Integrated Security=true";
        public static List<PNIRow> GetData(string fileName)
        {
            List<PNIRow> dtoes = null;
            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.TrimOptions = TrimOptions.Trim;
                try
                {
                    csv.Configuration.RegisterClassMap<PNIRowMap>();
                    var records = csv.GetRecords<PNIRow>();
                    dtoes = records.ToList();
                }
                catch (Exception ex)
                {
                    string message = ex.ToString();
                    throw;

                }
            }
            return dtoes;
        }

        internal static void Run()
        {
            var fileName = @"national_state_pni_data_week_17.csv";
            var filePath = @"c:\users\alexander.higgins\downloads";
            var fullFileName = Path.Combine(filePath, fileName);
            var records = GetData(fullFileName);
            ImportRecords(records);
        }
    }
    public class PNIRowMap : ClassMap<PNIRow>
    {
        public PNIRowMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.AREA).Name("AREA");
            Map(m => m.SUB_AREA).Name("SUB AREA");
            Map(m => m.AGE_GROUP).Name("AGE GROUP");
            Map(m => m.SEASON).Name("SEASON");
            Map(m => m.WEEK).Name("WEEK");
            Map(m => m.PERCENT_P_I).Name("PERCENT P&I").ConvertUsing(x => PercentPNIConvert(x));
            Map(m => m.NUM_INFLUENZA_DEATHS).Name("NUM INFLUENZA DEATHS,").ConvertUsing(x => FluDeathConvert(x));
            Map(m => m.NUM_PNEUMONIA_DEATHS).Name("NUM PNEUMONIA DEATHS").ConvertUsing(x => PneuDeathConvert(x));
            Map(m => m.TOTAL_DEATHS).Name("TOTAL DEATHS").ConvertUsing(x => TotalDeathConvert(x));
            Map(m => m.PERCENT_COMPLETE).Name("PERCENT COMPLETE");
        }


        private decimal PercentPNIConvert(IReaderRow x)
        {
            var value = x.GetField("PERCENT P&I");
            if (string.IsNullOrEmpty(value)) return 0m;
            try
            {
                return decimal.Parse(value);
            }
            catch (Exception ex)
            {
                string bp = ex.Message;
                return 0m;
            }
        }
        private int FluDeathConvert(IReaderRow x)
        {
            var value = x.GetField("NUM INFLUENZA DEATHS");
            return parseInt(value);
        }

        private int parseInt(string value)
        {
            value = value.Trim();
            if (value == "Insufficient Data") return 0;
            try
            {
                return int.Parse(value, NumberStyles.AllowThousands);
            }
            catch (Exception ex)
            {
                string bp = ex.Message;
                return 0;
            }
        }

        private int PneuDeathConvert(IReaderRow x)
        {
            var value = x.GetField("NUM PNEUMONIA DEATHS");
            return parseInt(value);
        }
        private int TotalDeathConvert(IReaderRow x)
        {
            var value = x.GetField("TOTAL DEATHS");
            return parseInt(value);
        }
    }
    public class PNIRow
    {
        //AREA,SUB AREA,AGE GROUP,SEASON,WEEK,PERCENT P&I,NUM INFLUENZA DEATHS,NUM PNEUMONIA DEATHS,TOTAL DEATHS,PERCENT COMPLETE
        public string AREA { get; set; }
        public string SUB_AREA { get; set; }
        public string AGE_GROUP { get; set; }
        public string SEASON { get; set; }
        public int WEEK { get; set; }
        public decimal PERCENT_P_I { get; set; }
        public int NUM_INFLUENZA_DEATHS { get; set; }
        public int NUM_PNEUMONIA_DEATHS { get; set; }
        //public int PNIDeaths => NUM_INFLUENZA_DEATHS + NUM_PNEUMONIA_DEATHS;
        public int TOTAL_DEATHS { get; set; }
        public string PERCENT_COMPLETE { get; set; }
    }
}
