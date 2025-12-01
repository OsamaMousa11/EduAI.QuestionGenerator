using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduAI.QuestionGenerator.Core.Enumration
{
  
       public enum QuestionType
    {
        [Display(Name = "Multiple Choice")]
        MultipleChoice = 1,

        [Display(Name = "True/False")]
        TrueFalse = 2,

        [Display(Name = "Open-Ended")]
        OpenEnded = 3,

        [Display(Name = "Fill in the Blank")]
        FillInTheBlank = 4,

        [Display(Name = "Matching")]
        Matching = 5,

        [Display(Name = "Short Answer")]
        ShortAnswer = 6
    }

}

