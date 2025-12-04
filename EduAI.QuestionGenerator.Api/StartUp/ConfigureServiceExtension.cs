
using EduAI.QuestionGenerator.Core.IServiceContract;
using EduAI.QuestionGenerator.Core.Services;
using EduAI.QuestionGenerator.Infrastructure.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

namespace EduAI.QuestionGenerator.Api.StartUp
{
    public static class ConfigureServiceExtension
    {

        public static IServiceCollection ServiceConfiguration(this IServiceCollection Services, IConfiguration Configuration)
        {



            Services.AddControllers();
            Services.AddEndpointsApiExplorer();
            Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "LectureQuestionController API",
                    Version = "v1",
                });
            });
            Services.Configure<FormOptions>(options =>
             {
                 options.MultipartBodyLengthLimit = 50 * 1024 * 1024; 
             });
            Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
            Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            Services.AddHttpClient<IAiService, AiService>();
            Services.AddScoped<IFileTextExtractor, FileTextExtractor>();
            Services.AddScoped<IQuestionService, QuestionGenerationService>();

     
            return Services;
        }
    }
}
