using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduAI.QuestionGenerator.Core.IServiceContract
{
    public interface IFileTextExtractor
    {
        Task<string> ExtractTextAsync(IFormFile file, CancellationToken ct = default);
        bool SupportsFileType(IFormFile file);
    }
}
