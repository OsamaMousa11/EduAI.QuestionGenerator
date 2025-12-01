using EduAI.QuestionGenerator.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduAI.QuestionGenerator.Core.IServiceContract
{
    public interface IAiService
    {
        Task<string> GenerateContentAsync(string prompt,CancellationToken cancellationToken = default
   );
    }
}
