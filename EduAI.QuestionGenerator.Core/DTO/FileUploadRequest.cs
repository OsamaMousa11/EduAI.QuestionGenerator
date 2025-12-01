using EduAI.QuestionGenerator.Core.Enumration;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduAI.QuestionGenerator.Core.DTO
{
    public class FileUploadRequest
    {
        [Required(ErrorMessage = "File is required")]
        public IFormFile File { get; set; } = null!;

        [Required(ErrorMessage = "Number of questions is required")]
        [Range(1, 100, ErrorMessage = "Number of questions must be between 1 and 100")]
        public int NumberOfQuestions { get; set; }

        [Required(ErrorMessage = "At least one question type is required")]
        [MinLength(1, ErrorMessage = "At least one question type must be selected")]
        public List<QuestionType> QuestionTypes { get; set; } = new();

        [Required(ErrorMessage = "At least one difficulty level is required")]
        [MinLength(1, ErrorMessage = "At least one difficulty level must be selected")]
        public List<DifficultyLevel> DifficultyLevels { get; set; } = new();

        [StringLength(10, ErrorMessage = "Language code must be 10 characters or less")]
        public string Language { get; set; } = "en";

        public bool IncludeAnswers { get; set; } = true;
        public bool IncludeExplanations { get; set; } = false;
        public bool ShuffleQuestions { get; set; } = true;
    }
}