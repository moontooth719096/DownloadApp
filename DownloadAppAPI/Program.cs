using DownloadAppAPI.Interfaces;
using DownloadAppAPI.Services;
using DownloadAppAPI.SignalRHub;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins("https://localhost:44321")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IYoutubeListDownloadService, YoutubeClientVerDownloadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();



app.UseRouting();

app.MapHub<YoutubeDownloadProgressHub>("/youtubeDownloadProgressHub");

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//    endpoints.MapHub<YoutubeDownloadProgressHub>("/youtubeDownloadProgressHub"); //加入這行 代表連接SignalR的路由與配對的Hub
//});

app.Run();
