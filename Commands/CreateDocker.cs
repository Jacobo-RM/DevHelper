using System.Xml.Linq;

namespace DevHelper.Commands
{
    public static class CreateDocker
    {
        public static async Task ExecuteAsync(string[] args)
        {
            Console.WriteLine("=== DevHelper: Inicializando Generador de Docker (Modo Avanzado) ===\n");

            var csproj = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj").FirstOrDefault();
            if (csproj is null)
            {
                Console.WriteLine("No se encontro el archivo .csproj");
                return;
            }

            var doc = XDocument.Load(csproj);
            var sdk = doc.Root?.Attribute("Sdk")?.Value;

            switch(sdk)
            {
                case "Microsoft.NET.Sdk":
                    GenerateConsoleDockerFiles();
                    break;
            }
        }

        private static void GenerateConsoleDockerFiles()
        {
            var projectName = Directory.GetFiles(
                    Directory.GetCurrentDirectory(),
                    "*.csproj")
                .Select(Path.GetFileNameWithoutExtension)
                .First();

            var dockerfile = $"""
        FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
        WORKDIR /src

        COPY . .

        RUN dotnet restore
        RUN dotnet publish -c Release -o /app/publish

        FROM mcr.microsoft.com/dotnet/runtime:9.0

        WORKDIR /app

        COPY --from=build /app/publish .

        ENTRYPOINT ["dotnet", "{projectName}.dll"]
        """;

            File.WriteAllText("Dockerfile", dockerfile);

            var compose = $"""
        services:
          {projectName.ToLower()}:
            build: .
            container_name: {projectName.ToLower()}
        """;

            File.WriteAllText("docker-compose.yml", compose);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Dockerfile generated");
            Console.WriteLine("docker-compose.yml generated");
            Console.ResetColor();
        }
    }
}