using Magazzino.Business;
using Magazzino.Business.Abstraction;
using Magazzino.Business.Kafka;
using Magazzino.Repository;
using Magazzino.Repository.Abstraction;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<MagazzinoDbContext>(options =>
    options.UseSqlServer("name=ConnectionStrings:MagazzinoDbContext", b => b.MigrationsAssembly("Unipr.Magazzino.Api")));

// Dependency injection
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddScoped<IBusiness, Business>();

// Pattern Observer 
builder.Services.AddSingleton(p => ActivatorUtilities.CreateInstance<Subject>(p));
builder.Services.AddSingleton<IUniprMagazzinoObservable>(p => p.GetRequiredService<Subject>());
builder.Services.AddSingleton<IUniprMagazzinoObserver>(p => p.GetRequiredService<Subject>());

// Kafka Producer
builder.Services.AddKafkaProducerServiceWithSubscription<KafkaTopicsOutput, ProducerServiceWithSubscription>(builder.Configuration);

// Kafka Consumer
builder.Services.AddKafkaConsumerService<KafkaTopicsOutput, ConsumerHandlerFactory>(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Condivisione Chiavi Portatile
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"../SharedCookieKeys"))
    .SetApplicationName("SharedCookieApp");

// Lettura del Cookie di Identity
builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MagazzinoDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();