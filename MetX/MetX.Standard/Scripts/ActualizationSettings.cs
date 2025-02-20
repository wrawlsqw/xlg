using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using MetX.Standard.Interfaces;
using MetX.Standard.IO;
using MetX.Standard.Library;
using MetX.Standard.Library.Extensions;

namespace MetX.Standard.Scripts
{
    public class ActualizationSettings
    {
        public ActualizationSettings(XlgQuickScriptTemplate quickScriptTemplate, bool simulate,
            XlgQuickScript scriptToRun, bool forExecutable, IGenerationHost host)
        {
            Host = host;
            QuickScriptTemplate = quickScriptTemplate ?? throw new ArgumentNullException(nameof(quickScriptTemplate));
            Simulate = simulate;
            Script = scriptToRun;
            ForExecutable = forExecutable;
            TemplateNameAsLegalFilenameWithoutExtension = Script.Name.AsFilename().Replace("-", " ");
            ProjectName = TemplateNameAsLegalFilenameWithoutExtension;

            var tempFolder = Environment.GetEnvironmentVariable("TEMP") ?? @"C:\Windows\Temp";
            var targetFolder = Path.Combine(tempFolder, "QuickScriptProcessors");
            ProjectFolder = Path.Combine(targetFolder, TemplateNameAsLegalFilenameWithoutExtension);
            Directory.CreateDirectory(ProjectFolder);

            FileSystem.CleanFolder(Path.Combine(ProjectFolder, "obj"));
            FileSystem.CleanFolder(Path.Combine(ProjectFolder, "bin"));

            DebugPath = Path.Combine(ProjectFolder, "bin", "Debug");
            
            GeneratedAreas = new GenInstance(scriptToRun, quickScriptTemplate, true);
        }

        public void UpdateBinPath()
        {
            if (!Directory.Exists(DebugPath))
                return; 

            var outputFolderInfo = new DirectoryInfo(DebugPath);
            var outputFolder = outputFolderInfo.EnumerateDirectories().OrderBy(x => x.LastWriteTime).First().FullName;
            BinPath = outputFolder;
        }

        public string DebugPath { get; set; }

        public string ProjectName { get; set; }
        public bool Simulate { get; set; }
        public XlgQuickScriptTemplate QuickScriptTemplate { get; set; }

        public AssocArray Answers { get; set; } = new AssocArray();
        public string TemplateNameAsLegalFilenameWithoutExtension { get; set; } = Guid.NewGuid().ToString("N");

        public XlgQuickScript Script { get; set; }
        public bool ForExecutable { get; set; }
        public string ProjectFolder { get; set; }
        public GenInstance GeneratedAreas { get; set; }
        public IGenerationHost Host { get; set; }

        public string ProjectFilePath => Path.Combine(ProjectFolder, $"{TemplateNameAsLegalFilenameWithoutExtension}.csproj");
        public string BinPath { get; set; }
    }
}