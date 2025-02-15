﻿using CommandLine;
using MetX.Standard.Generators.GenGen;
using Microsoft.CodeAnalysis.Options;

namespace MetX.Standard.Generators
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var worker = new GenGenWorker();
            Parser.Default.ParseArguments<GenGenOptions>(args)
                .WithParsed(worker.Go);
        }
    }
}