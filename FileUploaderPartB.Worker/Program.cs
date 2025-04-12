using FileUploaderPartB.Worker;
using FileUploaderPartB.Worker.Infrastructure.Configurations;
using FileUploaderPartB.Worker.Infrastructure.Data;
using FileUploaderPartB.Worker.Infrastructure.Repositories;
using FileUploaderPartB.Worker.Infrastructure.Services;
using FileUploaderPartB.Worker.Interfaces;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.AddScoped<DapperDbContext>();
builder.Services.AddScoped<IFileRepository, FileRepository>();

builder.Services.AddScoped<IKafkaConsumerService, KafkaConsumerService>();
builder.Services.AddScoped<IS3Service, S3Service>();
builder.Services.AddScoped<ICsvValidatorService, CsvValidatorService>();

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<AmazonS3Configuration>(builder.Configuration.GetSection("S3"));

var host = builder.Build();
host.Run();
