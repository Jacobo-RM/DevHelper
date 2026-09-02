using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace DevHelper.Commands
{
    public static class CreateAngular
    {
        public static async Task ExecuteAsync(string[] args)
        {
            Console.WriteLine("=== DevHelper: Inicializando Generador de Proyectos Angular (Modo Avanzado) ===\n");

            bool tieneNode = VerificarNode();

            if (!tieneNode)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Node.js no está instalado.");
                Console.ResetColor();

                Console.Write("¿Deseas instalar Node.js? (S/N): ");
                string respNode = Console.ReadLine()?.Trim().ToLower() ?? "";

                if (respNode != "s" &&
                    respNode != "si" &&
                    respNode != "sí")
                {
                    Console.WriteLine("Node.js es obligatorio para Angular.");
                    return;
                }

                Console.Write("Versión de Node.js (Enter = lts): ");
                string versionNode = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(versionNode))
                    versionNode = "lts";

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\nInstalando Node.js {versionNode}...");
                Console.ResetColor();

                bool instalado = await InstalarNodeGlobalAsync(versionNode);

                if (!instalado)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error instalando Node.js.");
                    Console.ResetColor();
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Node.js instalado correctamente.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Node.js encontrado: {ObtenerVersionNode()}");
                Console.ResetColor();

                Console.Write("¿Deseas instalar otra versión de Node.js? (S/N): ");
                string respNode = Console.ReadLine()?.Trim().ToLower() ?? "";

                if (respNode == "s" ||
                    respNode == "si" ||
                    respNode == "sí")
                {
                    Console.Write("Versión a instalar (Enter = lts): ");
                    string nuevaVersion = Console.ReadLine()?.Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(nuevaVersion))
                        nuevaVersion = "lts";

                    bool instalado = await InstalarNodeGlobalAsync(nuevaVersion);

                    if (!instalado)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No se pudo instalar la nueva versión de Node.js.");
                        Console.ResetColor();
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Instalación finalizada.");
                    Console.ResetColor();
                }
            }

            Console.WriteLine();

            bool tieneAngular = VerificarAngular();

            if (tieneAngular)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Angular CLI encontrado: {ObtenerVersionAngular()}");
                Console.ResetColor();

                Console.Write("¿Deseas instalar otra versión de Angular CLI? (S/N): ");
                string respAngular = Console.ReadLine()?.Trim().ToLower() ?? "";

                if (respAngular == "s" ||
                    respAngular == "si" ||
                    respAngular == "sí")
                {
                    Console.Write("Versión Angular CLI (Enter = latest): ");
                    string versionAngular = Console.ReadLine()?.Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(versionAngular))
                        versionAngular = "latest";

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"Instalando Angular CLI {versionAngular}...");
                    Console.ResetColor();

                    bool angularInstalado =
                        await InstalarAngularAsync(versionAngular);

                    if (!angularInstalado)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error instalando Angular CLI.");
                        Console.ResetColor();
                        return;
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Angular CLI no encontrado.");
                Console.ResetColor();

                Console.Write("Versión Angular CLI a instalar (Enter = latest): ");
                string versionAngular = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(versionAngular))
                    versionAngular = "latest";

                bool angularInstalado =
                    await InstalarAngularAsync(versionAngular);

                if (!angularInstalado)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No se pudo instalar Angular CLI.");
                    Console.ResetColor();
                    return;
                }
            }

            Console.WriteLine();

            Console.Write("Introduce la ruta donde crear el proyecto: ");
            string rutaDondeCrear =
                Console.ReadLine()?.Trim().Trim('"') ?? "";

            if (string.IsNullOrWhiteSpace(rutaDondeCrear))
            {
                Console.WriteLine("Ruta inválida.");
                return;
            }

            Console.Write("Introduce el nombre del proyecto Angular: ");
            string nombreProyecto =
                Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nombreProyecto))
            {
                nombreProyecto = "nuevo-proyecto-angular";
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nCreando proyecto Angular...");
            Console.ResetColor();

            CrearProyectoAngular(
                nombreProyecto,
                rutaDondeCrear
            );
        }

        static string ObtenerVersionAngular()
        {
            try
            {
                bool esWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                ProcessStartInfo psi = new()
                {
                    FileName = esWindows ? "cmd.exe" : "ng",
                    Arguments = esWindows ? "/c ng version" : "version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? p = Process.Start(psi);

                if (p == null)
                    return "Desconocida";

                string salida = p.StandardOutput.ReadToEnd();

                p.WaitForExit();

                Match match = Regex.Match(
                    salida,
                    @"Angular CLI:\s*([0-9.]+)"
                );

                return match.Success ? match.Groups[1].Value : "Desconocida";
            }
            catch
            {
                return "Desconocida";
            }
        }
        static string ObtenerVersionNode()
        {
            try
            {
                bool esWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                ProcessStartInfo psi = new()
                {
                    FileName = esWindows ? "cmd.exe" : "node",
                    Arguments = esWindows ? "/c node -v" : "-v",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? p = Process.Start(psi);

                if (p == null)
                    return "Desconocida";

                string salida = p.StandardOutput.ReadToEnd();

                p.WaitForExit();

                return salida.Trim();
            }
            catch
            {
                return "Desconocida";
            }
        }
        static bool VerificarNode()
        {
            try
            {
                bool esWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = esWindows ? "cmd.exe" : "node",
                    Arguments = esWindows ? "/c node -v" : "-v",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process? proceso = Process.Start(psi))
                {
                    proceso?.WaitForExit();
                    return proceso?.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }
        static bool VerificarAngular()
        {
            bool esWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = esWindows ? "cmd.exe" : "ng",
                    Arguments = esWindows ? "/c ng version" : "version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process proceso = Process.Start(psi))
                {
                    if (proceso == null) return false;
                    proceso.WaitForExit();
                    return proceso.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        static async Task<bool> InstalarNodeGlobalAsync(string version)
        {
            try
            {
                bool esWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                if (esWindows)
                {
                    if (ExisteWinget())
                    {
                        string wingetCmd;

                        if (version.Equals("lts", StringComparison.OrdinalIgnoreCase))
                        {
                            wingetCmd =
                                "/c winget install OpenJS.NodeJS.LTS --accept-package-agreements --accept-source-agreements";
                        }
                        else
                        {
                            wingetCmd =
                                $"/c winget install OpenJS.NodeJS --version {version} --accept-package-agreements --accept-source-agreements";
                        }

                        ProcessStartInfo psiWinget = new()
                        {
                            FileName = "cmd.exe",
                            Arguments = wingetCmd,
                            UseShellExecute = false,
                            CreateNoWindow = false
                        };

                        using Process? p = Process.Start(psiWinget);

                        if (p == null)
                            return false;

                        await p.WaitForExitAsync();

                        return p.ExitCode == 0;
                    }

                    Console.WriteLine("Winget no encontrado. Se usará descarga MSI.");

                    string versionReal = version;

                    if (version.Equals("lts", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Write(
                            "Introduce la versión LTS exacta (ej: 22.17.1): ");

                        versionReal =
                            Console.ReadLine()?.Trim() ?? "";

                        if (string.IsNullOrWhiteSpace(versionReal))
                        {
                            Console.WriteLine("Versión inválida.");
                            return false;
                        }
                    }

                    string url =
                        $"https://nodejs.org/dist/v{versionReal}/node-v{versionReal}-x64.msi";

                    string rutaMsi =
                        Path.Combine(
                            Path.GetTempPath(),
                            $"node-{versionReal}.msi");

                    using HttpClient cliente = new();

                    Console.WriteLine($"Descargando {url}");

                    byte[] datos =
                        await cliente.GetByteArrayAsync(url);

                    await File.WriteAllBytesAsync(
                        rutaMsi,
                        datos);

                    ProcessStartInfo psi = new()
                    {
                        FileName = "msiexec.exe",
                        Arguments =
                            $"/i \"{rutaMsi}\" /qn /norestart",
                        UseShellExecute = false,
                        CreateNoWindow = false
                    };

                    using Process? instalador =
                        Process.Start(psi);

                    if (instalador == null)
                        return false;

                    await instalador.WaitForExitAsync();

                    if (File.Exists(rutaMsi))
                        File.Delete(rutaMsi);

                    return instalador.ExitCode == 0;
                }
                else
                {
                    string comando =
                        version.Equals("lts",
                            StringComparison.OrdinalIgnoreCase)
                        ? "lts"
                        : version;

                    ProcessStartInfo psi = new()
                    {
                        FileName = "bash",
                        Arguments =
                            $"-c \"sudo npm install -g n && sudo n {comando}\"",
                        UseShellExecute = false,
                        CreateNoWindow = false
                    };

                    using Process? p = Process.Start(psi);

                    if (p == null)
                        return false;

                    await p.WaitForExitAsync();

                    return p.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error instalando Node.js: {ex.Message}");

                return false;
            }
        }

        static bool ExisteWinget()
        {
            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = "cmd.exe",
                    Arguments = "/c winget --version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process? p = Process.Start(psi);

                if (p == null)
                    return false;

                p.WaitForExit();

                return p.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
        static async Task<bool> InstalarAngularAsync(string version)
        {
            try
            {
                bool esWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                string packageTarget = $"@angular/cli@{version}";
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = esWindows ? "cmd.exe" : "npm",
                    Arguments = esWindows ? $"/c npm install -g {packageTarget}" : $"install -g {packageTarget}",
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                using (Process proceso = Process.Start(psi))
                {
                    if (proceso == null) return false;
                    await proceso.WaitForExitAsync();
                    return proceso.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico en la instalación: {ex.Message}");
                return false;
            }
        }

        static void CrearProyectoAngular(string nombre, string rutaDestino)
        {
            try
            {
                if (!Directory.Exists(rutaDestino))
                {
                    Directory.CreateDirectory(rutaDestino);
                }

                bool esWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = esWindows ? "cmd.exe" : "ng",
                    Arguments = esWindows ? $"/c ng new \"{nombre}\" --defaults" : $"new \"{nombre}\" --defaults",
                    WorkingDirectory = rutaDestino,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                using (Process proceso = Process.Start(psi))
                {
                    proceso?.WaitForExit();
                    if (proceso?.ExitCode == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\nProyecto Angular '{nombre}' creado con éxito en: {Path.Combine(rutaDestino, nombre)}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine("\nEl comando 'ng new' falló al intentar estructurar el proyecto.");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se pudo ejecutar el comando de creación: {ex.Message}");
            }
        }


    }
}