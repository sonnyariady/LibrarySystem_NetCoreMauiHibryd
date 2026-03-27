using Library.Api.Common.Excel;
using Library.Api.Common.Crud;
using Library.Api.Data;
using Library.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IExcelExporter, ClosedXmlExcelExporter>();
builder.Services.AddScoped<ICrudQueryService<AppDbContext, Author, int>, EfCrudQueryService<AppDbContext, Author, int>>();
builder.Services.AddScoped<ICrudQueryService<AppDbContext, Category, int>, EfCrudQueryService<AppDbContext, Category, int>>();
builder.Services.AddScoped<ICrudQueryService<AppDbContext, Book, int>, EfCrudQueryService<AppDbContext, Book, int>>();
builder.Services.AddScoped<ICrudQueryService<AppDbContext, Member, int>, EfCrudQueryService<AppDbContext, Member, int>>();
builder.Services.AddScoped<ICrudQueryService<AppDbContext, Loan, int>, EfCrudQueryService<AppDbContext, Loan, int>>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Open", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Open");
app.UseHttpsRedirection();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(db);
}

app.Run();
