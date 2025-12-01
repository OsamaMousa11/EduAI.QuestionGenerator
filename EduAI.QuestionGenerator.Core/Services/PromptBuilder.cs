using EduAI.QuestionGenerator.Core.DTO;
using System.Text;

namespace EduAI.QuestionGenerator.Core.Services
{
    internal static class PromptBuilder
    {
        public static string Build(string lectureText, FileUploadRequest request, string lectureTitle)
        {
            var types = string.Join(", ", request.QuestionTypes);
            var difficulties = string.Join(", ", request.DifficultyLevels);

            var sb = new StringBuilder();

            sb.AppendLine("You are an expert educational quiz generator for university-level courses.");
            sb.AppendLine();
            sb.AppendLine("**LECTURE INFORMATION:**");
            sb.AppendLine($"- Title: {lectureTitle}");
            sb.AppendLine($"- Language: {request.Language}");
            sb.AppendLine();
            sb.AppendLine("**GENERATION REQUIREMENTS:**");
            sb.AppendLine($"- Total Questions: {request.NumberOfQuestions}");
            sb.AppendLine($"- Question Types: {types}");
            sb.AppendLine($"- Difficulty Levels: {difficulties}");
            sb.AppendLine($"- Include Correct Answers: {(request.IncludeAnswers ? "Yes" : "No")}");
            sb.AppendLine($"- Include Explanations: {(request.IncludeExplanations ? "Yes" : "No")}");
            sb.AppendLine();
            sb.AppendLine("**LECTURE CONTENT:**");
            sb.AppendLine(lectureText);
            sb.AppendLine();
            sb.AppendLine("**INSTRUCTIONS:**");
            sb.AppendLine($"1. Generate EXACTLY {request.NumberOfQuestions} questions based on the lecture content");
            sb.AppendLine("2. Use ONLY the specified question types and difficulty levels");
            sb.AppendLine("3. Distribute questions evenly across types and difficulties");
            sb.AppendLine("4. For MultipleChoice: provide exactly 4 plausible options");
            sb.AppendLine("5. For TrueFalse: provide options [\"True\", \"False\"]");
            sb.AppendLine("6. For other types: set options to null or empty array");
            sb.AppendLine("7. Base ALL questions strictly on the provided lecture content");
            sb.AppendLine();
            sb.AppendLine("**CRITICAL: Return ONLY valid JSON. No markdown, no explanations, no extra text.**");
            sb.AppendLine();
            sb.AppendLine("**REQUIRED JSON FORMAT:**");
            sb.AppendLine("{");
            sb.AppendLine($"  \"quizId\": \"auto-generated\",");
            sb.AppendLine($"  \"lectureTitle\": \"{lectureTitle}\",");
            sb.AppendLine($"  \"totalQuestions\": {request.NumberOfQuestions},");
            sb.AppendLine($"  \"language\": \"{request.Language}\",");
            sb.AppendLine($"  \"generatedAt\": \"{System.DateTime.UtcNow:o}\",");
            sb.AppendLine($"  \"requestedQuestionTypes\": [\"{string.Join("\", \"", request.QuestionTypes)}\"],");
            sb.AppendLine($"  \"requestedDifficultyLevels\": [\"{string.Join("\", \"", request.DifficultyLevels)}\"],");
            sb.AppendLine("  \"questions\": [");
            sb.AppendLine("    {");
            sb.AppendLine("      \"questionNumber\": 1,");
            sb.AppendLine("      \"text\": \"Your question text here?\",");
            sb.AppendLine("      \"type\": \"MultipleChoice\",");
            sb.AppendLine("      \"difficulty\": \"Medium\",");
            sb.AppendLine("      \"options\": [\"Option A\", \"Option B\", \"Option C\", \"Option D\"],");
            sb.AppendLine("      \"correctAnswers\": [\"Option A\"],");
            sb.AppendLine($"      \"explanation\": {(request.IncludeExplanations ? "\"Explanation here\"" : "null")},");
            sb.AppendLine("      \"questionId\": \"unique-id\"");
            sb.AppendLine("    }");
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine($"Generate the quiz in {request.Language} language NOW:");

            return sb.ToString();
        }
    }
}