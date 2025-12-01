using EduAI.QuestionGenerator.Core.DTO;
using EduAI.QuestionGenerator.Core.Helper;
using EduAI.QuestionGenerator.Core.IServiceContract;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace EduAI.QuestionGenerator.Core.Services
{
    public class QuestionGenerationService : IQuestionService
    {
        private readonly IFileTextExtractor _textExtractor;
        private readonly IAiService _aiService;
        private readonly ILogger<QuestionGenerationService> _logger;

        public QuestionGenerationService(
            IFileTextExtractor textExtractor,
            IAiService aiService,
            ILogger<QuestionGenerationService> logger)
        {
            _textExtractor = textExtractor;
            _aiService = aiService;
            _logger = logger;
        }

        public async Task<QuestionsResponse> GenerateQuestionsFromFileAsync(
            FileUploadRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var file = request.File;
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty or missing");

                // 1. Validate file type
                if (!_textExtractor.SupportsFileType(file))
                    throw new NotSupportedException(
                        $"File format '{Path.GetExtension(file.FileName)}' is not supported.");

                // 2. Validate request model
                ValidationHelper.ValidateModel(request);

                // 3. Extract text from file
                _logger.LogInformation("Extracting text from file: {FileName} ({Size} bytes)",
                    file.FileName, file.Length);

                var extractedText = await _textExtractor.ExtractTextAsync(file, cancellationToken);

                if (string.IsNullOrWhiteSpace(extractedText) || extractedText.Length < 100)
                    throw new InvalidOperationException(
                        "Could not extract sufficient text from the file. File may be empty, corrupted, or image-based.");

                _logger.LogInformation("Extracted {Length} characters from file", extractedText.Length);

                // 4. Truncate if content is too long (to fit in AI context window)
                if (extractedText.Length > 90_000)
                {
                    _logger.LogWarning("Content exceeds 90k chars ({Length}), truncating", extractedText.Length);
                    extractedText = extractedText[..90_000] + "\n\n[... Content truncated for processing ...]";
                }

                // 5. Build AI prompt
                var lectureTitle = Path.GetFileNameWithoutExtension(file.FileName);
                _logger.LogInformation("Building prompt for {Count} questions from lecture: {Title}",
                    request.NumberOfQuestions, lectureTitle);

                var prompt = PromptBuilder.Build(extractedText, request, lectureTitle);

                // 6. Call AI service
                _logger.LogInformation("Sending request to AI service...");
                var rawJson = await _aiService.GenerateContentAsync(prompt, cancellationToken);

                // 7. Clean and parse JSON response
                var cleanJson = CleanJsonResponse(rawJson);
                _logger.LogDebug("Cleaned JSON response length: {Length} chars", cleanJson.Length);

                var response = JsonSerializer.Deserialize<QuestionsResponse>(
                    cleanJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    });

                if (response == null || response.Questions == null || response.Questions.Count == 0)
                {
                    _logger.LogError("Failed to parse AI response. Raw JSON: {Json}", cleanJson);
                    throw new InvalidOperationException("AI returned an invalid or empty response.");
                }

                // 8. Finalize response metadata
                response.QuizId = Guid.NewGuid().ToString("N");
                response.LectureTitle = lectureTitle;
                response.TotalQuestions = response.Questions.Count;
                response.Language = request.Language;
                response.GeneratedAt = DateTime.UtcNow.ToString("o");
                response.RequestedQuestionTypes = request.QuestionTypes;
                response.RequestedDifficultyLevels = request.DifficultyLevels;

                // 9. Number questions and ensure IDs
                for (int i = 0; i < response.Questions.Count; i++)
                {
                    response.Questions[i].QuestionNumber = i + 1;
                    if (string.IsNullOrEmpty(response.Questions[i].QuestionId))
                        response.Questions[i].QuestionId = Guid.NewGuid().ToString("N");
                }

                // 10. Shuffle if requested
                if (request.ShuffleQuestions && response.Questions.Count > 1)
                {
                    _logger.LogInformation("Shuffling {Count} questions", response.Questions.Count);
                    response.Questions = ShuffleQuestions(response.Questions);
                }

                _logger.LogInformation(
                    "Successfully generated quiz: {QuizId} with {Count} questions",
                    response.QuizId,
                    response.TotalQuestions);

                return response;
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning(ex, "Unsupported file type");
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation failed");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Request was cancelled or timed out");
                throw new TimeoutException("The operation timed out", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse AI response as JSON");
                throw new InvalidOperationException("AI returned malformed JSON response", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating questions from file");
                throw;
            }
        }

        #region Private Helper Methods

        private string CleanJsonResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return "{}";

            // Remove markdown code blocks
            json = json.Replace("```json", "").Replace("```", "");

            // Trim whitespace
            json = json.Trim();

            // Find and extract JSON object/array
            var startBrace = json.IndexOf('{');
            var startBracket = json.IndexOf('[');

            int startIndex = -1;
            char endChar = '}';

            if (startBrace >= 0 && (startBracket < 0 || startBrace < startBracket))
            {
                startIndex = startBrace;
                endChar = '}';
            }
            else if (startBracket >= 0)
            {
                startIndex = startBracket;
                endChar = ']';
            }

            if (startIndex > 0)
            {
                json = json.Substring(startIndex);
            }

            var endIndex = json.LastIndexOf(endChar);
            if (endIndex >= 0 && endIndex < json.Length - 1)
            {
                json = json.Substring(0, endIndex + 1);
            }

            return json.Trim();
        }

        private List<QuestionItem> ShuffleQuestions(List<QuestionItem> questions)
        {
            var random = new Random();
            return questions
                .OrderBy(_ => random.Next())
                .Select((q, index) =>
                {
                    q.QuestionNumber = index + 1;
                    return q;
                })
                .ToList();
        }

        #endregion
    }
}