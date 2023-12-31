namespace RetryTest
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Http;
    using System.Xml.Schema;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddRouting();
            builder.Services.AddLogging();

            //builder.Services.AddHttpClient<HttpGetter>()
            //    .AddRetries();
            builder.Services.AddControllers();

            //builder.Services.AddSingleton<HttpGetter>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}