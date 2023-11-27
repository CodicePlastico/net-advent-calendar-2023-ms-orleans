using Orleans.Configuration;
using Orleans.Serialization;

var builder = WebApplication.CreateBuilder(args);

// *** ORLEANS REGISTRATION E CONFIGURATION ***
builder.Host.UseOrleans(siloBuilder =>
{
    if (builder.Environment.IsDevelopment())
    {
        siloBuilder.UseLocalhostClustering();
        siloBuilder.AddMemoryGrainStorage("tableorderstorage");
        siloBuilder.UseInMemoryReminderService();
    }
    else
    {
        siloBuilder.Services.AddSerializer(serializerBuilder =>
        {
            serializerBuilder.AddNewtonsoftJsonSerializer(
                isSupported: type => type.Namespace.StartsWith("PizzaPlastica.OrderingSystem.Abstractions"));
        });

        // CREAZIONE DEL CLUSTER PER AMBIENTI DI STAGING / PRODUZIONE
        siloBuilder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "PizzaPlasticaCluster";
            options.ServiceId = "BestOrderingSystemEver";
        })
        .UseAdoNetClustering(options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("SqlOrleans");
            options.Invariant = "System.Data.SqlClient";
        })
        .ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000);

        // REGISTRAZIONE REMINDERS PER AMBIENTI DI STAGING / PRODUZIONE
        siloBuilder.UseAdoNetReminderService(reminderOptions => {
            reminderOptions.ConnectionString = builder.Configuration.GetConnectionString("SqlOrleans");
            reminderOptions.Invariant = "System.Data.SqlClient";
        });

        // REGISTRAZIONE STORAGE PER AMBIENTI DI STAGING / PRODUZIONE
        siloBuilder.AddAdoNetGrainStorage("tableorderstorage", storageOptions =>
        {
            storageOptions.ConnectionString = builder.Configuration.GetConnectionString("SqlOrleans");
            storageOptions.Invariant = "System.Data.SqlClient";
        });
    }
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();