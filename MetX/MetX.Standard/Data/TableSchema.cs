using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using MetX.Standard.Library;
using MetX.Standard.Library.Extensions;

// ReSharper disable NotAccessedField.Global
// ReSharper disable UnassignedField.Global

namespace MetX.Standard.Data
{
    /// <summary>C#CD: </summary>
    [Serializable]
    public class TableSchema
    {
        /// <summary>
        /// Holds information about the base table - this class should be
        /// static for each object
        /// </summary>
        [Serializable]
        public class Table
        {
            public TableColumnCollection Columns = new();

            /// <summary>The field list as it goes into the SELECT sql statement used for Select, Count, Exists and paging type queries</summary>
            public string FieldList;

            public TableIndexCollection Indexes = new();

            /// <summary>The basic INSERT INTO sql statement used for Insert type queries</summary>
            public string InsertSql;

            public DataProvider Instance;
            public TableKeyCollection Keys = new();
            public string Name;
            public string Schema;

            /// <summary>The basic UPDATE sql statement used for Update type query</summary>
            public string UpdateSql;

            /// <summary>C#CD: </summary>
            public Table() { }

            /// <summary>C#CD: </summary>
            /// <param name="tableName">C#CD: </param>
            /// <param name="schemaName"></param>
            public Table(string tableName, string schemaName)
            {
                Name = tableName;
                Schema = schemaName;
            }

            public string FullName
            {
                get
                {
                    if (string.IsNullOrEmpty(Schema))
                        Schema = "dbo";
                    return "[" + Schema + "].[" + Name + "]";
                }
            }

            public TableColumn PrimaryKey => Columns?.GetPrimaryKey();

            /// <summary>The basic SELECT sql statement used for Select, Count, Exists and paging type queries</summary>
            public string FromClause => 
                Schema.IsEmpty()
                    ? " FROM [" + Name + "] "
                    : " FROM [" + Schema + "].[" + Name + "] ";
            public string SelectSql => "SELECT " + FieldList + FromClause;
            public string CountSql => "SELECT COUNT(*) " + FromClause;
        }

        /// <summary>
        /// A helper class to help define the columns in an underlying table
        /// </summary>
        [Serializable]
        public class TableColumn
        {
            public bool AutoIncrement;
            public string ColumnName;
            public string Description;
            public DbType DataType;
            public string DomainName;
            public bool IsForeignKey;
            public bool IsIdentity;
            public bool IsIndexed;
            public bool IsNullable;
            public bool IsPrimaryKey;
            public int MaxLength;
            public int Precision;
            public int Scale;
            public string SourceType;

            /// <summary>C#CD: </summary>
            public TableColumn() { }

            /// <summary>C#CD: </summary>
            /// <param name="columnName">C#CD: </param>
            /// <param name="dbType">C#CD: </param>
            /// <param name="isPrimaryKey">C#CD: </param>
            /// <param name="isForeignKey">C#CD: </param>
            public TableColumn(string columnName, DbType dbType, bool isPrimaryKey, bool isForeignKey)
            {
                ColumnName = columnName;
                IsPrimaryKey = isPrimaryKey;
                IsForeignKey = isForeignKey;
                DataType = dbType;
            }

            public TableColumn(string columnName, DbType dbType, bool autoIncrement, int maxLength, bool isNullable, bool isPrimaryKey, bool isForeignKey)
            {
                ColumnName = columnName;
                IsPrimaryKey = isPrimaryKey;
                IsForeignKey = isForeignKey;
                DataType = dbType;
                AutoIncrement = autoIncrement;
                MaxLength = maxLength;
                IsNullable = isNullable;
            }
        }

        /// <summary>C#CD: </summary>
        [Serializable]
        public class TableColumnCollection : List<TableColumn>
        {
            public TableColumn PrimaryKeyColumn;

            private Dictionary<string, TableColumn> _iList = new();

            #region Collection Methods

            public new void Add(TableColumn column)
            {
                _iList.Add(column.ColumnName.ToLower(), column);
                base.Add(column);
            }

            /// <summary>C#CD: </summary>
            /// <param name="name">C#CD: </param>
            /// <param name="dbType">C#CD: </param>
            /// <param name="isNullable">C#CD: </param>
            /// <param name="isPrimaryKey">C#CD: </param>
            /// <param name="isForeignKey">C#CD: </param>
            public void Add(string name, DbType dbType, bool isNullable, bool isPrimaryKey, bool isForeignKey)
            {
                var col = new TableColumn
                {
                    IsPrimaryKey = isPrimaryKey,
                    IsForeignKey = isForeignKey,
                    IsNullable = isNullable,
                    DataType = dbType,
                    ColumnName = name
                };

                if (!Contains(name))
                {
                    Add(col);
                    _iList.Add(name.ToLower(), col);
                }
                if (isPrimaryKey)
                    PrimaryKeyColumn = col;
            }

            /// <summary>C#CD: </summary>
            /// <param name="name">C#CD: </param>
            /// <param name="dbType">C#CD: </param>
            /// <param name="isNullable">C#CD: </param>
            public void Add(string name, DbType dbType, bool isNullable)
            {
                // ReSharper disable once IntroduceOptionalParameters.Global
                Add(name, dbType, isNullable, false, false);
            }

            public bool Contains(string columnName)
            {
                return _iList.ContainsKey(columnName.ToLower());
            }

            #endregion Collection Methods

            /// <summary>C#CD: </summary>
            /// <param name="columnName">C#CD: </param>
            public TableColumn GetColumn(string columnName)
            {
                columnName = columnName.ToLower();
                if (_iList.ContainsKey(columnName))
                    return _iList[columnName];
                return null;
            }

            /// <summary>C#CD: </summary>
            public TableColumn GetPrimaryKey()
            {
                if (PrimaryKeyColumn != null)
                    return PrimaryKeyColumn;

                TableColumn coll = null;
                foreach (var child in this)
                {
                    if (child.IsPrimaryKey)
                    {
                        coll = child;
                        break;
                    }
                }
                PrimaryKeyColumn = this[0];
                return coll;
            }
        }

        /// <summary>
        /// This is an intermediary class that holds the current value of a table column
        /// for each object instance.
        /// </summary>
        [Serializable]
        public class TableColumnSetting
        {
            /// <summary>C#CD: </summary>
            private string _columnName;

            /// <summary>C#CD: </summary>
            private object _currentValue;

            /// <summary>C#CD: </summary>
            public string ColumnName
            {
                get => _columnName;
                set => _columnName = value;
            }

            /// <summary>C#CD: </summary>
            public object CurrentValue
            {
                get => _currentValue;
                set
                {
                    if (value is int intValue && intValue == int.MinValue)
                        _currentValue = DBNull.Value;
                    else if (value is DateTime dateValue && dateValue == DateTime.MinValue)
                        _currentValue = DBNull.Value;
                    else
                        _currentValue = value;
                }
            }
        }

        /// <summary>C#CD: </summary>
        [Serializable]
        public class TableColumnSettingCollection : Dictionary<string, TableColumnSetting>
        {
            /// <summary>C#CD: </summary>
            /// <param name="columnName">C#CD: </param>
            /// <returns>C#CD: </returns>
            public object GetValue(string columnName)
            {
                columnName = columnName.ToLower();
                if (ContainsKey(columnName))
                    return this[columnName].CurrentValue;
                return null;
            }

            /// <summary>C#CD: </summary>
            /// <param name="columnName">C#CD: </param>
            /// <param name="oVal">C#CD: </param>
            public void SetValue(string columnName, object oVal)
            {
                columnName = columnName.ToLower();
                if (!ContainsKey(columnName))
                {
                    var setting = new TableColumnSetting {ColumnName = columnName, CurrentValue = oVal};
                    Add(columnName.ToLower(), setting);
                }
                else
                {
                    this[columnName].CurrentValue = oVal;
                }
            }

            private bool Contains(string columnName)
            {
                return ContainsKey(columnName.ToLower());
            }
        }

        [Serializable]
        public class TableIndex
        {
            public List<string> Columns = new();
            public bool IsClustered;
            public string Name;

            public TableIndex()
            {
            }

            public TableIndex(IDataReader rdr)
            {
                Name = rdr["name"].ToString();
                IsClustered = rdr["isclustered"].ToString() == "1";
                Columns.Add(rdr["column"].ToString());
            }
        }

        [Serializable]
        public class TableIndexCollection : List<TableIndex>
        {
        }

        [Serializable]
        public class TableKey
        {
            [XmlArray(ElementName = "Columns")]
            [XmlArrayItem(typeof(TableKeyColumn), ElementName = "Column")]
            public List<TableKeyColumn> Columns = new();
            public bool IsForeign;
            public bool IsPrimary;
            public bool IsReference;
            public string Name;
        }

        [Serializable]
        public class TableKeyCollection : List<TableKey>
        {
            public TableKey Find(string toFind)
            {
                foreach (var currKey in this)
                    if (currKey.Name.ToLower() == toFind.ToLower())
                        return currKey;
                return null;
            }
        }

        [Serializable]
        public class TableKeyColumn
        {
            public string Column;
            public string Related;

            public TableKeyColumn()
            {
            }

            public TableKeyColumn(string column, string related)
            {
                Column = column;
                Related = related;
            }
        }
    }
}