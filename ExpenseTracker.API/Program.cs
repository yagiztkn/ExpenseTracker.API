using ExpenseTracker.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı köprümüzü kuruyoruz
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Controller (Garson) altyapısını ekliyoruz
builder.Services.AddControllers();

// 3. Swagger (Arayüz) test ekranı için gereken servisleri ekliyoruz
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Uygulama geliştirme aşamasındaysa Swagger ekranını kullanıma açıyoruz
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// 5. Yazdığımız CategoriesController gibi garsonları sahneye çağırıyoruz
app.MapControllers();

app.Run();