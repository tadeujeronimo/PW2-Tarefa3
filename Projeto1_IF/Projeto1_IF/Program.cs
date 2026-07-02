using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Data;
using Projeto1_IF.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<db_IFContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

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

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    async Task SeedGerenteAsync(string username, string email, string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing == null)
        {
            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, "Gerente@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
        else
        {
            if (existing.UserName != email)
            {
                existing.UserName = email;
                existing.NormalizedUserName = email.ToUpperInvariant();
                await userManager.UpdateAsync(existing);
            }

            if (!await userManager.IsInRoleAsync(existing, role))
            {
                await userManager.AddToRoleAsync(existing, role);
            }
        }
    }

    await SeedGerenteAsync("gerente.medico", "gerente.medico@example.com", "GerenteMedico");
    await SeedGerenteAsync("gerente.nutri", "gerente.nutri@example.com", "GerenteNutricionista");
    await SeedGerenteAsync("gerente.geral", "gerente.geral@example.com", "GerenteGeral");

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

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
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
