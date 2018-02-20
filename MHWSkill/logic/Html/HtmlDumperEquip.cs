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
        #region EQUIP_MHWG
        public class SkillMHWG
        {
            public string name;
            public string type;
            public string value;
        }

        public struct EquipmentDescMHWG
        {
            public string name;
            public Equipment.Slot slot;
            public int hr;
            public string defense;
            public string fire;
            public string water;
            public string thunder;
            public string ice;
            public string dragon;
            public List<SkillMHWG> skills;
            public string decoration;
        }

        public static string GetDecorationTextMHWG(string source)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var ch in source)
            {
                switch (ch)
                {
                    case '-':
                    case '①':
                    case '②':
                    case '③':
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }

        public static string GetSetCountTextMHWG(string source)
        {
            return ExtractNumberText(source);
        }

        public static string GetDefenseTextMHWG(string source)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var ch in source)
            {
                if((ch>='0' && ch <= '9')||
                    (ch == '/'))
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        public static Equipment HandleEquip(ref EquipmentDescMHWG equipDesc)
        {
            string defense = GetDefenseTextMHWG(equipDesc.defense);
            string[] defenseTexts = defense.Split('/');
            if(defenseTexts.Length < 2)
                return null;

            Equipment equip = new Equipment();
            equip.vs = new Equipment.VSAttribute();
            equip.id = SourceData.EquipId;
            equip.name = equipDesc.name;
            equip.hr = equipDesc.hr;
            equip.slot = equipDesc.slot;
            equip.defense = Convert.ToInt32(defenseTexts[0]);
            equip.defenseMax = Convert.ToInt32(defenseTexts[1]);
            equip.vs.fire = (Int16)Convert.ToInt16(equipDesc.fire);
            equip.vs.water = (Int16)Convert.ToInt16(equipDesc.water);
            equip.vs.thunder = (Int16)Convert.ToInt16(equipDesc.thunder);
            equip.vs.ice = (Int16)Convert.ToInt16(equipDesc.ice);
            equip.vs.dragon = (Int16)Convert.ToInt16(equipDesc.dragon);
            string zhCN = null;
            if (SourceData.m_source.equipJP2ZHCN.TryGetValue(equipDesc.name, out zhCN))
            {
                equip.nameZHCN = zhCN;
            }
            else
            {
                foreach(var kv in SourceData.m_source.equipJP2ZHCN)
                {
                    if(equipDesc.name.Contains(kv.Key))
                    {
                        zhCN = kv.Value;
                        int idx = equipDesc.name.IndexOf(kv.Key);
                        int keyLength = kv.Key.Length;
                        string name2 = equipDesc.name.Substring(idx + keyLength, equipDesc.name.Length - keyLength);

                        equip.nameZHCN = zhCN + name2;
                        break;
                    }
                }
            }
            foreach (var descSkill in equipDesc.skills)
            {
                int value = Convert.ToInt32(descSkill.value);
                Skill refSkill = SourceData.m_source.GetNamedSkill(descSkill.name, value);
                if (refSkill == null)
                    continue;
                //Skill skill = new Skill();
                ////skill.id = SourceData.SkillId;
                //skill.name = descSkill.name;
                ////string value = skills.value.Remove('+');
                //skill.value = Convert.ToInt32(descSkill.value);
                equip.skills.Add(refSkill);

                // skill - equip
                SourceData.m_source.AddSkill2Equipment(equip, refSkill);
            }
            
            string decorations = GetDecorationTextMHWG(equipDesc.decoration);
            int count = 0;
            bool containsDecoration = false;
            foreach (var ch in decorations)
            {
                switch(ch)
                {
                    case '-':
                        {
                            equip.decoration[count] = 0;
                        }
                        break;
                    case '①':
                        {
                            equip.decoration[count] = 1;
                            containsDecoration = true;
                        }
                        break;
                    case '②':
                        {
                            equip.decoration[count] = 2;
                            containsDecoration = true;
                        }
                        break;
                    case '③':
                        {
                            equip.decoration[count] = 3;
                            containsDecoration = true;
                        }
                        break;
                }

                if (++count >= 3)
                    break;
            }
            if(containsDecoration)
            {
                SourceData.m_source.AddDecortaionEquip(equip);
            }

            //Add equipment successed;
            SourceData.m_source.AddEquipment(equip);
            return equip;
        }

        public static Equipment.Slot GetSlotByMHWGName(string name)
        {
            int idx = name.LastIndexOf('/');
            if(idx<0 || 
                idx>=name.Length)
                return Equipment.Slot.MAX;

            string key = name.Substring(idx + 1, name.Length - idx - 1);
            var splitter = key.Split('-');

            if (splitter.Length < 2)
                return Equipment.Slot.MAX;

            switch (splitter[0])
            {
                case "b1":
                    return Equipment.Slot.Helms;
                case "b2":
                    return Equipment.Slot.Chests;
                case "b3":
                    return Equipment.Slot.Arms;
                case "b4":
                    return Equipment.Slot.Waist;
                case "b5":
                    return Equipment.Slot.Legs;  
            }

            return Equipment.Slot.MAX;
        }
        
        public static Equipment DoEquipmentFileMHWG_Equip(HtmlNode tr, int hr)
        {
            var td_collection = tr.SelectNodes("td");
            if (td_collection.Count != 9)
                return null;
            // equip data 
            HtmlNode equipRoot = td_collection[0];
            if (equipRoot == null)
                return null;

            HtmlNode nameNode = equipRoot.SelectSingleNode("a");
            if (nameNode == null)
                return null;

            HtmlNode imgNode = equipRoot.SelectSingleNode("img");
            if (imgNode == null)
                return null;

            string slotName = imgNode.GetAttributeValue("src", "");
            if (string.IsNullOrEmpty(slotName))
                return null;

            Equipment.Slot equipSlot = GetSlotByMHWGName(slotName);
            // known equipment slot
            if (equipSlot == Equipment.Slot.MAX)
                return null;

            var skillNames = td_collection[7].SelectNodes("a");
            var skillValues = td_collection[7].SelectNodes("span");
            if (skillNames == null ||
                skillValues == null ||
                skillNames.Count != skillValues.Count)
                return null;

            string name = nameNode.InnerText;
            EquipmentDescMHWG desc;
            desc.name = nameNode.InnerText;
            desc.slot = equipSlot;
            desc.defense = td_collection[1].InnerText;
            desc.fire = td_collection[2].InnerText;
            desc.water = td_collection[3].InnerText;
            desc.thunder = td_collection[4].InnerText;
            desc.ice = td_collection[5].InnerText;
            desc.dragon = td_collection[6].InnerText;
            desc.decoration = td_collection[8].InnerText;
            desc.skills = new List<SkillMHWG>();
            desc.hr = hr;

            for (int j = 0; j < skillValues.Count; ++j)
            {
                SkillMHWG skillMHWG = new SkillMHWG();
                skillMHWG.name = skillNames[j].InnerText;
                skillMHWG.type = skillValues[j].GetAttributeValue("class", "");
                skillMHWG.value = skillValues[j].InnerText;
                desc.skills.Add(skillMHWG);
            }

            return HandleEquip(ref desc);
        }

        public static List<Skill> DoEquipmentFileMHWG_EquipSet(HtmlNode tr)
        {
            var tdRoot = tr.SelectSingleNode("td[@class = 'left']");
            if (tdRoot == null)
                return null;

            var tdCollection = tdRoot.SelectNodes("span[@class = 'c_p b']");
            if (tdCollection == null)
                return null;

            int tdCollectionCount = tdCollection.Count;
            if (tdCollectionCount <= 0)
                return null;

            string setName = tdCollection[0].InnerText;
            if (setName == null)
                return null;

            var settdCollection = tdRoot.SelectNodes("span[@class = 'c_g b']");
            if (settdCollection == null)
                return null;

            var urlCollection = tdRoot.SelectNodes("a");
            if (urlCollection.Count <= 0 ||
                urlCollection.Count < settdCollection.Count)
                return null;

            List<Skill> lsSkill = null;
            for (int i = 0; i < settdCollection.Count; ++i)
            {
                string skillName = urlCollection[i].InnerText;
                if (string.IsNullOrEmpty(skillName))
                    return null;
                string setCountFullText = settdCollection[i].InnerText;
                if(string.IsNullOrEmpty(setCountFullText))
                    return null;

                string setCountText = GetSetCountTextMHWG(setCountFullText);
                if (string.IsNullOrEmpty(setCountText))
                    return null;

                Skill setSkill = new Skill();
                setSkill.id = SourceData.SkillId;
                setSkill.setId = SourceData.SetSkillId;
                setSkill.setName = setName;
                setSkill.name = skillName;
                setSkill.setCount = Convert.ToInt32(setCountText);

                SourceData.m_source.AddSetSkill(setSkill);

                if(lsSkill == null)
                    lsSkill = new List<Skill>();
                lsSkill.Add(setSkill);
            }

            return lsSkill;
        }

        //mhwg.org
        public static void DoEquipmentFileMHWG(string file)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            int hr = Convert.ToInt32(fileName);
            if (hr <= 0)
                return;

            HtmlNode node = GetRootNodeFromFile(file);
            if (node == null ||
                !node.HasChildNodes)
                return;

            // var tr_collection = node.SelectNodes("//div[@class ='card card-success3']/table[@class = 't6 f_min']/tbody/tr");
          
            var node_collection = node.SelectNodes("//div[@class ='card card-success3']");
            foreach (var div in node_collection)
            {
                var table = div.SelectSingleNode("table[@class = 't6 f_min']");
                if (table == null)
                    continue;
                // selected table : all tr
                var tr_collection = table.SelectNodes("tbody/tr");
                if (tr_collection == null)
                    continue;

                List<Equipment> lsEquipment = new List<Equipment>();
                foreach(var tr in tr_collection)
                {
                    string tr_att = tr.GetAttributeValue("class", "");
                    if(!string.IsNullOrEmpty(tr_att))
                    {
                        Equipment equip = DoEquipmentFileMHWG_Equip(tr, hr);
                        if(equip!=null)
                            lsEquipment.Add(equip);
                    }
                    else
                    {
                        if (lsEquipment.Count <= 0)
                            continue;

                        string set_att = tr.GetAttributeValue("style", "");
                        if (string.IsNullOrEmpty(set_att))
                            continue;

                        var skills = DoEquipmentFileMHWG_EquipSet(tr);
                        if (skills != null)
                        {
                            foreach(var equip in lsEquipment)
                            {
                                foreach (var skill in skills)
                                {
                                    equip.AddSetSkill(skill);

                                    // skill - equip
                                    SourceData.m_source.AddSkill2Equipment(equip, skill);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region EQUIP_MHCHINESE

        public static void DoEquipmentNameFileMHCHINESE(string file)
        {
            HtmlNode node = GetRootNodeFromFile(file);
            if (node == null ||
                !node.HasChildNodes)
                return;

            var equipJP2ZHCN = logic.SourceData.m_source.equipJP2ZHCN;
            var equipZHCN2JP = logic.SourceData.m_source.equipZHCN2JP;

            var tdCollection = node.SelectNodes("//table[@class ='simple-table']/tbody/tr/td");
            foreach (var td in tdCollection)
            {
                // a
                var aNode = td.SelectSingleNode("a");
                if (aNode == null)
                    continue;
                // jp - chinese name
                string jpName = ExtractJPNameFromHtmlNode(aNode);
                if(!string.IsNullOrEmpty(jpName))
                {
                    equipJP2ZHCN[jpName] = aNode.InnerText;
                    equipZHCN2JP[aNode.InnerText] = jpName;
                }
            }
        }

        #endregion

        public static void DoEquipmentNames()
        {
            string equipmentNameDir = rootDir + "/equipmentName/";
            if (!Directory.Exists(equipmentNameDir))
                return;

            foreach (var file in Directory.EnumerateFiles(equipmentNameDir, "*.html"))
            {
                DoEquipmentNameFileMHCHINESE(file);
            }
        }

        public static void DoEquipment()
        {
            DoEquipmentNames();

            string equipmentDir = rootDir + "/equipment/";
            if (!Directory.Exists(equipmentDir))
                return;

            foreach(var file in Directory.EnumerateFiles(equipmentDir,"*.html"))
            {
                DoEquipmentFileMHWG(file);
            }
        }
    }
}
