
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using EduAI.QuestionGenerator.Core.IServiceContract;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PdfSharp.Pdf;
using UglyToad.PdfPig;

using System.Text;


namespace EduAI.QuestionGenerator.Core.Services
{
    public class FileTextExtractor : IFileTextExtractor
    {
        private readonly ILogger<FileTextExtractor> _logger;

        public FileTextExtractor(ILogger<FileTextExtractor> logger)
        {
            _logger = logger;
        }

        public bool SupportsFileType(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return ext is ".pdf" or ".docx" or ".doc" or ".xlsx" or ".xls" or ".txt" or ".pptx" or ".ppt";
        }

        public async Task<string> ExtractTextAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";

            return ext switch
            {
                ".pdf" => await ExtractFromPdfAsync(file, ct),
                ".docx" or ".doc" => ExtractFromWord(file),
                ".xlsx" or ".xls" => ExtractFromExcel(file),
                ".txt" => await ExtractFromTxtAsync(file, ct),
                ".pptx" or ".ppt" => ExtractFromPowerPoint(file),
                _ => throw new NotSupportedException($"File type '{ext}' is not supported.")
            };
        }

        private static async Task<string> ExtractFromPdfAsync(IFormFile file, CancellationToken ct)
        {
            await using var stream = file.OpenReadStream();
            using var pdf = UglyToad.PdfPig.PdfDocument.Open(stream);
            var sb = new StringBuilder();

            foreach (var page in pdf.GetPages())
            {
                ct.ThrowIfCancellationRequested();
                sb.AppendLine(page.Text);
            }
            return sb.ToString();
        }

        private static string ExtractFromWord(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var doc = WordprocessingDocument.Open(stream, false);
            return doc.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
        }

        private static string ExtractFromExcel(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var doc = SpreadsheetDocument.Open(stream, false);
            var sb = new StringBuilder();

            foreach (Sheet sheet in doc.WorkbookPart!.Workbook.Sheets!)
            {
                var worksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(sheet.Id!);
                foreach (var cell in worksheetPart.Worksheet.Descendants<Cell>())
                {
                    if (cell.CellValue != null)
                        sb.Append(cell.CellValue.Text + " ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static async Task<string> ExtractFromTxtAsync(IFormFile file, CancellationToken ct)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            return await reader.ReadToEndAsync();
        }

        private static string ExtractFromPowerPoint(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var doc = PresentationDocument.Open(stream, false);
            var sb = new StringBuilder();

            foreach (var slidePart in doc.PresentationPart!.SlideParts)
            {
                foreach (var text in slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>())
                {
                    sb.AppendLine(text.Text);
                }
            }
            return sb.ToString();
        }
    }
}