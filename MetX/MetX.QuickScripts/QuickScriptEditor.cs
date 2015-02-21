﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows.Forms;
using MetX.Data;
using MetX.Library;
using Microsoft.CSharp;

namespace XLG.Pipeliner
{
    public partial class QuickScriptEditor : Form
    {
        public static readonly List<QuickScriptOutput> OutputWindows = new List<QuickScriptOutput>();

        public XlgQuickScript SelectedScript
        {
            get
            {
                return QuickScriptList.SelectedItem as XlgQuickScript;
            }
        }

        public XlgQuickScript CurrentScript = null;

        public XlgQuickScriptFile Scripts;

        public QuickScriptEditor(string filePath)
        {
            InitializeComponent();
            LoadQuickScriptsFile(filePath);
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
                        defaultIndex = QuickScriptList.Items.Count - 1;
                }
                if (defaultIndex > -1) QuickScriptList.SelectedIndex = defaultIndex;
            }
            finally
            {
                Updating = false;
            }
        }

        private QuickScriptEditor(XlgQuickScriptFile scripts)
        {
            Scripts = scripts;
            RefreshLists();
        }

        public void OpenNewOutput(string title, string output)
        {
            QuickScriptOutput quickScriptOutput = new QuickScriptOutput(title, output);
            OutputWindows.Add(quickScriptOutput);
            quickScriptOutput.Show(this);
            quickScriptOutput.BringToFront();
        }

        public void UpdateScriptFromForm()
        {
            if (CurrentScript == null) return;

            CurrentScript.Script = QuickScript.Text;
            Enum.TryParse(DestinationList.Text.Replace(" ", string.Empty), out CurrentScript.Destination);
            CurrentScript.Input = Input.Text;
            CurrentScript.SliceAt = SliceAt.Text;
            CurrentScript.DiceAt = DiceAt.Text;
            Scripts.Default = CurrentScript;
        }

        private void QuickScriptList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Updating) return;
            UpdateScriptFromForm();
            if (SelectedScript != null) UpdateFormWithScript(SelectedScript);
        }

        public bool Updating;

        private void UpdateFormWithScript(XlgQuickScript selectedScript)
        {
            if (SelectedScript == null)
            {
                //MessageBox.Show(this, "No script to update.");
                return;
            }

            QuickScriptList.Text = selectedScript.Name;
            QuickScript.Text = selectedScript.Script;
            
            DestinationList.Text = selectedScript.Destination == QuickScriptDestination.Unknown
                ? "Text Box"
                : selectedScript.Destination.ToString().Replace("Box", " Box");
            
            int index = Input.FindString(selectedScript.Input);
            Input.SelectedIndex = index > -1
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

            QuickScript.Focus();
            QuickScript.SelectionStart = 0;
            QuickScript.SelectionLength = 0;
            CurrentScript = selectedScript;
        }

        public void DisplayExpandedQuickScriptSourceInNotepad()
        {
            try
            {
                if (CurrentScript == null) return;
                UpdateScriptFromForm();
                string source = CurrentScript.ConvertQuickScriptToCSharp();
                if(!string.IsNullOrEmpty(source)) QuickScriptWorker.ViewTextInNotepad(source);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public IProcessLine GenerateQuickScriptLineProcessor()
        {
            if (string.IsNullOrEmpty(XlgQuickScript.Template))
            {
                MessageBox.Show(this, "Quick script template missing.");
                return null;
            }

            if (CurrentScript == null) return null;
            UpdateScriptFromForm();
            string source = CurrentScript.ConvertQuickScriptToCSharp();
            CompilerResults compilerResults = XlgQuickScript.CompileSource(source);

            if (compilerResults.Errors.Count <= 0)
            {
                Assembly assembly = compilerResults.CompiledAssembly;
                IProcessLine quickScriptProcessor = assembly.CreateInstance("MetX.QuickScriptProcessor") as IProcessLine;
                return quickScriptProcessor;
            }

            StringBuilder sb = new StringBuilder("Compilation failure. Errors found include:" + Environment.NewLine);
            for (int index = 0; index < compilerResults.Errors.Count; index++)
            {
                sb.AppendLine((index + 1) + ": " + compilerResults.Errors[index]);
                sb.AppendLine();
            }
            MessageBox.Show(sb.ToString());
            QuickScriptWorker.ViewTextInNotepad(source);

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
        }

        private void RunQuickScript_Click(object sender, EventArgs e)
        {
            RunQuickScriptNow();
        }

        private void RunQuickScriptNow()
        {
            try
            {
                string toParse = Clipboard.GetText();
                if (string.IsNullOrEmpty(toParse)) return;

                string[] lines = toParse.Replace("\r", string.Empty)
                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length <= 0) return;

                IProcessLine quickScriptProcessor = GenerateQuickScriptLineProcessor();
                if (quickScriptProcessor == null) return;

                StringBuilder sb = new StringBuilder();
                int lineCount = lines.Length;
                Dictionary<string, string> d = new Dictionary<string, string>();

                if (lines.Where((t, index) => !quickScriptProcessor.ProcessLine(sb, t, index, lineCount, d)).Any()) return;
                if (sb.Length <= 0) return;
                
                try
                {
                    switch (DestinationList.Text.ToLower().Replace(" ", string.Empty))
                    {
                        case "textbox":
                        case "text box":
                            OpenNewOutput(QuickScriptList.Text + " at " + DateTime.Now.ToString("G"), sb.ToString());
                            break;

                        case "clipboard":
                            Clipboard.Clear();
                            Clipboard.SetText(sb.ToString());
                            break;

                        case "notepad":
                            QuickScriptWorker.ViewTextInNotepad(sb.ToString());
                            break;

                        case "file":
                            // TODO
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
        }

        private void SaveQuickScript_Click(object sender, EventArgs e)
        {
            try
            {
                if (Updating) return;
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
            if (Updating) return;
            if (Scripts != null)
            {
                string name = string.Empty;
                DialogResult answer = UI.InputBox("New Script Name", "Please enter the name for the new script.", ref name);
                if (answer != DialogResult.OK || (name ?? string.Empty).Trim() == string.Empty) return;

                string script = string.Empty;
                if (CurrentScript != null)
                {
                    answer = MessageBox.Show(this, "Would you like to clone the current script?", "CLONE SCRIPT?", MessageBoxButtons.YesNoCancel);
                    switch (answer)
                    {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Yes:
                            UpdateScriptFromForm();
                            script = CurrentScript.Script;
                            break;
                    }
                }

                UpdateScriptFromForm();
                Updating = true;
                try
                {
                    XlgQuickScript newScript = new XlgQuickScript(name, script);
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
            DisplayExpandedQuickScriptSourceInNotepad();
        }

        private void QuickScriptEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveQuickScript_Click(sender, null);
        }

        private void DeleteScript_Click(object sender, EventArgs e)
        {
            if (Updating) return;
            if (CurrentScript == null) return;

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
                    Scripts.Default = Scripts[0];
                RefreshLists();
                UpdateFormWithScript(Scripts.Default);
            }
        }

        private void FilePathStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}