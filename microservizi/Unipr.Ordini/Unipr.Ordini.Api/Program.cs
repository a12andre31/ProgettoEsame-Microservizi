using Microsoft.EntityFrameworkCore;
using Ordini.Business;
using Ordini.Business.Abstraction;
using Ordini.Business.Kafka;
using Ordini.Business.Profiles;
using Ordini.Repository;
using Ordini.Repository.Abstraction;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<OrdiniDbContext>(options =>
    options.UseSqlServer("name=ConnectionStrings:OrdiniDbContext", b => b.MigrationsAssembly("Unipr.Ordini.Api")));

// Dependency injection
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

// Pattern Observer (Reattività)
builder.Services.AddSingleton(p => ActivatorUtilities.CreateInstance<Subject>(p));
builder.Services.AddSingleton<IUniprOrdiniObservable>(p => p.GetRequiredService<Subject>());
builder.Services.AddSingleton<IUniprOrdiniObserver>(p => p.GetRequiredService<Subject>());

// AutoMapper
builder.Services.AddAutoMapper(typeof(AssemblyMarker));

// Kafka Producer
builder.Services.AddKafkaProducerServiceWithSubscription<KafkaTopicsOutput, ProducerServiceWithSubscription>(builder.Configuration);

// Background Service per la transazione SAGA Ripetibile
builder.Services.AddHostedService<SagaTimeoutService>();

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