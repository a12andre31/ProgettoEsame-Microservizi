using Microsoft.EntityFrameworkCore;
using Pagamenti.Business;
using Pagamenti.Business.Abstraction;
using Pagamenti.Business.Kafka;
using Pagamenti.Repository;
using Pagamenti.Repository.Abstraction;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<PagamentiDbContext>(options =>
    options.UseSqlServer("name=ConnectionStrings:PagamentiDbContext", b => b.MigrationsAssembly("Unipr.Pagamenti.Api")));

// Dependency injection
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

// Pattern Observer 
builder.Services.AddSingleton(p => ActivatorUtilities.CreateInstance<Subject>(p));
builder.Services.AddSingleton<IUniprPagamentiObservable>(p => p.GetRequiredService<Subject>());
builder.Services.AddSingleton<IUniprPagamentiObserver>(p => p.GetRequiredService<Subject>());

// Kafka Producer
builder.Services.AddKafkaProducerServiceWithSubscription<KafkaTopicsOutput, ProducerServiceWithSubscription>(builder.Configuration);

// Kafka Consumer
builder.Services.AddKafkaConsumerService<KafkaTopicsOutput, ConsumerHandlerFactory>(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PagamentiDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();