using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StandCarros.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StandCarrosContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("StandCarrosContext") ?? throw new InvalidOperationException("Connection string 'StandCarrosContext' not found.")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<StandCarrosContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; 
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}else{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Car}/{action=Index}/{id?}");

app.MapRazorPages();

// Seed data after app is configured and built
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData(services);
    }
    catch (Exception ex)
    {
        // Handle exceptions
        Console.WriteLine(ex.Message);
    }
}

app.Run();

static async Task SeedData(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Ensure the Admin role exists
    var roleCheck = await roleManager.RoleExistsAsync("Admin");
    if (!roleCheck)
    {
        // Create the admin role
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Create a default admin user
    string adminEmail = "marco10ferreira18@gmail.com";
    string adminPassword = "AdminPassword123!";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new IdentityUser { Email = adminEmail, UserName = adminEmail };
        var result = await userManager.CreateAsync(newAdmin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}
