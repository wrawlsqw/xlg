using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

using MetX;
using MetX.IO;
using MetX.Security;
using MetX.Data;

namespace MetX.Glove.Console
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
                        configFilename = Token.First(gloveFilename, @"\App_Code\") + "\\web.config";
                    else
                        configFilename = Token.Before(gloveFilename, Token.Count(gloveFilename, @"\"), @"\") + @"\app.config";
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
                    System.Console.Write("--- FAILURE: Missing one or more arguments.");
                    return;
                }

                try
                {
                    CodeGenerator gen = new CodeGenerator(gloveFilename, xslFilename, configFilename, null);
                    string generatedCode = gen.GeneratedCode;
                    if (string.IsNullOrEmpty(generatedCode)) return;
                    FileSystem.StringToFile(outputFilename, generatedCode);
                    System.Console.Write("--- SUCCESS: " + outputFilename);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("--- FAILED: " + ex.ToString());
                }
            }
        }
    }
}
