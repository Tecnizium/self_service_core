using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using self_service_core.Handler;
using self_service_core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

//CosmosDB Client
builder.Services.AddSingleton<CosmosClient>((s) => {
    var config = s.GetRequiredService<IConfiguration>();
    var connectionString = config["CosmosDb:ConnectionString"];
    var client = new CosmosClient(connectionString);
    return client;
});

//MongoDB Client
builder.Services.AddSingleton<IMongoClient>((s) => {
    var config = s.GetRequiredService<IConfiguration>();
    var connectionString = config["MongoDb:ConnectionString"];
    var client = new MongoClient(connectionString);
    return client;
});

//MemoryCache
builder.Services.AddMemoryCache();

//CosmosDB Service
builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();

//MongoDB Service
builder.Services.AddSingleton<IMongoDbService, MongoDbService>();

//MemoryCache Service
builder.Services.AddSingleton<IMemoryCacheService, MemoryCacheService>();

//JwtToken Service
builder.Services.AddSingleton<JwtTokenService>();

//Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OrderCraft", Version = "v1" });
});

//Add Basic Authentication and JwtBearer Authentication
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null).AddJwtBearer("BearerAuthentication",
        options =>
        {
            options.Audience = "SelfServiceCore";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"]))
            };
        });

//Add Authorization with two policy (Admin and User)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
    options.AddPolicy("User", policy => policy.RequireClaim(ClaimTypes.Role, "User"));
    
    //Add Policy for Basic Authentication
    options.AddPolicy("Basic", policy => policy.AddAuthenticationSchemes("BasicAuthentication").RequireAuthenticatedUser());
    
    //Add Policy for Bearer Authentication
    options.AddPolicy("Bearer", policy => policy.AddAuthenticationSchemes("BearerAuthentication").RequireAuthenticatedUser());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();

app.Run();