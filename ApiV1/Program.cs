using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
//builder.Services.AddControllers(options =>
//{
//    options.Conventions.Add(new RoutePrefixConvention("api/v1"));
//});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "codesamplez.com",

            ValidateAudience = true,
            ValidAudience = "your-audience",

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("vb00A80clmBsKCzPp_0jvknOY43_lkJW")),

            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

//// <-- MOVE THIS UP BEFORE builder.Build()
//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(policy =>
//    {
//        policy.WithOrigins("https://localhost:5001")
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});

var app = builder.Build();

// Then use CORS middleware
//app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();