using Aspose.Words;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWords = Aspose.Words;
using Aspose.Words.Tables;

using Aspose.Words;
namespace CoronaVirusUS
{
    public class AsposeHelper
    {
        public static Document GetDataTableDocument(DataTable dataTable)
        {
            Document doc = new Document();

            // We can position where we want the table to be inserted and also specify any extra formatting to be
            // applied onto the table as well.



            // Set font settings

            DocumentBuilder builder = new DocumentBuilder(doc);

            builder.PageSetup.TopMargin = builder.PageSetup.BottomMargin = 10;

            Font font = builder.Font;
            //font.Size = 16;
            //font.Bold = true;

            font.Name = "Calibri";
            font.Color = System.Drawing.Color.Black;


            var rev = builder.ParagraphFormat.StyleIdentifier;
            builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Heading2;
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            builder.Writeln("U.S. Confirmed Coronavirus COVID-19 Cases");
            builder.ParagraphFormat.StyleIdentifier = rev;
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            //font.Underline = Underline.Dash;
            //builder.Font = font;
            // We want to rotate the page landscape as we expect a wide table.
            //doc.FirstSection.PageSetup.Orientation = Orientation.Landscape;

            Table table = ImportTableFromDataTable(builder, dataTable, true);
            //table.StyleIdentifier = StyleIdentifier.MediumList2Accent1;
            //table.StyleIdentifier = StyleIdentifier.ColorfulGridAccent1;
            table.StyleIdentifier = StyleIdentifier.GridTable5DarkAccent1;
            table.StyleIdentifier = StyleIdentifier.GridTable7ColorfulAccent1;
            //table.StyleOptions = TableStyleOptions.FirstRow | TableStyleOptions.RowBands | TableStyleOptions.FirstColumn | TableStyleOptions.ColumnBands;// | TableStyleOptions.LastColumn;

            // For our table we want to remove the heading for the image column.
            //table.FirstRow.LastCell.RemoveAllChildren();
            //builder.Writeln("");
            //builder.Writeln("");
            builder.Font.Size = 10;
            builder.Font.Italic = true;
            builder.Font.Bold = false;
            builder.Write($"Generated {DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")} via ");
            builder.Font.Bold = true;
            builder.Write("@kr3at");
            builder.Font.Bold = false;
            builder.Writeln("");
            builder.Writeln("Sources: US CDC/State and County HHS/DOH aggregated data published by worldometers.info");
            builder.Writeln("For latest data see: https://www.worldometers.info/coronavirus/country/us/");

            doc.UpdateFields();

            return doc;

        }
        public static Table ImportTableFromDataTable(DocumentBuilder builder, DataTable dataTable, bool importColumnHeadings)
        {
            Table table = builder.StartTable();
            var padd = table.TopPadding;
            table.Style.ParagraphFormat.LineSpacing = 10;

            // Check if the names of the columns from the data source are to be included in a header row.
            if (importColumnHeadings)
            {
                var headerSize = builder.Font.Size;
                builder.Font.Size = headerSize - 1;
                // Store the original values of these properties before changing them.
                bool boldValue = builder.Font.Bold;
                ParagraphAlignment paragraphAlignmentValue = builder.ParagraphFormat.Alignment;

                // Format the heading row with the appropriate properties.
                builder.Font.Bold = true;
                builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;

                // Create a new row and insert the name of each column into the first row of the table.
                foreach (DataColumn column in dataTable.Columns)
                {
                    builder.InsertCell();
                    builder.Write(column.ColumnName);
                }

                builder.EndRow();
                builder.Font.Size = headerSize;

                // Restore the original formatting.
                builder.Font.Bold = boldValue;
                builder.ParagraphFormat.Alignment = paragraphAlignmentValue;
            }
            var size = builder.Font.Size;
            builder.Font.Size = size - 3;

            foreach (DataRow dataRow in dataTable.Rows)
            {
                var loc = dataRow[0].ToString();
                switch (loc)
                {
                    case "Diamond Princess Ship":
                    case "Wuhan Repatriated":
                        continue;
                    default:
                        break;
                }
                foreach (object item in dataRow.ItemArray)
                {
                    // Insert a new cell for each object.
                    builder.InsertCell();

                    switch (item.GetType().Name)
                    {
                        case "DateTime":
                            // Define a custom format for dates and times.
                            DateTime dateTime = (DateTime)item;
                            builder.Write(dateTime.ToString("MMMM d, yyyy"));
                            break;
                        default:
                            // By default any other item will be inserted as text.
                            builder.Write(item.ToString());
                            break;
                    }

                }

                // After we insert all the data from the current record we can end the table row.
                builder.EndRow();
            }

            // We have finished inserting all the data from the DataTable, we can end the table.
            builder.EndTable();
            builder.Font.Size = size;
            return table;
        }

    }
}
