using BattleShipAPI.Adapter.Logs;
using BattleShipAPI.Facade;
using BattleShipAPI.Hubs;
using BattleShipAPI.Notifications;

namespace BattleShipAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(opts =>
            {
                opts.AddPolicy("reactApp", builder =>
                {
                    builder.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            builder.Services.AddSingleton<ConsoleLoggerAdapter>();
            builder.Services.AddSingleton<FileLoggerAdapter>(provider => new FileLoggerAdapter("GameHub_logs.txt"));

            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<GameFacade>();
            builder.Services.AddSingleton<CommandFacade>();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();
            
            app.MapControllers();

            app.MapHub<GameHub>("/Game");

            app.UseCors("reactApp");

            app.Run();
        }
    }
}
