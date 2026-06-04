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
        var board = new TrelloJiraCore.Core.Entities.Board { Title = "پروژه توسعه TrelloJiraCore", Description = "مدیریت فرآیندهای توسعه سیستم" };
        context.Boards.Add(board);
        context.SaveChanges();

        var list1 = new TrelloJiraCore.Core.Entities.List { Title = "برای انجام", BoardId = board.Id, Order = 1 };
        var list2 = new TrelloJiraCore.Core.Entities.List { Title = "در حال انجام", BoardId = board.Id, Order = 2 };
        var list3 = new TrelloJiraCore.Core.Entities.List { Title = "پایان یافته", BoardId = board.Id, Order = 3 };
        context.Lists.AddRange(list1, list2, list3);
        context.SaveChanges();

        context.Cards.Add(new TrelloJiraCore.Core.Entities.Card
        {
            Title = "طراحی دیتابیس",
            Description = "طراحی جداول اصلی سیستم",
            ListId = list1.Id,
            Priority = TrelloJiraCore.Core.Enums.Priority.High,
            Assignee = "دانیال"
        });
        context.SaveChanges();
    }
}

app.Run();
