using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduAI.QuestionGenerator.Core.Enumration
{
    public enum DifficultyLevel
    {
        [Display(Name = "Easy")]
        Easy = 1,

        [Display(Name = "Medium")]
        Medium = 2,

        [Display(Name = "Hard")]
        Hard = 3,

        [Display(Name = "Expert")]
        Expert = 4
    }
}
