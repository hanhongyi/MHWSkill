using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;

namespace MHWSkill.logic
{
    public partial class HtmlDumper
    {
        static string rootDir = System.Environment.CurrentDirectory + "/../data/html_source";

        static readonly string ZHCNHeader = "日文: ";
        static readonly string ZHCNSplitter = "<br />";
        public static string ExtractJPNameFromHtmlNode(HtmlNode node, string attributeName = null, 
                                                        string header = null, string splitter = null)
        {
            attributeName = string.IsNullOrEmpty(attributeName) ? "data-original-title" : attributeName;
            header = string.IsNullOrEmpty(header) ? ZHCNHeader : header;
            splitter = string.IsNullOrEmpty(splitter) ? ZHCNSplitter : splitter;

            string fullText = node.GetAttributeValue(attributeName, "");
            if (string.IsNullOrEmpty(fullText))
                return null;
            // 日文: 
            int idx = fullText.IndexOf(header);
            if (idx < 0 ||
                idx >= fullText.Length)
                return null;
            //<br />
            int idxSplitter = fullText.IndexOf(splitter);
            if (idxSplitter < 0 ||
                idxSplitter >= fullText.Length)
                return null;
            // real jp name
            int idxSkillName = idx + header.Length;
            return fullText.Substring(idxSkillName, idxSplitter - idxSkillName);
        }

        public static char[] commonRemoveText = new char[] { ' ', '\r', '\n' };
        public static string RemoveCharactor(string names, char[] charactor)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var name in names)
            {
                bool insert = true;
                foreach (var c in charactor)
                {
                    if (c == name)
                    {
                        insert = false;
                        break;
                    }
                }
                if (insert)
                    sb.Append(name);
            }
            return sb.ToString();
        }

        public static string ExtractNumberText(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var ch in text)
            {
                if ((ch >= '0' && ch <= '9'))
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        public static HtmlNode GetRootNodeFromFile(string file)
        {
            HtmlDocument doc = new HtmlDocument();
            try
            {
                doc.Load(file);
            }
            catch (Exception exp)
            {
                Console.WriteLine("charms {0}", exp.ToString());
                return null;
            }
            return doc.DocumentNode;
        }
    
        public static void Do()
        {
            if (!Directory.Exists(rootDir))
                return;

            SourceData.Reload();
            DoSkills();
            DoMissedSkills();
            DoEquipment();
            DoCharm();
            DoDecoration();
            SourceData.Build();
        }
    }
}
