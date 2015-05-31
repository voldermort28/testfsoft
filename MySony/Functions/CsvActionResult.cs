using System;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MySony.Functions
{
    public sealed class CsvActionResult : FileResult
    {
        private readonly DataTable _dataTable;

        public CsvActionResult(DataTable dataTable) : base("text/csv")
        {
            _dataTable = dataTable;
        }

        protected override void WriteFile(HttpResponseBase response)
        {
            var outputStream = response.OutputStream;
            using (var memoryStream = new MemoryStream())
            {
                WriteDataTable(memoryStream);
                outputStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            }
        }

        private void WriteDataTable(Stream stream)
        {
            var streamWriter = new StreamWriter(stream, Encoding.UTF8);
            WriteHeaderLine(streamWriter);
            streamWriter.WriteLine();
            WriteDataLines(streamWriter);
            streamWriter.Flush();
        }

        private void WriteHeaderLine(StreamWriter streamWriter)
        {
            foreach (DataColumn dataColumn in _dataTable.Columns)
            {
                WriteValue(streamWriter, dataColumn.ColumnName);
            }
        }

        private void WriteDataLines(StreamWriter streamWriter)
        {
            foreach (DataRow dataRow in _dataTable.Rows)
            {
                foreach (DataColumn dataColumn in _dataTable.Columns)
                {
                    WriteValue(streamWriter, dataRow[dataColumn.ColumnName].ToString());
                }
                streamWriter.WriteLine();
            }
        }


        private static void WriteValue(StreamWriter writer, String value)
        {
            if (value.IndexOf('0') == 0) writer.Write("=\"");
            else writer.Write("\"");
            writer.Write(value.Replace("\"", "\"\""));            
            writer.Write("\",");
        }
    }
}