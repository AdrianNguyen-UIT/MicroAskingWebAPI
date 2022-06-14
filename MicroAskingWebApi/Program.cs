using BlingFire;
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
builder.Services.AddSingleton<ISingletonService, LuceneSearcher>();
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

//string input = "Autophobia, also called monophobia, isolophobia, or eremophobia, is the specific phobia of isolation. I saw a girl with a telescope. Я увидел девушку с телескопом.";
//byte[] inBytes = System.Text.Encoding.UTF8.GetBytes(input);

//var h3 = BlingFireUtils.LoadModel("./bin/Debug/net6.0/xlm_roberta_base.bin");
//var h3_i2w = BlingFireUtils.LoadModel("./bin/Debug/net6.0/xlm_roberta_base.i2w");
//Console.WriteLine(String.Format("Model handle: {0}", h3));
//int[] Ids = new int[128];

//Console.WriteLine("Model: xlm_roberta_base");
//var outputCount = BlingFireUtils2.TextToIds(h3, inBytes, inBytes.Length, Ids, Ids.Length, 0);
//Console.WriteLine(String.Format("return length: {0}", outputCount));
//if (outputCount >= 0)
//{
//    Array.Resize(ref Ids, outputCount);
//    Console.WriteLine(String.Format("return array: [{0}]", string.Join(", ", Ids)));
//    Array.Resize(ref Ids, 128);

//    Console.WriteLine("IdsToText: " + BlingFireUtils2.IdsToText(h3_i2w, Ids));
//}


app.Run();