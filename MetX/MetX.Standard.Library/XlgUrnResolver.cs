using System;
using System.Collections.Generic;
using System.Xml;

namespace MetX.Standard.Library
{
    /// <summary>Implements a XmlResolver which tracks which files are loaded so PageCache dependencies can easily be implemented</summary>
    public class XlgUrnResolver : XmlResolver
    {
        XmlUrlResolver _xur;
        
        /// <summary>Only Load has been called on an XSL/XSL document, this will contain a list of the files included by the XSL/XML</summary>
        public List<string> FileEntitys;

        /// <summary>Maps a URI to an object containing the physical resource. Override this to implement your own functionality</summary>
        /// <param name="absoluteUri">The URI to retrieve</param>
        /// <param name="ofObjectToReturn">The type of object to return. Current implementation only returns System.IO.Stream objects</param>
        /// <returns>The actual resource</returns>
        public virtual object OnGetEntity(Uri absoluteUri, Type ofObjectToReturn)
        {
            return _xur.GetEntity(absoluteUri, null, ofObjectToReturn);
        }

        /// <summary>Basic constructor</summary>
        public XlgUrnResolver()
        {
            _xur = new XmlUrlResolver();
            FileEntitys = new List<string>();
        }

        /// <summary>Provides the base authentication credentials</summary>
        public override System.Net.ICredentials Credentials
        {
            set => _xur.Credentials = value;
        }

        /// <summary>Resolves and retrieves the requested entity via URI</summary>
        /// <param name="absoluteUri">The URI of the entity</param>
        /// <param name="role">Unknown (see XmlResolver)</param>
        /// <param name="ofObjectToReturn">Unknown (see XmlResolver)</param>
        /// <returns>Unknown (see XmlResolver)</returns>
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            if (absoluteUri.AbsoluteUri.StartsWith("file:"))
            {
                var filename = absoluteUri.AbsoluteUri.Replace("file:///", string.Empty).Replace("/", "\\");
                if (!FileEntitys.Contains(filename))
                    FileEntitys.Add(filename);
            }
            return OnGetEntity(absoluteUri, ofObjectToReturn);
        }
    }
}
