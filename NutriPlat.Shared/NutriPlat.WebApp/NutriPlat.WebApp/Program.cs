// NutriPlat.WebApp/Program.cs

// Crear el constructor de la aplicaci�n web.
var builder = WebApplication.CreateBuilder(args);

// --- Configuraci�n de Servicios ---

// 1. Agregar servicios para controladores y vistas (MVC).
// Esto es necesario para que funcionen tus Controllers y las Vistas Razor.
builder.Services.AddControllersWithViews();

// 2. (Opcional) Configurar Razor Pages si tambi�n las vas a usar.
// builder.Services.AddRazorPages();

// 3. (Opcional) Configurar servicios de sesi�n si los necesitas.
// builder.Services.AddDistributedMemoryCache(); // Necesario para el proveedor de sesi�n en memoria
// builder.Services.AddSession(options =>
// {
//     options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de inactividad de la sesi�n
//     options.Cookie.HttpOnly = true; // La cookie de sesi�n no es accesible por JavaScript del lado del cliente
//     options.Cookie.IsEssential = true; // Marca la cookie como esencial para GDPR
// });

// 4. (Opcional) Configurar HttpClientFactory si vas a hacer muchas llamadas a tu API desde el backend de WebApp.
//    Aunque para el login que hicimos, la llamada se hace desde JavaScript del lado del cliente.
// builder.Services.AddHttpClient();


// --- Construir la Aplicaci�n ---
var app = builder.Build();


// --- Configurar el Pipeline de Middleware HTTP ---

// 1. Manejo de Excepciones y Errores.
if (!app.Environment.IsDevelopment())
{
    // En producci�n, usar una p�gina de error personalizada.
    // Esto redirige al usuario a la acci�n "Error" del "HomeController".
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // HSTS (HTTP Strict Transport Security) a�ade una capa de seguridad.
    app.UseHsts();
}
else
{
    // En desarrollo, mostrar p�ginas de error detalladas.
    app.UseDeveloperExceptionPage();
}

// 2. Redirecci�n HTTPS.
// Fuerza a que todas las solicitudes HTTP sean redirigidas a HTTPS.
app.UseHttpsRedirection();

// 3. Servir Archivos Est�ticos.
// Permite que la aplicaci�n sirva archivos desde la carpeta wwwroot (CSS, JavaScript, im�genes).
app.UseStaticFiles();

// 4. Enrutamiento.
// Habilita el sistema de enrutamiento de ASP.NET Core.
app.UseRouting();

// 5. (Opcional) Usar Sesi�n (si la configuraste arriba).
// app.UseSession();

// 6. Autorizaci�n (si tuvieras autenticaci�n basada en cookies directamente en WebApp).
//    Como la autenticaci�n se maneja con tokens JWT interactuando con la API,
//    la autorizaci�n a nivel de WebApp podr�a ser m�s para proteger ciertas p�ginas/rutas
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


// --- Ejecutar la Aplicaci�n ---
app.Run();
