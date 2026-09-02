using System.Diagnostics;

namespace DevHelper.Commands
{
    public static class GitInit
    {
        public static async Task ExecuteAsync(string[] args)
        {
            Console.WriteLine("=== DevHelper: Inicializando Generador de GIT Init (Modo Avanzado) ===\n");

            if (!VerificarGit())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Git no está instalado o no está disponible en el PATH.");
                Console.ResetColor();
                return;
            }

            Console.Write("Introduce la ruta donde inicializar Git: ");

            string path =
                Console.ReadLine()?.Trim().Trim('"') ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Ruta inválida.");
                return;
            }

            if (!Directory.Exists(path))
            {
                Console.WriteLine("El directorio no existe.");

                Console.Write("\n¿Deseas crearlo ahora mismo? (S/N): ");

                string respuesta =
                    Console.ReadLine()?.Trim().ToLower() ?? "";

                if (respuesta != "s" &&
                    respuesta != "si" &&
                    respuesta != "sí")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Operación cancelada.");
                    Console.ResetColor();
                    return;
                }

                Directory.CreateDirectory(path);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Directorio creado: {path}");
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nIniciando repositorio Git en: {path}");
            Console.ResetColor();

            if (Directory.Exists(Path.Combine(path, ".git")))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Ya existe un repositorio Git en este directorio.");
                Console.ResetColor();
                return;
            }



            var gitInit = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "init",
                WorkingDirectory = path,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(gitInit))
            {
                if (process == null)
                {
                    Console.WriteLine("No se pudo ejecutar git.");
                    return;
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                Console.WriteLine(output);

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine(error);
                }

                if (process.ExitCode != 0)
                {
                    Console.WriteLine("Error ejecutando git init.");
                    return;
                }
            }

            string gitignorePath = Path.Combine(path, ".gitignore");

            if (!File.Exists(gitignorePath))
            {
                File.WriteAllText(
                    gitignorePath,
                    @"bin/
                    obj/
                    node_modules/
                    .vs/
                    *.user
                    *.log");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(".gitignore creado correctamente.");
                Console.ResetColor();
            }

            var gitAdd = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "add .",
                WorkingDirectory = path,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(gitAdd))
            {
                if (process == null)
                {
                    Console.WriteLine("No se pudo ejecutar git add.");
                    return;
                }

                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(error);
                    Console.ResetColor();
                    return;
                }
            }

            var gitCommit = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "commit -m \"Commit Inicial\"",
                WorkingDirectory = path,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(gitCommit))
            {
                if (process == null)
                {
                    Console.WriteLine("No se pudo ejecutar git commit.");
                    return;
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Commit Inicial realizado correctamente.");
                    Console.ResetColor();

                    if (!string.IsNullOrWhiteSpace(output))
                        Console.WriteLine(output);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error realizando el commit.");
                    Console.ResetColor();

                    Console.WriteLine(error);
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nRepositorio Git creado correctamente.");
            Console.ResetColor();

            await Task.CompletedTask;
        }

        static bool VerificarGit()
        {
            try
            {
                ProcessStartInfo psi = new()
                {
                    FileName = "git",
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
    }
}