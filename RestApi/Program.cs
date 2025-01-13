using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RESTAPI.Data;
using RESTAPI.Models;
using RESTAPI.PeliculasMappers;
using RESTAPI.Repositorio;
using RESTAPI.Respositorio.IRepositorio;

var builder = WebApplication.CreateBuilder(args);


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7022, listenOptions =>
    {
        // Usar el archivo .crt y .key para habilitar HTTPS
        listenOptions.UseHttps("/home/dell/certs/mycert.pfx", "");
    });
    options.ListenAnyIP(5212);
});


// Add services to the container.
builder.Services.AddDbContext<ApplicationDBContext>(opciones => 
                                opciones.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql")));

//Soporte para autenticacion con .NET Identity
builder.Services.AddIdentity<AppUsuario, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();


//Soporte para cache
builder.Services.AddResponseCaching();

//Agregamos los repositorios
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");

//Soporte para versiones
var apiVersioningBuilder = builder.Services.AddApiVersioning(opcion =>
{
    opcion.AssumeDefaultVersionWhenUnspecified = true;
    opcion.DefaultApiVersion = new ApiVersion(1, 0);
    opcion.ReportApiVersions = true;
    opcion.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version")
    );
});

apiVersioningBuilder.AddApiExplorer(
    opciones =>
    {
        opciones.GroupNameFormat = "'v'VVV";
    }
    );

//Agregamos el AutoMapper
builder.Services.AddAutoMapper(typeof(PeliculasMappers));

//Aqui se configura la Autenticacion
builder.Services.AddAuthentication(
    x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
    ).AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    }
    );
 
builder.Services.AddControllers(option =>
{
    //Perfil de cache ste s eusa para poder usarlo globalmente
    option.CacheProfiles.Add("Defecto20Seg", new CacheProfile(){Duration = 20});
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Ingrese el token",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
    }
);

//Soporte para CORS
builder.Services.AddCors(p => p.AddPolicy("PoliticaCors", build =>
{
    build.WithOrigins("https://localhost:3223").AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.UseHttpsRedirection();
//Soporte para CORS
app.UseCors("PoliticaCors");
//Soporte para autenticacion
app.UseAuthentication();
app.UseAuthorization();

app.Run();

