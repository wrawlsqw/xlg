using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using MetX.Standard.Library;

namespace MetX.Standard.Scripts
{
    [Serializable]
    [XmlType(Namespace = "", AnonymousType = true)]
    public class XlgQuickScriptFile : List<XlgQuickScript>
    {
        public XlgQuickScript Default { get; set; }

        public string FilePath { get; set; }

        public XlgQuickScriptFile(string filePath) { FilePath = filePath; }

        public bool Save()
        {
            if (Count == 0 || string.IsNullOrEmpty(FilePath))
            {
                return true;
            }

            Directory.CreateDirectory(FilePath.TokensBeforeLast(@"\"));

            if (File.Exists(FilePath))
            {
                File.Move(FilePath,
                    FilePath + "_" +
                    DateTime.Now.ToString("s").Replace(":", "").Replace("-", "").Replace("T", " ") +
                    ".xlgq"
                    );
            }
            var content = new StringBuilder();
            foreach (var script in this)
            {
                content.AppendLine(script.ToFileFormat(script.Id == Default.Id));
            }
            File.WriteAllText(FilePath, content.ToString());
            var history = Directory.GetFiles(FilePath.TokensBeforeLast(@"\"), "*_* *.xlgq");
            if (history.Length > 4)
            {
                Array.Sort(history);
                for (var i = 4; i < history.Length; i++)
                {
                    File.SetAttributes(history[i], FileAttributes.Normal);
                    try
                    {
                        File.Delete(history[i]);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch { }
                }
            }
            return true;
        }

        public static XlgQuickScriptFile Load(string filePath)
        {
            var ret = new XlgQuickScriptFile(filePath);
            if (!File.Exists(ret.FilePath))
            {
                return ret;
            }
            var rawScripts = File
                .ReadAllText(ret.FilePath)
                .Split(new[] { "~~QuickScriptName:" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawScript in rawScripts)
            {
                if (string.IsNullOrWhiteSpace(rawScript)) continue;
                var script = new XlgQuickScript();
                var isDefault = script.Load(rawScript);
                ret.Add(script);
                if (isDefault) ret.Default = script;
            }
            ret.Sort((script, quickScript) => string.CompareOrdinal(script.Name, quickScript.Name));
            if (ret.Default == null && ret.Count > 0)
            {
                ret.Default = ret[0];
            }
            return ret;
        }

        public string[] ScriptNames()
        {
            var names = new string[Count];
            for (var i = 0; i < Count; i++)
            {
                names[i] = this[i].Name;
            }

            return names;
        }
    }
}