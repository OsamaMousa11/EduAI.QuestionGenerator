using EduAI.QuestionGenerator.Core.Enumration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduAI.QuestionGenerator.Core.DTO
{
    public class QuestionItem
    {
        public int QuestionNumber { get; set; }
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public DifficultyLevel Difficulty { get; set; }


        public List<string>? Options { get; set; }


        public List<string> CorrectAnswers { get; set; } = new();


        public string? Explanation { get; set; }

        
        public string QuestionId { get; set; } = Guid.NewGuid().ToString("N");
    }
}
