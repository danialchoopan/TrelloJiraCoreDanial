using Microsoft.EntityFrameworkCore;
using TrelloJiraCore.Core.Interfaces;
using TrelloJiraCore.Infrastructure.Data;
using TrelloJiraCore.Infrastructure.Repositories;
using TrelloJiraCore.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider");

if (dbProvider == "SqlServer")
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=TrelloJira.db"));
}

builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();
app.MapHub<BoardHub>("/boardHub");

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    if (!context.Boards.Any())
    {
        var board1 = new TrelloJiraCore.Core.Entities.Board { Title = "توسعه زیرساخت بک‌آند", Description = "مدیریت سرویس‌های هسته و دیتابیس" };
        var board2 = new TrelloJiraCore.Core.Entities.Board { Title = "کمپین بازاریابی تابستانه", Description = "برنامه‌ریزی و اجرای تبلیغات محیطی و دیجیتال" };
        context.Boards.AddRange(board1, board2);
        context.SaveChanges();

        var lists1 = new[] {
            new TrelloJiraCore.Core.Entities.List { Title = "ایده‌ها", BoardId = board1.Id, Order = 1 },
            new TrelloJiraCore.Core.Entities.List { Title = "تایید فنی", BoardId = board1.Id, Order = 2 },
            new TrelloJiraCore.Core.Entities.List { Title = "در حال کدنویسی", BoardId = board1.Id, Order = 3 },
            new TrelloJiraCore.Core.Entities.List { Title = "تست نهایی", BoardId = board1.Id, Order = 4 },
            new TrelloJiraCore.Core.Entities.List { Title = "اتمام", BoardId = board1.Id, Order = 5 }
        };
        context.Lists.AddRange(lists1);
        context.SaveChanges();

        context.Cards.AddRange(
            new TrelloJiraCore.Core.Entities.Card { Title = "پیاده‌سازی JWT", Description = "سیستم احراز هویت مبتنی بر توکن", ListId = lists1[2].Id, Priority = TrelloJiraCore.Core.Enums.Priority.Urgent, Assignee = "دانیال", Order = 1 },
            new TrelloJiraCore.Core.Entities.Card { Title = "بهینه‌سازی کوئری‌ها", Description = "افزایش سرعت لود بوردهای سنگین", ListId = lists1[0].Id, Priority = TrelloJiraCore.Core.Enums.Priority.High, Assignee = "علی", Order = 1 },
            new TrelloJiraCore.Core.Entities.Card { Title = "مستندات Swagger", Description = "بروزرسانی توضیحات API", ListId = lists1[4].Id, Priority = TrelloJiraCore.Core.Enums.Priority.Low, Assignee = "سارا", Order = 1 },
            new TrelloJiraCore.Core.Entities.Card { Title = "هماهنگی با تیم موبایل", Description = "بررسی هاب SignalR برای اندروید", ListId = lists1[1].Id, Priority = TrelloJiraCore.Core.Enums.Priority.Medium, Assignee = "رضا", Order = 1 }
        );

        var lists2 = new[] {
            new TrelloJiraCore.Core.Entities.List { Title = "در انتظار", BoardId = board2.Id, Order = 1 },
            new TrelloJiraCore.Core.Entities.List { Title = "اجرا", BoardId = board2.Id, Order = 2 },
            new TrelloJiraCore.Core.Entities.List { Title = "بایگانی", BoardId = board2.Id, Order = 3 }
        };
        context.Lists.AddRange(lists2);
        context.SaveChanges();

        context.ActivityLogs.Add(new TrelloJiraCore.Core.Entities.ActivityLog {
            User = "سیستم",
            Action = "دیتابیس با موفقیت مقداردهی اولیه شد",
            BoardId = board1.Id,
            Timestamp = DateTime.UtcNow
        });

        context.SaveChanges();
    }
}

app.Run();
