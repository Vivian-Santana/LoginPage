using LoginPage.Dados;
using LoginPage.Mapeamento;
using LoginPage.Rotas;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped <LoginPageDbContext> ();//injeção da conexão com o banco
builder.Services.AddDbContext<LoginPageDbContext>(); //banco
builder.Services.AddAutoMapper(typeof(ConfiguracaoDeMapeamento));

var app = builder.Build();

// Configure the HTTP request pipeline.
 if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.RotasUsuarios();

app.Run();
