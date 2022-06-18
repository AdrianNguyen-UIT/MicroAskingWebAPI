using MicroAskingWebApi.Clients;
using MicroAskingWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<InferenceHttpClient>();


// Add  LuceneSearcher as SingletonService
builder.Services.AddSingleton<ISingletonAService, LuceneSearcher>();
builder.Services.AddSingleton<ISingletonBService, SocketService>();

builder.Services.
    AddCors(options => options.AddDefaultPolicy(builder => 
    {
        builder.WithOrigins("http://localhost:3000");
        builder.WithMethods(new string[] { "GET", "POST" });
    }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();