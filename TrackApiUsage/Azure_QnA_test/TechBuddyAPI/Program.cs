using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Prometheus;
using System.Globalization;
using System.Threading.RateLimiting;
using TechBuddyAPI;
using TechBuddyAPI.Data;
using TechBuddyAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var conStrBuilder = new SqlConnectionStringBuilder(
        builder.Configuration.GetConnectionString("TechBuddyContext"));
conStrBuilder.Password = builder.Configuration["Secrets:ConnectionstringPassword"];
var connection = conStrBuilder.ConnectionString;

//builder.Services.AddDbContext<TechBuddyContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("TechBuddyContext" ?? throw new InvalidOperationException("Connection string 'TechBuddyContext' not found."))));
builder.Services.AddDbContext<TechBuddyContext>(options =>
    options.UseSqlServer(connection));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


//builder.Services.Configure<MyRateLimitOptions>(
//    builder.Configuration.GetSection(MyRateLimitOptions.MyRateLimit));

// Add services to the container.
//var myOptions = new MyRateLimitOptions();
//builder.Configuration.GetSection(MyRateLimitOptions.MyRateLimit).Bind(myOptions);
//var fixedPolicy = "fixed";

//builder.Services.AddRateLimiter(_ =>
//{
//    _.AddFixedWindowLimiter(policyName: fixedPolicy, options =>
//    {
//        options.PermitLimit = myOptions.PermitLimit;
//        options.Window = TimeSpan.FromSeconds(myOptions.Window);
//        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//        options.QueueLimit = myOptions.QueueLimit;
//    });
//});


builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromSeconds(15)
            });
    });
    options.RejectionStatusCode = 429;
    options.OnRejected = async (context, token) =>
    {
        var message = BuildRateLimitResponseMessage(context);

        await context.HttpContext.Response.WriteAsync(message);
    
    };
});

string BuildRateLimitResponseMessage(OnRejectedContext onRejectedContext)
{
    var hostName = onRejectedContext.HttpContext.Request.Headers.Host.ToString();
    var query = onRejectedContext.HttpContext.Request.Path.ToString();
    DateTime dateTime= DateTime.Now;

    string fileName = @"RateLimiterLog.txt";
    try
    {
        using (StreamWriter writer = new StreamWriter(fileName, true))
        {
            writer.Write($"Dato:({dateTime}), ip-adresse:({hostName}), request path:({query})\n");
        }
    }
    catch (Exception exp)
    {
        Console.Write(exp.Message);
    }
    return
        $"du har nået det maksimale antal anmodninger, der er tilladt for ip-adressen ({hostName})\n Request path:({query})";
}


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseRateLimiter();

app.UseRouting();
app.UseHttpMetrics();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(cors => cors
.AllowAnyMethod()
.AllowAnyHeader()
.SetIsOriginAllowed(origin => true)
.AllowCredentials()
);


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapMetrics();

app.Run();
