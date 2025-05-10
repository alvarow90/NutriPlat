// NutriPlat.WebApp/Program.cs

// Crear el constructor de la aplicación web.
var builder = WebApplication.CreateBuilder(args);

// --- Configuración de Servicios ---

// 1. Agregar servicios para controladores y vistas (MVC).
// Esto es necesario para que funcionen tus Controllers y las Vistas Razor.
builder.Services.AddControllersWithViews();

// 2. (Opcional) Configurar Razor Pages si también las vas a usar.
// builder.Services.AddRazorPages();

// 3. (Opcional) Configurar servicios de sesión si los necesitas.
// builder.Services.AddDistributedMemoryCache(); // Necesario para el proveedor de sesión en memoria
// builder.Services.AddSession(options =>
// {
//     options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de inactividad de la sesión
//     options.Cookie.HttpOnly = true; // La cookie de sesión no es accesible por JavaScript del lado del cliente
//     options.Cookie.IsEssential = true; // Marca la cookie como esencial para GDPR
// });

// 4. (Opcional) Configurar HttpClientFactory si vas a hacer muchas llamadas a tu API desde el backend de WebApp.
//    Aunque para el login que hicimos, la llamada se hace desde JavaScript del lado del cliente.
// builder.Services.AddHttpClient();


// --- Construir la Aplicación ---
var app = builder.Build();


// --- Configurar el Pipeline de Middleware HTTP ---

// 1. Manejo de Excepciones y Errores.
if (!app.Environment.IsDevelopment())
{
    // En producción, usar una página de error personalizada.
    // Esto redirige al usuario a la acción "Error" del "HomeController".
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // HSTS (HTTP Strict Transport Security) añade una capa de seguridad.
    app.UseHsts();
}
else
{
    // En desarrollo, mostrar páginas de error detalladas.
    app.UseDeveloperExceptionPage();
}

// 2. Redirección HTTPS.
// Fuerza a que todas las solicitudes HTTP sean redirigidas a HTTPS.
app.UseHttpsRedirection();

// 3. Servir Archivos Estáticos.
// Permite que la aplicación sirva archivos desde la carpeta wwwroot (CSS, JavaScript, imágenes).
app.UseStaticFiles();

// 4. Enrutamiento.
// Habilita el sistema de enrutamiento de ASP.NET Core.
app.UseRouting();

// 5. (Opcional) Usar Sesión (si la configuraste arriba).
// app.UseSession();

// 6. Autorización (si tuvieras autenticación basada en cookies directamente en WebApp).
//    Como la autenticación se maneja con tokens JWT interactuando con la API,
//    la autorización a nivel de WebApp podría ser más para proteger ciertas páginas/rutas
//    basado en si el token existe en localStorage, pero esto se maneja principalmente en el cliente.
// app.UseAuthorization();


// 7. Mapear Rutas de Controladores.
// Configura el enrutamiento por defecto para los controladores MVC.
// La ruta por defecto suele ser "{controller=Home}/{action=Index}/{id?}".
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 8. (Opcional) Mapear Razor Pages (si las configuraste).
// app.MapRazorPages();


// --- Ejecutar la Aplicación ---
app.Run();
