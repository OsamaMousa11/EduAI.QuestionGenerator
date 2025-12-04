using EduAI.QuestionGenerator.Api.StartUp;

var builder = WebApplication.CreateBuilder(args);


builder.Services.ServiceConfiguration(builder.Configuration);



var app = builder.Build();



    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");

    });



app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();
