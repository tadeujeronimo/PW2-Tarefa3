using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Data;
using Projeto1_IF.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// db_IFContext usa a mesma string que ApplicationDbContext (banco db_IF).
// Se existir uma entrada "db_IFContext" no appsettings.json ela é ignorada
// aqui — ambos os contextos precisam apontar para o mesmo banco.
builder.Services.AddDbContext<db_IFContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Garante que as roles (autorizações) usadas pelo sistema existam no banco.
// Médico e Nutricionista são atribuídas no autocadastro; as três de
// Gerente* precisam ser associadas manualmente a um usuário direto no
// banco de dados (AspNetUsers / AspNetUserRoles), conforme pedido no
// enunciado do trabalho final.
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Medico", "Nutricionista", "GerenteMedico", "GerenteNutricionista", "GerenteGeral" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Seed dos planos caso a tabela esteja vazia (o SQL do banco não
    // incluiu INSERTs para tbPlano). Os dois primeiros são para Médico
    // e os dois últimos para Nutricionista, facilitando o filtro no
    // cadastro (bônus do enunciado do trabalho final).
    var db = scope.ServiceProvider.GetRequiredService<db_IFContext>();
    if (!db.TbPlano.Any())
    {
        db.TbPlano.AddRange(
            new TbPlano { Nome = "Médico Total",    Validade = 12, Valor = 500m },
            new TbPlano { Nome = "Médico Parcial",  Validade = 6,  Valor = 300m },
            new TbPlano { Nome = "Nutricional Total",  Validade = 12, Valor = 450m },
            new TbPlano { Nome = "Nutricional Parcial", Validade = 6, Valor = 250m }
        );
        await db.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
