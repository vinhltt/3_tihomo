using System.Data;
using ExcelDataReader;

namespace ExcelApi.Services
{
    /// <summary>
    /// Service for processing Excel files (XLSX format only)
    /// </summary>
    public class ExcelProcessingService(ILogger<ExcelProcessingService> logger) : IExcelProcessingService
    {
        private const string XLSX_EXTENSION = ".xlsx";
        private const int DEFAULT_HEADER_ROW_INDEX = 24; // Excel row 25 (0-based index)
        private const string DEFAULT_END_MARKER = "Total Debit Transaction";

        /// <inheritdoc/>
        public async Task<List<Dictionary<string, string>>> ProcessExcelFileAsync(
            IFormFile file,
            string? password,
            List<string>? headers = null,
            int? headerRowIndex = null,
            string? endMarker = null)
        {
            try
            {
                // Verify file extension is XLSX
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension != XLSX_EXTENSION)
                {
                    throw new InvalidOperationException(
                        "Only XLSX format is supported. Please convert your Excel file to XLSX format.");
                }

                // Process Excel file with ExcelDataReader
                return await Task.Run(() => ProcessExcelFileWithDataReader(
                    file,
                    password,
                    headers,
                    headerRowIndex ?? DEFAULT_HEADER_ROW_INDEX,
                    endMarker ?? DEFAULT_END_MARKER));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Excel file: {Message}", ex.Message);
                throw;
            }
        }

        private List<Dictionary<string, string>> ProcessExcelFileWithDataReader(
            IFormFile file,
            string? password,
            List<string>? clientHeaders,
            int headerRowIndex,
            string endMarker)
        {
            using var stream = file.OpenReadStream();
            using var reader = CreateExcelReader(stream, password);

            // Convert Excel data to DataSet
            var result = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = false // Don't use the first row as headers since we'll handle it manually
                }
            });

            // Get the first worksheet
            if (result.Tables.Count == 0)
            {
                throw new InvalidOperationException("Excel file doesn't contain any worksheets");
            }

            var dataTable = result.Tables[0];

            // Get headers (either from client or from Excel file)
            List<string> headers;
            if (clientHeaders is { Count: > 0 })
            {
                logger.LogInformation("Using client-provided headers instead of reading from Excel");
                headers = clientHeaders;
            }
            else
            {
                logger.LogInformation("Reading headers from row {HeaderRowNumber} (index {HeaderRowIndex})",
                    headerRowIndex + 1, headerRowIndex);

                // Check if header row exists
                if (dataTable.Rows.Count <= headerRowIndex)
                {
                    throw new InvalidOperationException($"Header row not found at row {headerRowIndex + 1} (index {headerRowIndex})");
                }

                headers = ExtractHeaders(dataTable.Rows[headerRowIndex]);
            }

            return ExtractData(dataTable, headers, headerRowIndex, endMarker);
        }

        private static IExcelDataReader CreateExcelReader(Stream stream, string? password)
        {
            var configuration = new ExcelReaderConfiguration
            {
                Password = password
            };

            return ExcelReaderFactory.CreateReader(stream, configuration);
        }

        private static List<string> ExtractHeaders(DataRow headerRow)
        {
            var headers = new List<string>();

            for (var i = 0; i < headerRow.ItemArray.Length; i++)
            {
                var value = headerRow[i].ToString() ?? string.Empty;
                headers.Add(value);
            }

            return headers;
        }

        private List<Dictionary<string, string>> ExtractData(
            DataTable dataTable,
            List<string> headers,
            int headerRowIndex,
            string endMarker)
        {
            var data = new List<Dictionary<string, string>>();

            // Start from the row after the header
            for (var rowIndex = headerRowIndex + 1; rowIndex < dataTable.Rows.Count; rowIndex++)
            {
                var row = dataTable.Rows[rowIndex];
                var rowData = new Dictionary<string, string>();


                // Check if this cell contains the end marker text
                if (row[0].ToString()?.Contains(endMarker) ?? false)
                {
                    logger.LogInformation("Found end marker '{EndMarker}' at row {RowNumber}. Stopping processing.",
                        endMarker, rowIndex + 1);
                    break;
                }
                for (var colIndex = 0; colIndex < headers.Count && colIndex < row.ItemArray.Length; colIndex++)
                {
                    var cellValue = row[colIndex].ToString() ?? string.Empty;

                    rowData[headers[colIndex]] = cellValue;
                }

                data.Add(rowData);
            }

            return data;
        }
    }
}