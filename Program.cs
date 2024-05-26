    using System.Net;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using FluentValidation;
    using FluentValidation.AspNetCore;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.OpenApi.Models;
    using MiniApp1Api.BackgroundServices;
    using MiniApp1Api.BackgroundServices.Models;
    using MiniApp1Api.Configuration;
    using MiniApp1Api.Data;
    using MiniApp1Api.Data.Entities;
    using MiniApp1Api.Data.Enums;
    using MiniApp1Api.Filters;
    using MiniApp1Api.Services;
    using TransferProject.Hubs;

    WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

    builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:5001");

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSignalR();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                new string[] {}
            }
        });
    });


    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddDbContext<TransferProjectDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"));
    });

    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

    builder.Services.AddSingleton<EmailSenderBackgroundService>();
    builder.Services.AddHostedService(provider => provider.GetRequiredService<EmailSenderBackgroundService>());

    builder.Services.AddIdentity<UserApp, IdentityRole>(Opt =>
    {
        Opt.User.RequireUniqueEmail = true;
        Opt.Password.RequireNonAlphanumeric = false;
        Opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(2);
        Opt.Lockout.MaxFailedAccessAttempts = 4;
        Opt.Lockout.AllowedForNewUsers = true;
    }).AddEntityFrameworkStores<TransferProjectDbContext>().AddDefaultTokenProviders();


    builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOption"));
    builder.Services.Configure<Client>(builder.Configuration.GetSection("Clients"));

    builder.Services.AddControllers(options => options.Filters.Add(new ProblemDetailsExceptionFilter()))
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }).AddFluentValidation(fv =>
        {
            fv.ValidatorOptions.CascadeMode = CascadeMode.Stop;
            fv.DisableDataAnnotationsValidation = true;
            fv.RegisterValidatorsFromAssemblyContaining<Program>(); // Güncelleme burada
            fv.ImplicitlyValidateChildProperties = true;
        });

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
    {
        CustomTokenOption? tokenOptions = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
        opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
        {
            ValidIssuer = tokenOptions.Issuer,
            ValidAudience = tokenOptions.Audience[0],
            IssuerSigningKey = SignService.GetSymmetricSecurtiyKey(tokenOptions.SecurityKey),

            ValidateIssuerSigningKey = true,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    WebApplication? app = builder.Build();

    await CreateRoles(app.Services.CreateScope().ServiceProvider);

    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapHub<ExampleTypeSafeHub>("/canlidestek");

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();

async Task CreateRoles(IServiceProvider serviceProvider)
{
    RoleManager<IdentityRole>? roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    UserManager<UserApp> identity = serviceProvider.GetRequiredService<UserManager<UserApp>>();

    string[] roleNames = { UserTypes.User.ToString(), UserTypes.Admin.ToString() };
    foreach (string? roleName in roleNames)
    {
        bool roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    UserApp tunga = new()
    {
        Email = "asiftunga@hotmail.com",
        PhoneNumber = "5335636310",
        FirstName = "Asif Tunga",
        LastName = "Mubarek",
        UserName = ("Asif Tunga" + "asiftunga@hotmail.com").Replace(" ",""),
        LockoutEnabled = false,
        IpAddress = "111.111.111.111"
    };

    UserApp merdan = new()
    {
        Email = "212merdan@gmail.com",
        PhoneNumber = "5522672304",
        FirstName = "Merdan",
        LastName = "Kurbanov",
        UserName = ("Merdan" + "212merdan@gmail.com").Replace(" ",""),
        LockoutEnabled = false,
        IpAddress = "111.111.111.111"
    };

    UserApp? tungaDbRecord = await identity.FindByEmailAsync(tunga.Email);

    UserApp? merdanDbRecord = await identity.FindByEmailAsync(merdan.Email);

    string password = builder.Configuration.GetSection("AdminSettings:Password").Value!;

    if (tungaDbRecord is null)
    {
        await identity.CreateAsync(tunga, password);
        await identity.AddToRoleAsync(tunga, UserTypes.Admin.ToString());
    }

    if (merdanDbRecord is null)
    {
        await identity.CreateAsync(merdan, password);
        await identity.AddToRoleAsync(merdan, UserTypes.Admin.ToString());
    }
}
