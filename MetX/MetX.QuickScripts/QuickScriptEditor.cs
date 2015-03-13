﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using MetX.Data;
using MetX.Library;
using XLG.QuickScripts.TextEditorSample;

namespace XLG.QuickScripts
{
    public partial class QuickScriptEditor : Form
    {
        public static readonly List<QuickScriptOutput> OutputWindows = new List<QuickScriptOutput>();

        public XlgQuickScript SelectedScript { get { return QuickScriptList.SelectedItem as XlgQuickScript; } }

        public XlgQuickScript CurrentScript = null;
        public XlgQuickScriptFile Scripts;
        public bool Updating;

        TextArea textArea;
        CodeCompletionWindow completionWindow;

        public QuickScriptEditor(string filePath)
        {
            InitializeComponent();

            textArea = ScriptEditor.ActiveTextAreaControl.TextArea;
            textArea.KeyEventHandler += ProcessKey;

            FileSyntaxModeProvider fsmProvider = new FileSyntaxModeProvider(AppDomain.CurrentDomain.BaseDirectory);
            HighlightingManager.Manager.AddSyntaxModeFileProvider(fsmProvider); // Attach to the text editor.
            ScriptEditor.SetHighlighting("QuickScript"); // Activate the highlighting, use the name from the SyntaxDefinition node.
            //ScriptEditor.SetHighlighting("C#");
            ScriptEditor.Refresh();

            LoadQuickScriptsFile(filePath);
        }

        private bool ProcessKey(char ch)
        {
            if (ch == '.')
            {
                ShowCompletionWindow();
            }
            return false;
        }

        private void ShowCompletionWindow()
        {
/*
            CompletionDataProvider completionDataProvider = new CompletionDataProvider();
            completionWindow = CodeCompletionWindow.ShowCompletionWindow(this, ScriptEditor, String.Empty, completionDataProvider, '.');
            if (completionWindow != null)
            {
                completionWindow.Closed += CompletionWindowClosed;
            }
*/
        }

        private void CompletionWindowClosed(object source, EventArgs e)
        {
            if (completionWindow != null)
            {
                completionWindow.Closed -= CompletionWindowClosed;
                completionWindow.Dispose();
                completionWindow = null;
            }
        }

        private void RefreshLists()
        {
            Updating = true;
            try
            {
                QuickScriptList.Items.Clear();
                int defaultIndex = 0;
                foreach (XlgQuickScript script in Scripts)
                {
                    QuickScriptList.Items.Add(script);
                    if (Scripts.Default != null && script == Scripts.Default)
                    {
                        defaultIndex = QuickScriptList.Items.Count - 1;
                    }
                }
                if (defaultIndex > -1)
                {
                    QuickScriptList.SelectedIndex = defaultIndex;
                }
            }
            finally
            {
                Updating = false;
            }
        }

        public void OpenNewOutput(XlgQuickScript script, string title, string output)
        {
            QuickScriptOutput quickScriptOutput = new QuickScriptOutput(this, title, output, script);
            OutputWindows.Add(quickScriptOutput);
            quickScriptOutput.Show(this);
            quickScriptOutput.BringToFront();
        }

        public void UpdateScriptFromForm()
        {
            if (CurrentScript == null)
            {
                return;
            }

            CurrentScript.Script = ScriptEditor.Text;
            Enum.TryParse(DestinationList.Text.Replace(" ", string.Empty), out CurrentScript.Destination);
            CurrentScript.Input = InputList.Text;
            CurrentScript.SliceAt = SliceAt.Text;
            CurrentScript.DiceAt = DiceAt.Text;
            CurrentScript.InputFilePath = InputFilePath.Text;
            CurrentScript.DestinationFilePath = DestinationFilePath.Text;
            Scripts.Default = CurrentScript;
        }

        private void QuickScriptList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Updating)
            {
                return;
            }
            UpdateScriptFromForm();
            if (SelectedScript != null)
            {
                UpdateFormWithScript(SelectedScript);
            }
        }

        private void UpdateFormWithScript(XlgQuickScript selectedScript)
        {
            if (SelectedScript == null)
            {
                return;
            }

            QuickScriptList.Text = selectedScript.Name;
            ScriptEditor.Text = selectedScript.Script;
            ScriptEditor.Refresh();

            DestinationList.Text = selectedScript.Destination == QuickScriptDestination.Unknown
                ? "Text Box"
                : selectedScript.Destination.ToString().Replace("Box", " Box");

            int index = InputList.FindString(selectedScript.Input);
            InputList.SelectedIndex = index > -1
                ? index
                : 0;

            index = SliceAt.FindString(selectedScript.SliceAt);
            if (index > -1)
            {
                SliceAt.SelectedIndex = index;
            }
            else
            {
                SliceAt.SelectedIndex = SliceAt.Items.Add(selectedScript.SliceAt);
            }

            index = DiceAt.FindString(selectedScript.DiceAt);
            if (index > -1)
            {
                DiceAt.SelectedIndex = index;
            }
            else
            {
                DiceAt.SelectedIndex = DiceAt.Items.Add(selectedScript.DiceAt);
            }

            InputFilePath.Text = selectedScript.InputFilePath;
            if (InputFilePath.Text.Length > 0) InputFilePath.SelectionStart = InputFilePath.Text.Length;
            DestinationFilePath.Text = selectedScript.DestinationFilePath;
            if (DestinationFilePath.Text.Length > 0) DestinationFilePath.SelectionStart = DestinationFilePath.Text.Length;

            ScriptEditor.Focus();
            //ScriptEditor.SelectionStart = 0;
            //ScriptEditor.SelectionLength = 0;
            CurrentScript = selectedScript;
        }

        public void DisplayExpandedQuickScriptSourceInNotepad(bool independent)
        {
            try
            {
                if (CurrentScript == null) return;
                UpdateScriptFromForm();
                string source = CurrentScript.ToCSharp(independent);
                if (!string.IsNullOrEmpty(source))
                {
                    QuickScriptWorker.ViewTextInNotepad(source);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public BaseLineProcessor GenerateQuickScriptLineProcessor(XlgQuickScript scriptToRun)
        {
            if (string.IsNullOrEmpty(XlgQuickScript.DependentTemplate))
            {
                MessageBox.Show(this, "Quick script template missing.");
                return null;
            }

            if (scriptToRun == null)
            {
                return null;
            }
            string source = scriptToRun.ToCSharp(false);
            CompilerResults compilerResults = XlgQuickScript.CompileSource(source, false);

            if (compilerResults.Errors.Count <= 0)
            {
                Assembly assembly = compilerResults.CompiledAssembly;
                BaseLineProcessor quickScriptProcessor =
                    assembly.CreateInstance("MetX.QuickScriptProcessor") as BaseLineProcessor;
                return quickScriptProcessor;
            }

            StringBuilder sb =
                new StringBuilder("Compilation failure. Errors found include:" + Environment.NewLine
                                  + Environment.NewLine);
            for (int index = 0; index < compilerResults.Errors.Count; index++)
            {
                sb.AppendLine((index + 1) + ": Line "
                              + compilerResults.Errors[index]
                                  .ToString()
                                  .TokensAfterFirst("(")
                                  .Replace(")", string.Empty));
                sb.AppendLine();
            }
            MessageBox.Show(sb.ToString());
            QuickScriptWorker.ViewTextInNotepad(source);

            return null;
        }

        public string GenerateIndependentQuickScriptExe(XlgQuickScript scriptToRun)
        {
            if (InvokeRequired)
            {
                return (string)Invoke(new d_GenerateExe(GenerateIndependentQuickScriptExe), scriptToRun);
            }
            if (string.IsNullOrEmpty(XlgQuickScript.IndependentTemplate))
            {
                MessageBox.Show(this, "Independent Quick script template missing.");
                return null;
            }

            if (scriptToRun == null)
            {
                return null;
            }
            string source = scriptToRun.ToCSharp(true);
            CompilerResults compilerResults = XlgQuickScript.CompileSource(source, true);

            if (compilerResults.Errors.Count <= 0)
            {
                Assembly assembly = compilerResults.CompiledAssembly;
                string parentDestination = scriptToRun.DestinationFilePath.TokensBeforeLast(@"\");
                if (!Directory.Exists(parentDestination)) return assembly.Location;
                string exeFilePath = Path.Combine(parentDestination, (scriptToRun.Name + "_" + DateTime.Now.ToString("G").ToLower()).AsFilename()) + ".exe";
                string csFilePath = exeFilePath.Replace(".exe", ".cs");
                File.Copy(assembly.Location, exeFilePath);
                File.WriteAllText(csFilePath, source);
                //QuickScriptWorker.ViewFileInNotepad(csFilePath);
                //QuickScriptWorker.ViewTextInNotepad(source);
                return exeFilePath;
            }

            StringBuilder sb =
                new StringBuilder("Compilation failure. Errors found include:" + Environment.NewLine
                                  + Environment.NewLine);
            List<string> lines = new List<string>(source.LineList());
            for (int index = 0; index < compilerResults.Errors.Count; index++)
            {
                string error = compilerResults.Errors[index].ToString();
                if (error.Contains("(")) error = error.TokensAfterFirst("(").Replace(")", string.Empty);
                sb.AppendLine((index + 1) + ": Line " + error);
                sb.AppendLine();
                if (error.Contains(Environment.NewLine))
                    lines[compilerResults.Errors[index].Line - 1] += "\t// " + error.Replace(Environment.NewLine, " ");
                else if (compilerResults.Errors[index].Line == 0)
                    lines[0] += "\t// " + error;
                else
                    lines[compilerResults.Errors[index].Line - 1] += "\t// " + error;
            }
            MessageBox.Show(sb.ToString());
            QuickScriptWorker.ViewTextInNotepad(lines.Flatten());

            return null;
        }

        private void LoadQuickScriptsFile(string filePath)
        {
            Scripts = XlgQuickScriptFile.Load(filePath);

            if (Scripts.Count == 0)
            {
                XlgQuickScript script = new XlgQuickScript("First script", QuickScriptWorker.FirstScript);
                Scripts.Add(script);
                Scripts.Default = script;
                script = new XlgQuickScript("Example / Tutorial", QuickScriptWorker.ExampleTutorialScript);
                Scripts.Add(script);
            }

            RefreshLists();
            UpdateFormWithScript(Scripts.Default);
            Text = "Quick Script Editor - " + filePath;
        }

        private void RunQuickScript_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentScript == null) return;
                UpdateScriptFromForm();
                RunQuickScript(CurrentScript);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        protected bool ScriptIsRunning;
        private readonly object m_ScriptSyncRoot = new object();

        private delegate void d_RunQuickScript(XlgQuickScript scriptToRun, QuickScriptOutput targetOutput);
        private delegate string d_GenerateExe(XlgQuickScript scriptToRun);

        public void RunQuickScript(XlgQuickScript scriptToRun, QuickScriptOutput targetOutput = null)
        {
            if (InvokeRequired)
            {
                Invoke(new d_RunQuickScript(RunQuickScript), scriptToRun, targetOutput);
                return;
            }

            bool lockTaken = false;
            Monitor.TryEnter(m_ScriptSyncRoot, ref lockTaken);
            if (!lockTaken) return;

            try
            {
                ScriptIsRunning = true;
                string toParse = null;

                if (scriptToRun.Destination == QuickScriptDestination.File)
                {
                    if (string.IsNullOrEmpty(scriptToRun.DestinationFilePath))
                    {
                        MessageBox.Show(this, "Please supply an output filename.", "OUTPUT FILE PATH REQUIRED");
                        DestinationFilePath.Focus();
                        return;
                    }
                    if (!File.Exists(scriptToRun.DestinationFilePath))
                    {
                        Directory.CreateDirectory(scriptToRun.DestinationFilePath.TokensBeforeLast(@"\"));
                    }
                }

                switch (scriptToRun.Input.ToLower().Replace(" ", string.Empty))
                {
                    case "clipboard":
                        toParse = Clipboard.GetText();
                        break;

                    case "file":
                        if (string.IsNullOrEmpty(scriptToRun.InputFilePath))
                        {
                            MessageBox.Show(this, "Please supply an input filename.", "INPUT FILE PATH REQUIRED");
                            InputFilePath.Focus();
                            return;
                        }
                        if (!File.Exists(scriptToRun.InputFilePath))
                        {
                            MessageBox.Show(this, "The supplied input filename does not exist.",
                                "INPUT FILE DOES NOT EXIST");
                            InputFilePath.Focus();
                            return;
                        }

                   
                        toParse = File.ReadAllText(scriptToRun.InputFilePath);
                        break;
                }
                if (string.IsNullOrEmpty(toParse))
                {
                    return;
                }

                // This way supports both windows and linux line endings
                string[] lines = toParse.Replace("\r", string.Empty).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length <= 0)
                {
                    return;
                }

                BaseLineProcessor quickScriptProcessor = GenerateQuickScriptLineProcessor(scriptToRun);
                if (quickScriptProcessor == null)
                {
                    return;
                }

                quickScriptProcessor.LineCount = lines.Length;

                // Start
                try
                {
                    quickScriptProcessor.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error running Start:" + Environment.NewLine +
                        ex.ToString());
                }

                // ProcessLine (each)
                for (int index = 0; index < lines.Length; index++)
                {
                    var currLine = lines[index];
                    try
                    {
                        if (!quickScriptProcessor.ProcessLine(currLine, index))
                            return;
                    }
                    catch (Exception ex)
                    {
                        DialogResult answer = MessageBox.Show("Error processing line " + (index + 1) + ":" + Environment.NewLine +
                            currLine + Environment.NewLine +
                            Environment.NewLine +
                            ex, "CONTINUE PROCESSING", MessageBoxButtons.YesNo);
                        if (answer == DialogResult.No) return;
                    }
                }
                /*
                                if (lines.Where((t, index) => !quickScriptProcessor.ProcessLine(t, index)).Any())
                                {
                                    return;
                                }
                */
                try
                {
                    quickScriptProcessor.Finish();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error running Finish:" + Environment.NewLine +
                        ex.ToString());
                }

                if (quickScriptProcessor.Output == null || quickScriptProcessor.Output.Length <= 0)
                {
                    return;
                }

                try
                {
                    switch (scriptToRun.Destination)
                    {
                        case QuickScriptDestination.TextBox:
                            if (targetOutput == null)
                                OpenNewOutput(
                                    scriptToRun,
                                    QuickScriptList.Text + " at " + DateTime.Now.ToString("G"),
                                    quickScriptProcessor.Output.ToString());
                            else
                            {
                                targetOutput.Text = QuickScriptList.Text + " at " + DateTime.Now.ToString("G");
                                targetOutput.Output.Text = quickScriptProcessor.Output.ToString();
                            }
                            break;

                        case QuickScriptDestination.Clipboard:
                            Clipboard.Clear();
                            Clipboard.SetText(quickScriptProcessor.Output.ToString());
                            break;

                        case QuickScriptDestination.Notepad:
                            QuickScriptWorker.ViewTextInNotepad(quickScriptProcessor.Output.ToString());
                            break;

                        case QuickScriptDestination.File:
                            File.WriteAllText(DestinationFilePath.Text, quickScriptProcessor.Output.ToString());
                            QuickScriptWorker.ViewFileInNotepad(DestinationFilePath.Text);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                ScriptIsRunning = false;
                Monitor.Exit(m_ScriptSyncRoot);
            }
        }

        private void SaveQuickScript_Click(object sender, EventArgs e)
        {
            try
            {
                if (Updating)
                {
                    return;
                }
                if (Scripts != null)
                {
                    UpdateScriptFromForm();
                    Scripts.Save();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        private void AddQuickScript_Click(object sender, EventArgs e)
        {
            if (Updating)
            {
                return;
            }
            if (Scripts != null)
            {
                string name = string.Empty;
                DialogResult answer = UI.InputBox("New Script Name", "Please enter the name for the new script.",
                    ref name);
                if (answer != DialogResult.OK || (name ?? string.Empty).Trim() == string.Empty)
                {
                    return;
                }

                string script = string.Empty;
                XlgQuickScript newScript = null;
                if (CurrentScript != null)
                {
                    answer = MessageBox.Show(this, "Would you like to clone the current script?", "CLONE SCRIPT?",
                        MessageBoxButtons.YesNoCancel);
                    switch (answer)
                    {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Yes:
                            UpdateScriptFromForm();
                            //script = CurrentScript.Script;
                            newScript = CurrentScript.Clone(name);
                            break;
                    }
                }

                UpdateScriptFromForm();
                Updating = true;
                try
                {
                    if(newScript == null)
                        newScript = new XlgQuickScript(name, script);
                    Scripts.Add(newScript);
                    QuickScriptList.Items.Add(newScript);
                    QuickScriptList.SelectedIndex = QuickScriptList.Items.Count - 1;
                    UpdateFormWithScript(newScript);
                }
                finally
                {
                    Updating = false;
                }
            }
        }

        private void ViewGeneratedCode_Click(object sender, EventArgs e)
        {
            DisplayExpandedQuickScriptSourceInNotepad(false);
        }

        private void QuickScriptEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                foreach (QuickScriptOutput outputWindow in OutputWindows)
                {
                    outputWindow.Close();
                    outputWindow.Dispose();
                }
                OutputWindows.Clear();
            }
            catch
            {
                // Ignored
            }
            SaveQuickScript_Click(sender, null);
        }

        private void DeleteScript_Click(object sender, EventArgs e)
        {
            if (Updating)
            {
                return;
            }
            if (CurrentScript == null)
            {
                return;
            }

            DialogResult answer = MessageBox.Show(this,
                "This will permanently delete the current script.\n\tAre you sure this is what you want to do?",
                "DELETE SCRIPT", MessageBoxButtons.YesNo);
            if (answer == DialogResult.Yes)
            {
                Updating = true;
                XlgQuickScript script = CurrentScript;
                try
                {
                    QuickScriptList.Items.Remove(script);
                    Scripts.Remove(script);
                }
                finally
                {
                    Updating = false;
                }
                if (Scripts.Count == 0)
                {
                    script = new XlgQuickScript("First script");
                    Scripts.Add(script);
                    Scripts.Default = script;
                }
                else if (Scripts.Default == script)
                {
                    Scripts.Default = Scripts[0];
                }
                RefreshLists();
                UpdateFormWithScript(Scripts.Default);
            }
        }

        private void FilePathStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e) { }

        private void toolStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e) { }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) { }

        private void EditInputFilePath_Click(object sender, EventArgs e)
        {
            QuickScriptWorker.ViewFileInNotepad(InputFilePath.Text);
        }

        private void EditDestinationFilePath_Click(object sender, EventArgs e)
        {
            QuickScriptWorker.ViewFileInNotepad(DestinationFilePath.Text);
        }

        private void BrowseInputFilePath_Click(object sender, EventArgs e)
        {
            OpenInputFilePathDialog.FileName = InputFilePath.Text;
            OpenInputFilePathDialog.InitialDirectory = InputFilePath.Text.TokensBeforeLast(@"\");
            OpenInputFilePathDialog.AddExtension = true;
            OpenInputFilePathDialog.CheckFileExists = true;
            OpenInputFilePathDialog.CheckPathExists = true;
            //OpenInputFilePathDialog.DefaultExt = "." + ext;
            OpenInputFilePathDialog.Filter = "All files (*.*)|*.*";
            OpenInputFilePathDialog.Multiselect = false;
            OpenInputFilePathDialog.ShowDialog(this);
            if (OpenInputFilePathDialog.FileName != null)
            {
                InputFilePath.Text = OpenInputFilePathDialog.FileName;
            }
        }

        private void BrowseDestinationFilePath_Click(object sender, EventArgs e)
        {
            SaveDestinationFilePathDialog.FileName = DestinationFilePath.Text;
            SaveDestinationFilePathDialog.InitialDirectory = DestinationFilePath.Text.TokensBeforeLast(@"\");
            SaveDestinationFilePathDialog.AddExtension = true;
            SaveDestinationFilePathDialog.CheckPathExists = true;
            SaveDestinationFilePathDialog.Filter = "All files (*.*)|*.*";
            SaveDestinationFilePathDialog.ShowDialog(this);
            if (SaveDestinationFilePathDialog.FileName != null)
            {
                DestinationFilePath.Text = SaveDestinationFilePathDialog.FileName;
            }
        }

        private void ViewIndependectGeneratedCode_Click(object sender, EventArgs e)
        {
            //DisplayExpandedQuickScriptSourceInNotepad(true);
            try
            {
                if (CurrentScript == null) return;
                UpdateScriptFromForm();
                string location = GenerateIndependentQuickScriptExe(CurrentScript);
                if (!location.IsNullOrEmpty())
                {
                    if (DialogResult.Yes == MessageBox.Show(this,
                        "Executable generated successfully at: " + location + Environment.NewLine +
                        Environment.NewLine +
                        "Would you like to run it now? (No will open the generated file).", "RUN EXE?", MessageBoxButtons.YesNo)) 
                        Process.Start(new ProcessStartInfo(location)
                        {
                            UseShellExecute = true,
                            WorkingDirectory = location.TokensBeforeLast(@"\"),
                            
                        });
                    else
                    {
                        QuickScriptWorker.ViewFileInNotepad(location.Replace(".exe", ".cs"));
                    }
                        //Process.Start("explorer", location);
                }
                //RunQuickScript(CurrentScript);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            using (TextEditorForm textEditor = new TextEditorForm())
                textEditor.ShowDialog(this);
        }
    }
}