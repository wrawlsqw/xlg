﻿using System;
using System.Xml;

namespace MetX.Standard.Generation.CSharp.Project
{
    public class AspectsCsProjGenerator : CsProjGenerator
    {
        private CsProjGenerator _csProjGeneratorImplementation;
        public static string InitialDefaultTargetTemplate = "Namespace.AspectsName";

        public AspectsCsProjGenerator() 
        {
            WithDefaultTargetTemplate();
        }

        public override IGenerateCsProj Generate()
        {
            if (Document == null)
                throw new MissingFieldException("Document is required");
            
            if (Options.TryFullResolve(out var resolvedContents)) Document.LoadXml(resolvedContents);
            return this;
        }

        public sealed override IGenerateCsProj WithDefaultTargetTemplate()
        {
            DefaultTargetTemplate = InitialDefaultTargetTemplate;
            return this;
        }

        public AspectsCsProjGenerator(CsProjGeneratorOptions options, XmlDocument document = null) 
            : base(options.WithFilename($"{options.Namespace}.{options.AspectsName}"), document)
        {
            WithDefaultTargetTemplate();
            
        }

        public AspectsCsProjGenerator(string filePath) : base(filePath)
        {
            WithDefaultTargetTemplate();
        }
    }
}