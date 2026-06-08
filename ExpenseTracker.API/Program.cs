using ExpenseTracker.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
    
var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı köprümüzü kuruyoruz
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Controller (Garson) altyapısını ekliyoruz
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() //Herhangi bir kaynaktan gelen isteklere izin ver
              .AllowAnyMethod() //Herhangi bir HTTP metoduna izin ver (GET, POST, PUT, DELETE, vb.)
              .AllowAnyHeader();//Herhangi bir HTTP başlığına izin ver
    });

});



// 3. Swagger (Arayüz) test ekranı için gereken servisleri ekliyoruz
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Token'ınızı buraya girin. Örnek: Bearer eyJhbGci...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []

    });
}); 

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
 options.TokenValidationParameters = new TokenValidationParameters
 {
     ValidateIssuerSigningKey = true,
     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
     ValidateIssuer = false,
     ValidateAudience = false
 };
});

var app = builder.Build();

// 4. Uygulama geliştirme aşamasındaysa Swagger ekranını kullanıma açıyoruz
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// 5. Yazdığımız CategoriesController gibi garsonları sahneye çağırıyoruz
app.MapControllers();

app.Run();