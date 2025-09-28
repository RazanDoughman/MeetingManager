using MeetingManager;
using MeetingManager.RoomFeatures.Service;
using MeetingManager.Rooms.Service;
using MeetingManager.Service;
using MeetingManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

using System.Text;
using MeetingManager.Users.Model;                 // ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// ----------------DB----------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------- Identity -------------
builder.Services.AddIdentityCore<ApplicationUser>(opts =>
{
    opts.User.RequireUniqueEmail = true;
    opts.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddSignInManager<SignInManager<ApplicationUser>>()
.AddDefaultTokenProviders();


builder.Services.Configure<IdentityOptions>(opt =>
{
    opt.SignIn.RequireConfirmedAccount = false;
    opt.SignIn.RequireConfirmedEmail = false;
});

// --------------- JWT ----------------
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
    };
});

// ---------- Authorization ----------
builder.Services.AddAuthorization(options =>
{
    // Everything requires auth unless [AllowAnonymous]
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("CanView", p => p.RequireRole("Admin", "Employee", "Guest"));
    options.AddPolicy("CanBook", p => p.RequireRole("Admin", "Employee"));
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// --------------- CORS --------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("ViteDev", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

//builder.Services.AddScoped<IRoleService, RoleService>();
//builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddScoped<IRoomFeatureService, RoomFeatureService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IAttendeeService, AttendeeService>();
builder.Services.AddScoped<IActionItemService, ActionItemService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();



// -------- Controllers/Swagger -------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "MeetingManager API", Version = "v1" });

    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ----------- Pipeline -------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ViteDev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ------- Seed roles + admin --------
using (var scope = app.Services.CreateScope())
{
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = new[] { "Admin", "Employee", "Guest" };
    foreach (var r in roles)
        if (!await roleMgr.RoleExistsAsync(r))
            await roleMgr.CreateAsync(new IdentityRole(r));

    var admin = await userMgr.FindByEmailAsync("admin@local.test");
    if (admin == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@local.test",
            EmailConfirmed = true,
            FullName = "Admin"
        };
        await userMgr.CreateAsync(adminUser, "Admin#12345"); // change later
        await userMgr.AddToRoleAsync(adminUser, "Admin");
    }
}

app.Run();