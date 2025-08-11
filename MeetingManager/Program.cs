using MeetingManager;
using MeetingManager.Roles.Service;
using MeetingManager.RoomFeatures.Service;
using MeetingManager.Rooms.Service;
using MeetingManager.Service;
using MeetingManager.Services.Interfaces;
using MeetingManager.Users.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddScoped<IRoomFeatureService, RoomFeatureService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IAttendeeService, AttendeeService>();
builder.Services.AddScoped<IActionItemService, ActionItemService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();




builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
