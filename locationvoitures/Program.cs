using locationvoitures.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
//Cet objet sert à préparer et configurer ton application avant de la lancer.
var builder = WebApplication.CreateBuilder(args);

// sans cette ligne le serveu ne c'est pas quand travaille avec MVC
builder.Services.AddControllersWithViews();

// 2️⃣ Configuration de la connexion MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 35));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion)
);

// 3️⃣ Configuration de l'Identity avec ApplicationUser et rôles
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>() // Active les rôles
.AddEntityFrameworkStores<ApplicationDbContext>(); // Lie Identity au DbContext

var app = builder.Build();

// 4️⃣ Création automatique des rôles au démarrage
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Client", "Admin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// 5️⃣ Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Important : avant UseAuthorization
app.UseAuthorization();

// 6️⃣ Routes MVC et Identity
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Nécessaire pour Identity

app.Run();
