using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using mozgovichok.Controllers;
using mozgovichok.Models;
using mozgovichok.Services.Authorization;
using mozgovichok.Services.Autorization;
using mozgovichok.Services.Courses;
using mozgovichok.Services.News;
using mozgovichok.Services.Organisations;
using mozgovichok.Services.Users;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();


//options => options.AddPolicy("AllowAll", builder => builder
//                .AllowAnyOrigin()
//                .AllowAnyHeader()
//                .AllowAnyMethod())
//           );

// Add services to the container.
builder.Services.Configure<UsersDBSettings> (builder.Configuration.GetSection("MozgovichokDatabase"));

//builder.Services.AddSingleton<AdminsService>();
//builder.Services.AddSingleton<CuratorsService>();
//builder.Services.AddSingleton<SpecialistsService>();


builder.Services.AddSingleton<AuthorizationsService>();
builder.Services.AddSingleton<PupilsService>();
builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<ArchivedPupilsService>();
builder.Services.AddSingleton<OrganisationsService>();
builder.Services.AddSingleton<TariffsService>();
builder.Services.AddSingleton<OrdersService>();
builder.Services.AddSingleton<ExercisesService>();
builder.Services.AddSingleton<CoursesService>();
builder.Services.AddSingleton<NewsService>();


//builder.Services.AddScoped<JWTHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtSettings = builder.Configuration.GetSection("JWTSettings");
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://localhost:44304",
            ValidAudience = "https://localhost:44304",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345555555555555555"))
        };
    });

builder.Services.AddAuthorization(options =>
{
    //options.AddPolicy("AdminAccess", policy =>
    //{
    //    policy.RequireClaim(ClaimTypes.Role, "admin");
    //});
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// настраиваем CORS
app.UseCors(builder => builder
                        //.WithOrigins("https://stage.vr-mozg.ru", "http://stage.vr-mozg.ru")
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());



app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
