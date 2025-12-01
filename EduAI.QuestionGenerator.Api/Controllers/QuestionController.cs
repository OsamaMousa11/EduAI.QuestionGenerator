using EduAI.QuestionGenerator.Core.DTO;
using EduAI.QuestionGenerator.Core.IServiceContract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EduAI.QuestionGenerator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class QuizController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly ILogger<QuizController> _logger;

        private static readonly string[] AllowedExtensions =
        {
            ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".txt", ".pptx", ".ppt"
        };

        public QuizController(
            IQuestionService questionService,
            ILogger<QuizController> logger)
        {
            _questionService = questionService;
            _logger = logger;
        }

        /// <summary>
        /// Generates a quiz from an uploaded file (PDF, Word, Excel, PowerPoint, or TXT)
        /// </summary>
        /// <param name="request">File and quiz generation parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated quiz with questions</returns>
        [HttpPost("generate")]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB max
        [ProducesResponseType(typeof(QuestionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateQuiz(
            [FromForm] FileUploadRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate file presence
                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(new
                    {
                        error = "File is required and cannot be empty.",
                        allowedFormats = AllowedExtensions
                    });
                }

                // Validate file extension
                var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    return StatusCode(
                        StatusCodes.Status415UnsupportedMediaType,
                        new
                        {
                            error = $"Unsupported file type '{ext}'.",
                            allowedFormats = AllowedExtensions,
                            yourFile = request.File.FileName
                        });
                }

                // Validate file size
                if (request.File.Length > 50 * 1024 * 1024)
                {
                    return BadRequest(new
                    {
                        error = "File size exceeds the maximum limit of 50 MB.",
                        yourFileSize = $"{request.File.Length / (1024.0 * 1024.0):F2} MB"
                    });
                }

                _logger.LogInformation(
                    "Generating quiz from file: {FileName} ({Size} KB), Questions: {Count}",
                    request.File.FileName,
                    request.File.Length / 1024,
                    request.NumberOfQuestions);

                // Generate quiz
                var quiz = await _questionService.GenerateQuestionsFromFileAsync(request, cancellationToken);

                _logger.LogInformation(
                    "Quiz generated successfully. QuizId: {QuizId}, Questions: {Count}",
                    quiz.QuizId,
                    quiz.TotalQuestions);

                return Ok(quiz);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning(ex, "Unsupported file type: {FileName}", request.File?.FileName);
                return StatusCode(
                    StatusCodes.Status415UnsupportedMediaType,
                    new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for file: {FileName}", request.File?.FileName);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operation failed for file: {FileName}", request.File?.FileName);
                return BadRequest(new { error = ex.Message });
            }
            catch (TimeoutException ex)
            {
                _logger.LogWarning(ex, "Request timed out for file: {FileName}", request.File?.FileName);
                return StatusCode(
                    StatusCodes.Status408RequestTimeout,
                    new { error = "The request took too long to process. Please try with a smaller file or fewer questions." });
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Request was cancelled for file: {FileName}", request.File?.FileName);
                return StatusCode(
                    StatusCodes.Status499ClientClosedRequest,
                    new { error = "Request was cancelled." });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while generating quiz for file: {FileName}",
                    request.File?.FileName);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "An unexpected error occurred. Please try again later." });
            }
        }

    
    }
}