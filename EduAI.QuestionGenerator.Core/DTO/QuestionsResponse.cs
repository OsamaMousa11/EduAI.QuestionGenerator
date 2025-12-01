using EduAI.QuestionGenerator.Core.Enumration;
using System;
using System.Collections.Generic;

namespace EduAI.QuestionGenerator.Core.DTO
{
    public class QuestionsResponse
    {
        public string QuizId { get; set; } = Guid.NewGuid().ToString("N");
        public string LectureTitle { get; set; } = "Untitled Lecture";
        public int TotalQuestions { get; set; }
        public string Language { get; set; } = "en";
        public string GeneratedAt { get; set; } = DateTime.UtcNow.ToString("o");

        public List<QuestionType> RequestedQuestionTypes { get; set; } = new();
        public List<DifficultyLevel> RequestedDifficultyLevels { get; set; } = new();

        public List<QuestionItem> Questions { get; set; } = new();
    }
}