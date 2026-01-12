using Microsoft.EntityFrameworkCore;
using Order.Application.Interfaces;
using Order.Application.Services;
using Order.Infrastructure.Data;
using Order.Infrastructure.Repositories;
using BuildingBlocks.Auth.Extensions;
using BuildingBlocks.Logging.Extensions;
using BuildingBlocks.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging("OrderService");
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddControllers();
builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
//    db.Database.Migrate();
//}

app.UseCorrelationId();
app.UseExceptionHandling();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();