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
        #region Decortaion_MHWG
        public static int GetDecortaionHRByMHWGName(string name)
        {
            int idx = name.LastIndexOf('/');
            if (idx < 0 ||
                idx >= name.Length)
                return 0;

            string key = name.Substring(idx + 1, name.Length - idx - 1);
            var splitter = key.Split('_');

            if (splitter.Length < 2)
                return 0;

            var hrTexts = splitter[1].Split('.');
            if (hrTexts.Length < 2)
                return 0;

            return Convert.ToInt32(hrTexts[0]);
        }

        public static void DoDecortaionFileMHWG(string file)
        {
            //string fileName = Path.GetFileNameWithoutExtension(file);
            HtmlNode node = GetRootNodeFromFile(file);
            if (node == null ||
                !node.HasChildNodes)
                return;
            var tdbodies = node.SelectNodes("//div[@class ='col-md-4']/table[@class = 't1']/tbody");
            if (tdbodies == null)
                return;
            foreach (var tdbody in tdbodies)
            {
                var trNodes = tdbody.SelectNodes("tr");
                if (trNodes == null||
                    trNodes.Count<2)
                    continue;

                // tr[0].td value 
                // tr[1] from
                var tdValueNode = trNodes[0].SelectNodes("td");
                var tdFromNode = trNodes[1].SelectSingleNode("td");
                if (tdValueNode == null ||
                    tdValueNode.Count<2 ||
                    tdFromNode == null)
                    continue;

                var imgNode = tdValueNode[0].SelectSingleNode("img");
                if (imgNode == null)
                    continue;


                string hrName = imgNode.GetAttributeValue("src", null);
                if (string.IsNullOrEmpty(hrName))
                    continue;

                string skillName = RemoveCharactor(tdValueNode[1].InnerText, commonRemoveText);
                Skill refSkill = SourceData.m_source.GetNamedSkill(skillName, 1);
                if (refSkill == null)
                    continue;
                
                var nameNode = tdValueNode[0].SelectSingleNode("a");
                if(nameNode == null)
                    continue;

                int hr = GetDecortaionHRByMHWGName(hrName);

                // name
                logic.Decoration decoration = new logic.Decoration();
                decoration.hr = hr;
                decoration.name = nameNode.InnerText;
                decoration.from = RemoveCharactor(tdFromNode.InnerText, commonRemoveText);
                decoration.slot = SkillObject.Slot.Decoration;

                string slotLevelText = ExtractNumberText(nameNode.InnerText);
                decoration.slotLevel = Convert.ToInt32(slotLevelText);

                decoration.skills.Add(refSkill);

                SourceData.m_source.namedDecoration[decoration.name] = decoration;
                SourceData.m_source.AddSkill2SkillObject(decoration, refSkill);
            }
        }
        #endregion

        public static void DoDecoration()
        {
            string dir = rootDir + "/decoration/";
            if (!Directory.Exists(dir))
                return;

            foreach (var file in Directory.EnumerateFiles(dir, "*.html"))
            {
                DoDecortaionFileMHWG(file);
            }
        }
    }
}
