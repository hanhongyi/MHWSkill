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
        #region CHARM_MHCHINESE

        public static void HandleCharm(string name, string nameZhcn, int value, HtmlNodeCollection skills)
        {
            List<Skill> lsSkills = new List<Skill>();
            foreach(var skill in skills)
            {
                var skillName = skill.InnerText;
                if (string.IsNullOrEmpty(skillName))
                    continue;
                // 
                Skill refSkill = SourceData.m_source.GetNamedSkill(skillName, value);
                if (refSkill == null)
                    continue;

                lsSkills.Add(refSkill);
            }

            if (lsSkills.Count <= 0)
                return;

            logic.Charm charm = new logic.Charm();
            charm.name = name;
            charm.nameZHCN = name;
            charm.skills = lsSkills;
            charm.slot = SkillObject.Slot.Charm;

            SourceData.m_source.namedCharms[charm.name] = charm;

            foreach(var skill in lsSkills)
                SourceData.m_source.AddSkill2SkillObject(charm, skill);
        }

        public static void DoCharmsFileMHCHINESE(string file)
        {
            //string fileName = Path.GetFileNameWithoutExtension(file);
            HtmlNode node = GetRootNodeFromFile(file);
            if (node == null ||
                !node.HasChildNodes)
                return;
            var tdbodies = node.SelectNodes("//table[@class ='simple-table']/tbody");
            if (tdbodies == null)
                return;
            foreach (var tdbody in tdbodies)
            {
                var titleNode = tdbody.SelectSingleNode("tr/th");
                var valueNode = tdbody.SelectSingleNode("tr/td");
                if (titleNode == null ||
                    valueNode == null)
                    continue;

                var titleANode = titleNode.SelectSingleNode("a");
                var vauleANode = valueNode.SelectNodes("a");
                if (titleANode == null ||
                    vauleANode == null)
                    continue;

                string charmJPName = ExtractJPNameFromHtmlNode(titleANode, "data-original-title", "日文:", "<br>");
                if (string.IsNullOrEmpty(charmJPName))
                    continue;

                string charmName = titleANode.InnerText;
                // multi charms?

                bool added = false;
                var thName = titleNode.InnerText;
                if (thName.Length > 1)
                {
                    string[] levelNames = thName.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    int value = 0;
                    for (int i = 0; i < levelNames.Length; ++i)
                    {
                        string levelName = levelNames[i];
                        if (string.IsNullOrEmpty(levelName) ||
                            levelName == charmName)
                            continue;
                        added = true;
                        ++value;
                        try
                        {
                            HandleCharm(charmJPName + levelName, charmName + levelName, value, vauleANode);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }

                if (!added)
                {
                    HandleCharm(charmJPName + "I", charmName + "I", 1, vauleANode);
                }
            }
        }
        #endregion

        public static void DoCharm()
        {
            string charmsDir = rootDir + "/charms/";
            if (!Directory.Exists(charmsDir))
                return;

            foreach (var file in Directory.EnumerateFiles(charmsDir, "*.html"))
            {
                DoCharmsFileMHCHINESE(file);
            }
        }
    }
}
