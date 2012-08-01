using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CSharp;

namespace CsRun
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                    Environment.Exit(-126);

                if (!File.Exists(args[0]))
                {
                    Console.WriteLine("File not found: " + args[0]);
                    Environment.Exit(-125);
                }
                Run(args[0], args.Skip(1));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Environment.Exit(-127);
        }

        private static readonly Regex RxRef = new Regex(@"^\s*//ref:\s*(.*)\s*;\s*$",RegexOptions.Compiled|RegexOptions.Multiline|RegexOptions.IgnoreCase);

        private static void Run(string csFilename, IEnumerable<string> args)
        {
            var compilerParameters = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true
                };
            var referencedAssemblies = new[] {"System.dll", "System.Core.dll", "System.Data.dll"};
            foreach (var assembly in referencedAssemblies)
            {
                compilerParameters.ReferencedAssemblies.Add(assembly);
            }
            compilerParameters.IncludeDebugInformation = true;

            var cSharpCodeProvider = new CSharpCodeProvider();

            var code = File.ReadAllText(csFilename);

            foreach(Match m in RxRef.Matches(code))
            {
                compilerParameters.ReferencedAssemblies.Add(m.Groups[1].Value);
            }

            var result = cSharpCodeProvider.CompileAssemblyFromSource(compilerParameters, code);
            if (result.Errors.Count != 0)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error);
                }
                return;
            }
            result.CompiledAssembly.GetType("CsRun.Program").GetMethod("Main").Invoke(null, new object[] {args.ToArray()});
        }
    }
}