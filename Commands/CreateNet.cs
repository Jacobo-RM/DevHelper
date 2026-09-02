using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DevHelper.Commands
{
    public static class CreateNet
    {
        public static async Task ExecuteAsync(string[] args)
        {
            Console.WriteLine("=== DevHelper: Inicializando Generador de Proyectos .NET (Modo Avanzado) ===\n");

            bool tieneDotnet = VerificarDotnet();

            if (tieneDotnet)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($".NET SDK encontrado: {ObtenerVersionDotnet()}");
                Console.ResetColor();

                Console.Write("\n¿Deseas instalar otra versión de .NET SDK? (S/N): ");
                string respuesta = Console.ReadLine()?.Trim().ToLower() ?? "";

                if (respuesta == "s" ||
                    respuesta == "si" ||
                    respuesta == "sí")
                {
                    Console.Write("Versión a instalar (Ej: 8.0, 9.0, LTS, STS): ");
                    string version = Console.ReadLine()?.Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(version))
                        version = "STS";

                    bool instalado = await InstalarDotnetAsync(version);

                    if (!instalado)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error instalando .NET SDK.");
                        Console.ResetColor();
                        return;
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(".NET SDK NO encontrado.");
                Console.ResetColor();

                Console.Write("\n¿Deseas instalar .NET SDK ahora mismo? (S/N): ");
                string respuesta = Console.ReadLine()?.Trim().ToLower() ?? "";

                if (respuesta != "s" &&
                    respuesta != "si" &&
                    respuesta != "sí")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Operación cancelada. Se requiere .NET SDK para continuar.");
                    Console.ResetColor();
                    return;
                }

                Console.Write("Versión a instalar (Ej: 8.0, 9.0, LTS, STS): ");
                string version = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(version))
                    version = "STS";

                bool instalado = await InstalarDotnetAsync(version);

                if (!instalado)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error durante la instalación de .NET SDK.");
                    Console.ResetColor();
                    return;
                }
            }

            Console.WriteLine();

            Console.Write("Introduce la ruta de la carpeta donde quieres ubicar tu proyecto: ");

            string rutaDondeCrear =
                Console.ReadLine()?.Trim().Trim('"') ?? "";

            if (string.IsNullOrWhiteSpace(rutaDondeCrear))
            {
                Console.WriteLine("Ruta inválida.");
                return;
            }

            Console.Write("Introduce el nombre que tendrá el proyecto: ");

            string nombreProyecto =
                Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nombreProyecto))
            {
                nombreProyecto = "NuevoProyectoDotNet";
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Tipos de proyecto disponibles:");
            Console.ResetColor();

            Console.WriteLine("1 - Console");
            Console.WriteLine("2 - Web API");
            Console.WriteLine("3 - MVC");
            Console.WriteLine("4 - Razor");
            Console.WriteLine("5 - Class Library");
            Console.WriteLine("6 - Worker Service");

            Console.Write("\nSelecciona una opción [1-6]: ");

            string opcion = Console.ReadLine()?.Trim() ?? "1";

            string template = opcion switch
            {
                "2" => "webapi",
                "3" => "mvc",
                "4" => "razor",
                "5" => "classlib",
                "6" => "worker",
                _ => "console"
            };

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\nCreando proyecto {template}...");
            Console.ResetColor();

            CrearProyecto(
                nombreProyecto,
                rutaDondeCrear,
                template
            );
        }

        static bool VerificarDotnet()
        {
            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = "dotnet",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? proceso = Process.Start(psi);

                if (proceso == null)
                    return false;

                proceso.WaitForExit();

                return proceso.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        static string ObtenerVersionDotnet()
        {
            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = "dotnet",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? proceso = Process.Start(psi);

                if (proceso == null)
                    return "Desconocida";

                string salida = proceso.StandardOutput.ReadToEnd();

                proceso.WaitForExit();

                return salida.Trim();
            }
            catch
            {
                return "Desconocida";
            }
        }

        static async Task<bool> InstalarDotnetAsync(string version)
        {
            try
            {
                bool esWindows =
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                if (!esWindows)
                {
                    Console.WriteLine("La instalación automática en Linux todavía no está implementada.");
                    return false;
                }

                string paquete;

                if (version.Equals("LTS", StringComparison.OrdinalIgnoreCase))
                {
                    paquete = "Microsoft.DotNet.SDK.8";
                }
                else if (version.Equals("STS", StringComparison.OrdinalIgnoreCase))
                {
                    paquete = "Microsoft.DotNet.SDK.9";
                }
                else if (version.StartsWith("8"))
                {
                    paquete = "Microsoft.DotNet.SDK.8";
                }
                else if (version.StartsWith("9"))
                {
                    paquete = "Microsoft.DotNet.SDK.9";
                }
                else
                {
                    paquete = "Microsoft.DotNet.SDK";
                }

                ProcessStartInfo psi = new()
                {
                    FileName = "cmd.exe",
                    Arguments =
                        $"/c winget install {paquete} --accept-package-agreements --accept-source-agreements",
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                using Process? proceso = Process.Start(psi);

                if (proceso == null)
                    return false;

                await proceso.WaitForExitAsync();

                return proceso.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error instalando .NET SDK: {ex.Message}");
                return false;
            }
        }

        static void CrearProyecto(
            string nombre,
            string rutaDestino,
            string template)
        {
            try
            {
                if (!Directory.Exists(rutaDestino))
                {
                    Directory.CreateDirectory(rutaDestino);
                }

                ProcessStartInfo psi = new()
                {
                    FileName = "dotnet",
                    Arguments = $"new {template} -o \"{nombre}\"",
                    WorkingDirectory = rutaDestino,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                using Process? proceso = Process.Start(psi);

                if (proceso == null)
                {
                    Console.WriteLine("No se pudo iniciar dotnet.");
                    return;
                }

                proceso.WaitForExit();

                if (proceso.ExitCode == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(
                        $"\nProyecto '{nombre}' creado con éxito en:\n{Path.Combine(rutaDestino, nombre)}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error creando el proyecto.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se pudo ejecutar el comando de creación: {ex.Message}");
            }
        }
    }
}