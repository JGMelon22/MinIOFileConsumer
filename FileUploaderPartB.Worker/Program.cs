using FileUploaderPartB.Worker;
using FileUploaderPartB.Worker.Infrastructure.Data;
using FileUploaderPartB.Worker.Infrastructure.Repositories;
using FileUploaderPartB.Worker.Interfaces;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddScoped<DapperDbContext>();
builder.Services.AddScoped<IFileRepository, FileRepository>();

var host = builder.Build();
host.Run();
