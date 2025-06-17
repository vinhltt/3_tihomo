using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ExcelApi.Services;
using ExcelApi.Models;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ExcelApi.Controllers
{
    /// <summary>
    /// Controller for handling Excel file operations
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the ExcelController
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class ExcelController(IExcelProcessingService excelProcessingService) : ControllerBase
    {
        /// <summary>
        /// Uploads and extract data in Excel file
        /// </summary>
        /// <param name="model">The file upload model containing the Excel file and configuration options</param>
        /// <returns>Processed data from the Excel file</returns>
        /// <response code="200">Returns the processed data</response>
        /// <response code="400">If the file is null or empty</response>
        /// <response code="500">If there was an internal error processing the file</response>
        [HttpPost("extract")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Extract data in Excel file",
            Description = "Uploads an Excel file and processes its content. Headers, header row index, and end marker can all be customized."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExtractDataInExcel([FromForm] FileUploadModel model)
        {
            // Handle the case where the file is passed directly in the model
            IFormFile? file = model?.File;

            // If File property is null, check if any file was uploaded with any parameter name
            if (file == null && Request.Form.Files.Count > 0)
            {
                // Get the first uploaded file regardless of parameter name
                file = Request.Form.Files[0];
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                var result = await excelProcessingService.ProcessExcelFileAsync(
                    file, 
                    model?.Password, 
                    model?.Headers,
                    model?.HeaderRowIndex,
                    model?.EndMarker);
                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error processing file: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handles files sent with dynamic field names (for n8n integration)
        /// </summary>
        [HttpPost("extract-dynamic")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Extract data in Excel file with dynamic file naming",
            Description = "Alternative endpoint that supports dynamic file field names for integration with workflow tools like n8n."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExtractDataWithDynamicFileField()
        {
            // Get form data
            var password = Request.Form["Password"].FirstOrDefault();
            List<string> headersText = Request.Form["Headers"].ToList()!;
            var headerRowIndexText = Request.Form["HeaderRowIndex"].FirstOrDefault();
            var endMarker = Request.Form["EndMarker"].FirstOrDefault();
            
            // Parse headers if provided
            List<string> headers = headersText;
            // Parse header row index if provided
            int? headerRowIndex = null;
            if (!string.IsNullOrEmpty(headerRowIndexText) && int.TryParse(headerRowIndexText, out int index))
            {
                headerRowIndex = index;
            }
            
            // Get the first file, regardless of field name
            if (Request.Form.Files.Count == 0)
            {
                return BadRequest("No file uploaded.");
            }
            
            var file = Request.Form.Files[0];
            if (file.Length == 0)
            {
                return BadRequest("Empty file uploaded.");
            }
            
            try
            {
                var result = await excelProcessingService.ProcessExcelFileAsync(
                    file, 
                    password, 
                    headers,
                    headerRowIndex,
                    endMarker);
                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error processing file: {ex.Message}");
            }
        }
    }
}