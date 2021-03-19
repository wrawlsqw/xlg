﻿using System.Collections.Generic;
using System.Linq;
using System.Xml;
using MetX.Standard.Library;

namespace MetX.Standard.Generation.CSharp.Project
{
    public class Modifier
    {
        
        public XmlNode ProjectNode => GetNodeFromCacheOrDocument(XPaths.Project, false);
        public PropertyGroups PropertyGroups { get; set; }
        public Targets Targets { get; set; }
        
        public string FilePath { get; set; }
        public XmlDocument Document { get; set; }

        public static Modifier LoadFile(string filePath)
        {
            var document = new XmlDocument();
            document.Load(filePath);
            var modifier = new Modifier
            {
                Document = document,
                //Contents = document.InnerXml,
                FilePath = filePath,
            };
            modifier.PropertyGroups = new PropertyGroups(modifier);
            modifier.Targets = new Targets(modifier);
            
            return modifier;
        }

        public Modifier SaveToFile()
        {
            if (FilePath.IsEmpty())
                return this;
            
            Document?.Save(FilePath);
            return this;
        }

        public void SetElementInnerText(string xpath, bool value)
        {
            SetElementInnerText(xpath, value ? "true" : "false");
        }

        public void SetElementInnerText(string xpath, string innerText)
        {
            var node = (XmlElement) GetNodeFromCacheOrDocument(xpath, true) 
                       ?? MakeXPath(Document, xpath);
            node.InnerText = innerText ?? "";
        }

        public string InnerTextAt(string xpath, bool blankMeansNull = true)
        {
            var node = GetNodeFromCacheOrDocument(xpath, false);
            if(node == null) 
                return blankMeansNull ? null : "";
            return node.InnerText;
        }

        public XmlNode MakeXPath(string xpath)
        {
            if (xpath.IsEmpty())
                return null;
            
            return MakeXPath(Document, xpath);
        }

        public XmlNode MakeXPath(XmlNode parent, string xpath)
        {
            var originalXPath = xpath;
            while (true)
            {
                // grab the next node name in the xpath; or return parent if empty
                var partsOfXPath = xpath.Trim('/').Split('/');
                var nextNodeInXPath = partsOfXPath.First();
                if (string.IsNullOrEmpty(nextNodeInXPath))
                {
                    AddToCache(originalXPath, parent);
                    return parent;
                }

                // get or create the node from the name
                var node = parent.SelectSingleNode(nextNodeInXPath);
                if (node == null)
                {
                    node = parent.AppendChild(Document.CreateElement(nextNodeInXPath));
                }

                // rejoin the remainder of the array as an xpath expression and recurse
                var rest = string.Join("/", partsOfXPath.Skip(1).ToArray());
                parent = node;
                xpath = rest;
            }
        }

        public Dictionary<string, XmlNode> NodeCache = new();
        public XmlNode AddToCache(string xpath, XmlNode node)
        {
            return node;
            
            if (xpath.IsEmpty())
                return null;
            
            if (node == null)
            {
                RemoveFromCache(xpath);
                return null;
            }
            
            var key = xpath.ToLower();
            if (NodeCache.ContainsKey(key))
                NodeCache[key] = node;
            else
                NodeCache.Add(key, node);
            return node;
        }
        public void RemoveFromCache(string xpath)
        {
            if (xpath.IsEmpty())
                return;

            var key = xpath.ToLower();
            if (NodeCache.ContainsKey(key))
                NodeCache.Remove(key);
        }
        public XmlNode GetNodeFromCacheOrDocument(string xpath, bool addIfMissing)
        {
            if (xpath.IsEmpty())
                return null;
            
            var key = xpath.ToLower();
            /*
            if (NodeCache.ContainsKey(key))
                return NodeCache[key];
                */

            var node = Document.SelectSingleNode(xpath);
            if (node == null)
            {
                if(addIfMissing)
                {
                    node = MakeXPath(xpath);
                }
            }
            else
            {
                node = AddToCache(xpath, node);
            }
            return node;
        }

        public bool IsElementMissing(string xpath)
        {
            var node = GetNodeFromCacheOrDocument(xpath, false);
            return node == null;
        }

    }
}