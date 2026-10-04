// ARCHIVO TEMPORAL de la estructura base del repositorio.
// La Persona 4 lo reemplaza por el Program.cs definitivo en su ultimo commit.
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseStaticFiles();
app.MapGet("/", () => "SED's Imports & Services - estructura base del repositorio (Fase 2)");
app.Run();
