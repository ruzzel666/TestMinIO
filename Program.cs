
using Minio;
using TestMinIO.Services;

namespace TestMinIO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddSingleton(_ =>
            {
                return new MinioClient()
                    .WithEndpoint("localhost:9000")
                    .WithCredentials("minio_access_key", "minio_secret_key")
                    .WithSSL(false) // только для локальной разработки
                    .Build();
            });

            builder.Services.AddScoped<FileStorageService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
