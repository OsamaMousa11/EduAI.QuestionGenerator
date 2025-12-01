namespace EduAI.QuestionGenerator.Api.StartUp
{
    public static class ConfigureServiceExtension
    {

        public static IServiceCollection ServiceConfiguration(this IServiceCollection Services, IConfiguration Configuration)
        {
            return Services;
        }
    }
}
