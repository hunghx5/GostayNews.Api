using Microsoft.EntityFrameworkCore;
using GoStay.Common.Configuration;
using GoStay.DataAccess.DBContext;
using GoStay.DataAccess.Interface;
using GoStay.DataAccess.Repositories;
using GoStay.DataAccess.UnitOfWork;
using Q101.ServiceCollectionExtensions.ServiceCollectionExtensions;
using GoStay.Services.Hotels;
using GoStay.Api.Configurations;
using GoStay.Common.Helpers.Order;
using GoStay.Services;
using GoStay.DataAccess.Entities;
using GoStay.DataDto.Users;
using GoStay.Api.Providers;
using GoStay.Common.Helpers;

var builder = WebApplication.CreateBuilder(args);

// ===================== CONFIG =====================
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

AppConfigs.LoadAll(config);

builder.Services.AddHttpContextAccessor();

// ===================== DB =====================
builder.Services.AddDbContext<CommonDBContext>(options =>
    options.UseSqlServer(AppConfigs.SqlConnection),
    ServiceLifetime.Scoped);

builder.Services.AddTransient(typeof(ICommonRepository<>), typeof(CommonRepository<>));
builder.Services.AddTransient(typeof(ICommonUoW), typeof(CommonUoW));
builder.Services.AddScoped(typeof(IOrderFunction), typeof(OrderFunction));

// ===================== SERVICES =====================
builder.Services.RegisterAssemblyTypesByName(
    typeof(IHotelService).Assembly,
    name => name.EndsWith("Service"))
    .AsScoped()
    .AsImplementedInterfaces()
    .Bind();

builder.Services.AddCommonServices();
builder.Services.Configure<AppSettings>(config.GetSection("AppSettings"));

builder.Services.AddHttpClient<IMyTypedClientServices, MyTypedClientServices>();

// ===================== CORS (FIX LỖI CHÍNH) =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ===================== MVC + SWAGGER =====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

StaticServiceProvider.Provider = app.Services;

// ===================== MIDDLEWARE =====================
app.UseDeveloperExceptionPage();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GoStay Api");
});

// ⚠️ THỨ TỰ CỰC KỲ QUAN TRỌNG
app.UseRouting();

app.UseCors("AllowAll");   // 🔥 BẮT BUỘC PHẢI Ở ĐÂY

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
