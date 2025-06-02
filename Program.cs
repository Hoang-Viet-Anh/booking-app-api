using booking_api.Context;
using booking_api.Interfaces;
using booking_api.Mapping;
using booking_api.Seed;
using booking_api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContextPool<BookingContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("BookingContext")));
builder.Services.AddScoped<IWorkspacesService, WorkspacesService>();
builder.Services.AddScoped<IBookingsService, BookingsService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:4200");
        policyBuilder.AllowAnyMethod();
        policyBuilder.AllowAnyHeader();
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