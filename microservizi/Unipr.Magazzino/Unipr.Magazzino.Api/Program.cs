using Microsoft.EntityFrameworkCore;
using Magazzino.Business;
using Magazzino.Business.Abstraction;
using Magazzino.Business.Kafka;
using Magazzino.Repository;
using Magazzino.Repository.Abstraction;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<MagazzinoDbContext>(options =>
    options.UseSqlServer("name=ConnectionStrings:MagazzinoDbContext", b => b.MigrationsAssembly("Unipr.Magazzino.Api")));

// Dependency injection
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

// Pattern Observer (Reattività)
builder.Services.AddSingleton(p => ActivatorUtilities.CreateInstance<Subject>(p));
builder.Services.AddSingleton<IUniprMagazzinoObservable>(p => p.GetRequiredService<Subject>());
builder.Services.AddSingleton<IUniprMagazzinoObserver>(p => p.GetRequiredService<Subject>());

// Kafka Producer
builder.Services.AddKafkaProducerServiceWithSubscription<KafkaTopicsOutput, ProducerServiceWithSubscription>(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();