using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

// ReSharper disable PublicConstructorInAbstractClass

namespace MetX.Standard.Data
{
    /// <summary>
    /// Base class for persisting objects. Follows the "Active Record Design Pattern".
    /// You can read more on this pattern at http://en.wikipedia.org/wiki/Active_Record
    /// </summary>
	[Serializable]
	public abstract class ActiveRecord
    {
        #region State Properties
        [XmlIgnore] public bool IsLoaded;
        [XmlIgnore] public bool IsNew;
        [XmlIgnore] public string TableName;
        [XmlIgnore] public object Tag;
        [XmlIgnore] public int RecordHashThen;
        #endregion
        #region default constructor
        public ActiveRecord() { IsNew = true; }

        /*
        public ActiveRecord(IDataReader dataReader) => Load(dataReader);
        public ActiveRecord(DataRow dr) => Load(dr);
        */

        #endregion

        public abstract void Load(IDataReader dataReader);
        public abstract void Load(DataRow dr);
        public abstract string ClassName();
        
        public virtual void Save()  {  Save(null);  }
        public virtual void SaveIfChanged()  {  if(HasChanged())  Save(null);  }
        public abstract void Save(string userName);

		public abstract string OuterXml();
        public abstract void ToXml(XmlWriter xw);
		public abstract void ToXml(TextWriter xw);
		public abstract void ToXml(StringBuilder sb);
		public abstract void ToXml(StringBuilder sb, string outerTagName);

		public abstract QueryCommand GetInsertCommand(string userName);
        public abstract QueryCommand GetUpdateCommand(string userName);

        public abstract int RecordHashNow();
        public bool HasChanged()  { return IsNew || RecordHashThen == 0 || RecordHashThen != RecordHashNow();  }
    }

    [Serializable]
    public abstract class ActiveList<T> : List<T> where T : ActiveRecord
    {
		public abstract string OuterXml();
		public abstract string InnerXml();
		public abstract void ToXml(TextWriter output);
		public abstract void ToXml(XmlWriter xw);
		public abstract void ToXml(StringBuilder sb);
		public abstract void ToXml(StringBuilder sb, string outerTagName);
	}

}
