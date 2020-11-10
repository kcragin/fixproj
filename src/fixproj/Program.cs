﻿using System;
using System.Diagnostics;
using fixproj.Abstract;
using fixproj.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Plossum.CommandLine;

namespace fixproj
{
    public class Program
    {
        private static readonly Options Options = new Options();

        public static int Main(string[] args)
        {
            try
            {
                new CommandLineParser(Options).Parse();

                var collection = new ServiceCollection()
                    .AddSingleton<IProcess, ProcessFiles>(x => new ProcessFiles(Options))
                    .BuildServiceProvider();

                return collection.GetService<IProcess>().Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
            finally
            {
                if (Debugger.IsAttached)
                {
                    Console.ReadLine();
                }
            }
        }
    }
}
