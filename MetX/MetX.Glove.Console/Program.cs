using System;
using System.Windows.Forms;
using MetX.Data;
using MetX.IO;
using MetX.Library;

namespace XLG.Pipeliner
{
    class Program
    {
        [STAThread()]
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GloveMain());
            }
            else
            {
                string gloveFilename = null, xslFilename = null, configFilename = null, outputFilename = null;

                if (args.Length == 1)
                {
                    gloveFilename = args[0];
                    xslFilename = gloveFilename + ".xsl";
                    if (gloveFilename.IndexOf(@"\App_Code\", StringComparison.Ordinal) > -1)
                        configFilename = gloveFilename.FirstToken(@"\App_Code\") + "\\web.config";
                    else
                        configFilename = gloveFilename.TokensBefore(gloveFilename.TokenCount(@"\"), @"\") + @"\app.config";
                    outputFilename = gloveFilename.Replace(".xlg", ".Glove.cs");
                }
                else if (args.Length == 4)
                {
                    gloveFilename = args[0];
                    xslFilename = args[1];
                    configFilename = args[2];
                    outputFilename = args[3];
                }

                if (gloveFilename == null || xslFilename == null || configFilename == null || outputFilename == null)
                {
                    Console.Write("--- FAILURE: Missing one or more arguments.");
                    return;
                }

                try
                {
                    CodeGenerator gen = new CodeGenerator(gloveFilename, xslFilename, configFilename, null);
                    string generatedCode = gen.GenerateCode();
                    if (string.IsNullOrEmpty(generatedCode)) return;
                    FileSystem.StringToFile(outputFilename, generatedCode);
                    Console.Write("--- SUCCESS: " + outputFilename);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("--- FAILED: " + ex.ToString());
                }
            }
        }
    }
}
