using System;
using System.Runtime.CompilerServices;
using DevHelper.Commands;


namespace DevHelper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            switch (args[0])
            {
                case "createNet":
                    await CreateNet.ExecuteAsync(args);
                    break;
                case "createAngular":
                    await CreateAngular.ExecuteAsync(args);
                    break;
                case "createGitInit":
                    await GitInit.ExecuteAsync(args);
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Unknown command '{args[0]}'. Run 'devhelper help' for usage.");
                    Console.ResetColor();
                    break;
            }


        }
    }
}
