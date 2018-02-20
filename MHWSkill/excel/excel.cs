using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace PG.Excel
{
	public class Field
	{
		public string Name;
		public int Index;
		public Field[] ArrayData = null;
		public int ArrayIndex = 0;
	};
	public class FieldSet
	{
		public Field[] Fields;
		public Dictionary<string, Field[]> NameFields;
		public bool HasQueryField = false;
		protected Field idField;
		public Field IdField
		{
			get
			{
				if(idField!=null)
				{
					return idField;
				}
				var fields = NameFields.MHW_TryGetValue<string,Field[]>("id");
				if (fields!=null)
				{
					idField = fields[0];
				}
				return idField;
			}
		}
		public int Count
		{
			get { return Fields.Length; }
		}
		public Field this[ int idx ]
		{
			get
			{
				return Fields[ idx ];
			}
		}
		public Field[] this[ string name ]
		{
			get
			{
				if( string.IsNullOrEmpty(name) ) return null;
				return NameFields[ name ];
			}
		}
	}

	public class Line
	{
		public Table Owner;
		public string Source;
		public string[] sourceSplited;
		public string[] Cells;
		public int LineIndex;

		public int IdIndex;

		protected int _ID = -1;
		public int ID
		{
			get
			{
				if (_ID != -1)
					return _ID;

				try
				{
					_ID = System.Convert.ToInt32(this[IdIndex]);
					return _ID;
				}
				catch(Exception )
				{

				}

				return -1;
			}
		}


		public string this[ int idx ]
		{
			get
			{
				if(sourceSplited == null)
				{
					sourceSplited = Source.Split('\t');
					for(int i = 0;i<sourceSplited.Length && i<Cells.Length;++i)
					{
						Cells[i] = sourceSplited[i];
					}
				}

				return Cells[ idx ];
			}
		}
	}
	public unsafe class Table
	{
		public FieldSet Fields = new FieldSet();
		public class Context
		{
			public string name;
			public bool sorted = false;
			public List<Line> Lines = new List<Line>();

			public int Comparer(Line left,Line right)
			{
				return left.ID.CompareTo(right.ID);
			}

			public void Sort()
			{
				if (sorted)
					return;

				Lines.Sort(Comparer);
				sorted = true;
			}
		}

		public Dictionary<string, Context> Contexts = new Dictionary<string, Context>();
		public bool IgnoreTitle = true;

		public Table LoadTextFile(string uri,bool createTable = true)
		{
			if (!System.IO.File.Exists(uri))
				return null;

			Context context = new Context();
			context.name = uri;
			if(LoadText(context.Lines, uri))
			{
				Table tab = this;
				if(createTable)
				{
					tab = new Table();
					tab.Fields = Fields;
				}
				tab.Contexts[uri] = context;
				return tab;
			}
			return null;
		}

		public bool LoadJson(string root, string uri)
		{
			try
			{
				string configText = System.IO.File.ReadAllText(uri);
				var data = LitJson.JsonMapper.ToObject(configText);

				var fields = data["fields"];
				int fc = fields.Count;
				Fields.Fields = new Field[fc];
				Fields.NameFields = new Dictionary<string, Field[]>(fc);
				for (int fi = 0; fi < fc; ++fi)
				{
					var fd = fields[fi];
					Field f = new Field();
					f.Index = fi;
					f.Name = fd["name"].AsString;
					Fields.Fields[fi] = f;
					Field[] fv = Fields.NameFields.MHW_TryGetValue(f.Name);
					if (fv == null)
					{
						fv = new Field[] { f };
					}
					else
					{
						var tmp = new Field[fv.Length + 1];
						System.Array.Copy(fv, tmp, fv.Length);
						tmp[fv.Length] = f;
						fv = tmp;
					}
					Fields.NameFields[f.Name] = fv;
				}

				foreach (var kv in Fields.NameFields)
				{
					var fv = kv.Value;
					if (fv.Length <= 1) continue;
					for (int fi = 0; fi < fv.Length; ++fi)
					{
						fv[fi].ArrayData = fv;
						fv[fi].ArrayIndex = fi;
					}
				}

				const string keyFiles = "files";
				if (!data.ContainKey(keyFiles))
					return true;

				var files = data["files"];
				int filec = files.Count;
				for (int i = 0; i < filec; ++i)
				{
					var d = files[i].AsString;
					string u = root + d;
					u += ".txt";
					u = u.Replace("/", "\\");

					Context context = new Context();
					context.name = u;
					if (LoadText(context.Lines, u))
					{
						Contexts[u] = context;
					}
				}
				return true;
			}
			catch (Exception e)
			{
                Console.WriteLine("faild read file {0}\n{1}", uri, e.ToString());
				return false;
			}
		}
		
		Line AppendLine(string text,int line)
		{
			Line n = new Line();
			n.Owner = this;
			n.Cells = new string[ Fields.Count ];
			n.Source = text;
			n.LineIndex = line;
			var idField = Fields.IdField;
			if (idField != null)
				n.IdIndex = Fields.IdField.Index;
			else
				n.IdIndex = 0;
			return n;
		}

		protected bool LoadAllText(List<Line> Lines,string[] textLines)
		{
			int i = 0;
			foreach (var r in textLines)
			{
				if (i++ > 0)
				{
					Lines.Add(AppendLine(r,i));
				}
			}
			return true;
		}

		protected bool LoadCopyText(List<Line> Lines, string uri)
		{
			try
			{
				if (System.IO.File.Exists(uri))
				{
					string fileName = System.IO.Path.GetFileName(uri);
					// copy file?
					string copy_uri = System.AppDomain.CurrentDomain.BaseDirectory + "/tmp/" + fileName + "_tmp_";

					if (System.IO.File.Exists(copy_uri))
					{
						System.IO.File.Delete(copy_uri);
					}
					System.IO.File.Copy(uri, copy_uri, true);
					string[] rd = System.IO.File.ReadAllLines(copy_uri, Encoding.Unicode);
					if (System.IO.File.Exists(copy_uri))
					{
						System.IO.File.Delete(copy_uri);
					}
					LoadAllText(Lines, rd);
				}
				return true;
			}
			catch(Exception e)
			{
				Console.WriteLine("faild read file on safemode {0}\n{1}", uri, e.ToString());
			}
			return false;
		}

		public bool LoadText(List<Line> Lines,string uri)
		{
			if (!System.IO.File.Exists(uri))
				return false;

			try
			{
				string[] rd = System.IO.File.ReadAllLines(uri, Encoding.Unicode);
				LoadAllText(Lines, rd);
				return true;
			}
			catch(Exception e)
			{
				Console.WriteLine("faild read file {0}\n{1}", uri, e.ToString());
				if (LoadCopyText(Lines, uri))
					return true;
			}
			return false;
		}
	}

	public class TableMgr
	{
		public string m_root;
		protected Table m_tableIndex;
		public Dictionary<string, Table> m_tabMgr = new Dictionary<string, Table>();
		public void ReadMBIndex(string root,string path)
		{
			if(root[root.Length - 1]!='/')
			{
				root += '/';
			}

			m_tabMgr.Clear();
			m_root = root;

			// index
			var name = path.Substring(root.Length);
			m_tableIndex = new Table();
			if (!m_tableIndex.LoadJson(root, path))
				return;

			Field id = m_tableIndex.Fields.IdField;
			if(id == null)
			{
				return;
			}

			Field[] fileFields = m_tableIndex.Fields["file"];
			if (fileFields == null||
				fileFields.Length<=0)
			{
				return;
			}

			Field f = fileFields[0];
			foreach(var kv in m_tableIndex.Contexts)
			{
				List<Line> Lines = kv.Value.Lines;
				foreach (var line in Lines)
				{
					string txt = line[f.Index];
					if (txt != null)
					{
						string fullPath = root + "/json/"+ txt + ".json";
						if (!System.IO.File.Exists(fullPath))
							continue;

						var tab = new Table();
						if (!tab.LoadJson(root, fullPath))
							continue;
						foreach(var context in tab.Contexts)
						{
							m_tabMgr[context.Key] = tab;
						}
					}
				}
			}
		}
	    
		public Table GetTable(string name)
		{
			Table tab = m_tabMgr.MHW_TryGetValue<string, Table>(name);
			return tab;
		}
	}

	public static class MHWExt
	{
		public static V MHW_TryGetValue<K, V>(this Dictionary<K, V> d, K k)
		{
			V v = default(V);
			d.TryGetValue(k, out v);
			return v;
		}

		public static bool VerifyDir(string dir)
		{
			if (System.IO.Directory.Exists(dir)) return true;

			if (!VerifyDir(System.IO.Path.GetDirectoryName(dir))) return false;

			System.IO.Directory.CreateDirectory(dir);
			return true;
		}
	}
}

