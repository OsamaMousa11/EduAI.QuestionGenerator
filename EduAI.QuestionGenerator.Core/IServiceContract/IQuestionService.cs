using EduAI.QuestionGenerator.Core.DTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduAI.QuestionGenerator.Core.IServiceContract
{
    public interface IQuestionService
    {
        Task<QuestionsResponse> GenerateQuestionsFromFileAsync(
       FileUploadRequest request,
       CancellationToken cancellationToken = default);
    }
}
