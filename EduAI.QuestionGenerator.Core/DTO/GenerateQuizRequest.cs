using EduAI.QuestionGenerator.Core.Enumration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduAI.QuestionGenerator.Core.DTO
{
    public class GenerateQuestionsRequest
    {
        [Required(ErrorMessage = "The number of questions is required")]
        [Range(1, 100, ErrorMessage = "The number of questions must be between 1 and 100")]
      
        public int NumberOfQuestions { get; set; }
        [Required(ErrorMessage = "At least one type of question must be selected")]
        [MinLength(1, ErrorMessage = "At least one type must be selected")]
        public List<string> QuestionTypes { get; set; } = new();

        [Required(ErrorMessage = "At least one difficulty level must be selected")]
        [MinLength(1, ErrorMessage = "At least one difficulty level must be selected")]
        public List<string> DifficultyLevels { get; set; } = new();

        public string? Language { get; set; } = "en";

        public bool IncludeAnswers { get; set; } = true;

        public bool IncludeExplanations { get; set; } = false;
        public bool ShuffleQuestions { get; set; } = true;

    }
}
