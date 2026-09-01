using Microsoft.AspNetCore.DataProtection;
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

//magazzino
builder.Services.AddUniprMagazzinoClient(builder.Configuration);

// Kafka Producer
builder.Services.AddKafkaProducerServiceWithSubscription<KafkaTopicsOutput, ProducerServiceWithSubscription>(builder.Configuration);

// Kafka Consumer
builder.Services.AddKafkaConsumerService<KafkaTopicsOutput, ConsumerHandlerFactory>(builder.Configuration);

// Background Service per la transazione SAGA Ripetibile
builder.Services.AddHostedService<SagaTimeoutService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Condivisione Chiavi Portatile
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"../SharedCookieKeys"))
    .SetApplicationName("SharedCookieApp");

//Lettura del Cookie di Identity
builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdiniDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();