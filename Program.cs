using System.Reflection;
using booking_api.Context;
using booking_api.Interfaces;
using booking_api.Mapping;
using booking_api.Seed;
using booking_api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Services.AddControllers();
builder.Services.AddDbContextPool<BookingContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("BookingContext")));
builder.Services.AddScoped<IWorkspacesService, WorkspacesService>();
builder.Services.AddScoped<IBookingsService, BookingsService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

var corsOrigin = builder.Configuration["CorsOrigin"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.WithOrigins(corsOrigin!)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookingContext>();
    context.Database.Migrate();
    DbSeeding.SeedData(context);
}

app.MapControllers();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();