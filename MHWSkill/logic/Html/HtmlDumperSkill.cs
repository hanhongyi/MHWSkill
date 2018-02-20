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
        #region SKILL_MHCHINESE
        
        public static void DoSkillFileMHCHINESE_Skill(HtmlNode tr, int category, ref string skillName)
        {
            const int minTdsCount = 3;
            var tds = tr.SelectNodes("td");
            if (tds == null ||
                tds.Count < minTdsCount)
                return;

            string rowspan = tds[0].GetAttributeValue("rowspan", "");
            int tdStartIdx = 0;
            if (!string.IsNullOrEmpty(rowspan))
            {
                // title
                if (tds.Count < minTdsCount + 1)
                    return;

                var skillNameNode = tds[0].SelectSingleNode("a");
                if (tds[0] == null)
                    return;
                /*
                string skillFullName = skillNameNode.GetAttributeValue("data-original-title", "");
                if (string.IsNullOrEmpty(skillFullName))
                    return;
                // 日文: 
                int idx = skillFullName.IndexOf(SkillNameHeader);
                if (idx < 0 ||
                    idx >= skillFullName.Length)
                    return;
                //<br />
                int idxSplitter = skillFullName.IndexOf(SkillNameHeaderSplitter);
                if (idxSplitter < 0 ||
                    idxSplitter >= skillFullName.Length)
                    return;
                // real jp name
                int idxSkillName = idx + SkillNameHeader.Length;
                skillName = skillFullName.Substring(idxSkillName, idxSplitter - idxSkillName);
                */
                skillName = ExtractJPNameFromHtmlNode(skillNameNode);
                tdStartIdx = 1;                
            }

            // context
            logic.Skill skill = new Skill();
            skill.id = SourceData.SkillId;
            skill.name = skillName;
            skill.nameZHCN = tds[tdStartIdx].InnerText;
            skill.value = Convert.ToInt32(tds[tdStartIdx+1].InnerText);
            skill.desc = tds[tdStartIdx+2].InnerText;
            skill.category = (Skill.Category)category;
            SourceData.m_source.AddSkill(skill);
        }

        // mhchinese
        public static void DoSkillFileMHCHINESE(string file)
        {
            //string fileName = Path.GetFileNameWithoutExtension(file);
            string fileName = Path.GetFileNameWithoutExtension(file);
            int category = Convert.ToInt32(fileName);
            if (category <= 0)
                return;

            HtmlNode node = GetRootNodeFromFile(file);
            if (node == null||
                !node.HasChildNodes)
                return;
            var trs = node.SelectNodes("//table[@class ='simple-table']/tbody/tr");
            if (trs == null)
                return;
            string skillName = "";
            foreach (var tr in trs)
            {
                DoSkillFileMHCHINESE_Skill(tr, category,ref skillName);
            }
        }

        #endregion

        #region MISSED_SKILL_MHCHINESE

        public static void DoMissedSkillFileMHCHINESE_Skill(HtmlNode tr, int category)
        {
            var tds = tr.SelectNodes("td");
            if (tds == null)
                return;

            foreach(var td in tds)
            {
                var aNode = td.SelectSingleNode("a");
                if (aNode == null)
                    continue;
                /*
                string skillFullName = aNode.GetAttributeValue("data-original-title", "");
                if (string.IsNullOrEmpty(skillFullName))
                    continue;
                // 日文: 
                int idx = skillFullName.IndexOf(SkillNameHeader);
                if (idx < 0 ||
                    idx >= skillFullName.Length)
                    continue;
                //<br />
                int idxSplitter = skillFullName.IndexOf(SkillNameHeaderSplitter);
                if (idxSplitter < 0 ||
                    idxSplitter >= skillFullName.Length)
                    continue;

                // real jp name
                int idxSkillName = idx + SkillNameHeader.Length;
                string skillName = skillFullName.Substring(idxSkillName, idxSplitter - idxSkillName);
                */

                string skillName = ExtractJPNameFromHtmlNode(aNode);
                if (skillName == null)
                    continue;

                logic.Skill skill = SourceData.m_source.GetNamedSkill(skillName, 1);
                if (skill != null)
                    continue;

                // context
                skill = new Skill();
                skill.id = SourceData.SkillId;
                skill.name = skillName;
                skill.nameZHCN = aNode.InnerText;
                skill.value = 1;
                skill.desc = "";
                skill.category = (Skill.Category)category;
                SourceData.m_source.AddSkill(skill);
            }
        }

        // mhchinese
        public static void DoMissedSkillFileMHCHINESE(string file)
        {
            //string fileName = Path.GetFileNameWithoutExtension(file);
            string fileName = Path.GetFileNameWithoutExtension(file);
            
            int category = 1;
            HtmlDocument doc = new HtmlDocument();
            try
            {
                doc.Load(file);
            }
            catch (Exception exp)
            {
                Console.WriteLine("missed skill {0}", exp.ToString());
                return;
            }

            HtmlNode node = doc.DocumentNode;
            if (node == null ||
                !node.HasChildNodes)
                return;
            var trs = node.SelectNodes("//table[@class ='simple-table']/tbody/tr");
            if (trs == null)
                return;
            foreach (var tr in trs)
            {
                DoMissedSkillFileMHCHINESE_Skill(tr, category);
            }
        }

        #endregion

        public static void DoSkills()
        {
            string skillDir = rootDir + "/skill/";
            if (!Directory.Exists(skillDir))
                return;

            foreach (var file in Directory.EnumerateFiles(skillDir, "*.html"))
            {
                DoSkillFileMHCHINESE(file);
            }
        }

        public static void DoMissedSkills()
        {
            string skillDir = rootDir + "/missedSkill/";
            if (!Directory.Exists(skillDir))
                return;

            foreach (var file in Directory.EnumerateFiles(skillDir, "*.html"))
            {
                DoMissedSkillFileMHCHINESE(file);
            }
        }
    }
}
