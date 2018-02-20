using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWSkill.logic
{
    public class NamedObject
    {
        public int id;
        public string name;
        public string nameZHCN;
        public string DisplayName
        {
            get
            {
                return !string.IsNullOrEmpty(nameZHCN) ? nameZHCN : name;
            }
        }
    }

    public class Skill : NamedObject
    {
        public enum Type
        {
            // common skill
            Common = 0,
            // set skill
            Set = 1,
        }

        public enum Category
        {
            Attack = 1,
            Defense = 2,
            Resistance = 3,
            Archer = 4,
            HunterSkill = 5,
            Cat = 6,
            SetBonuse = 7,
        }
        
        public string setName;
        public int value;
        public string desc;
        public Skill.Type type = Skill.Type.Common;
        public Skill.Category category = Skill.Category.Attack;
        public int setId = 0;
        public int setCount = 0;
    };

    public class SkillObject : NamedObject
    {
        public List<Skill> skills = new List<Skill>();
        public enum Slot
        {
            Weapon = 0,
            Helms = 1,
            Chests = 2,
            Arms = 3,
            Waist = 4,
            Legs = 5,
            Charm = 6,
            Decoration = 7,

            MAX = 8,
        };

        public Slot slot;

        public bool ContainsSkill(Skill right)
        {
            for (int i = 0; i < skills.Count; ++i)
            {
                Skill left = skills[i];
                if (left != null &&
                    left.id != right.id &&
                    (left.setId == 0 || right.setId == 0 ||
                    left.setId != right.setId))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public int GetSkillValue(string name)
        {
            foreach (var v in skills)
            {
                if (v.name == name)
                    return v.value;
            }
            return 0;
        }
    }

    public class Equipment : SkillObject
    {
        public struct VSAttribute
        {
            public Int16 fire;
            public Int16 water;
            public Int16 thunder;
            public Int16 ice;
            public Int16 dragon;
        };

        public int hr;
        public int defense;
        public int defenseMax;
        public VSAttribute vs;
        public byte[] decoration = new byte[3];
       
        public static void WriteHeader(StringBuilder sb)
        {
            sb.Append("");
        }

        public void AddSetSkill(Skill right)
        {
            if (right.type != Skill.Type.Set ||
                ContainsSkill(right))
                return;

            Skill setSkill = new Skill();
            setSkill.type = right.type;
            setSkill.id = right.id;
            setSkill.setCount = right.setCount;
            setSkill.setId = right.id;
            setSkill.name = right.name;

            skills.Add(right);
        }

    };

    public class Decoration : SkillObject
    {
        public const int MaxLevel = 3;
        public const int MaxSlot = 3;

        public int slotLevel;
        public int hr;
        public string from;
    }

    public class Charm : SkillObject
    {
    }

    public partial class SourceData
    {
        public Dictionary<int, Equipment> equipment = new Dictionary<int, Equipment>();
        public Dictionary<int, Skill> skills = new Dictionary<int,Skill>();
        public Dictionary<int, Skill> setSkills = new Dictionary<int, Skill>();

        public Dictionary<string, List<Skill>> namedSkills = new Dictionary<string, List<Skill>>();
        public Dictionary<string, Skill> namedSetSkills = new Dictionary<string,Skill>();
        public Dictionary<string, Equipment> namedEquipment = new Dictionary<string,Equipment>();
        public Dictionary<string, Charm> namedCharms = new Dictionary<string, Charm>();
        public Dictionary<string, Decoration> namedDecoration = new Dictionary<string, Decoration>();
        public List<Equipment>[] decorationEquipment = new List<Equipment>[Decoration.MaxLevel];

        //public Dictionary<int, List<Equipment>> skill2Equipment = new Dictionary<int, List<Equipment>>();
        public Dictionary<string, List<SkillObject>> skillName2SkillObject = new Dictionary<string, List<SkillObject>>();

        public Dictionary<string, string> equipJP2ZHCN = new Dictionary<string, string>();
        public Dictionary<string, string> equipZHCN2JP = new Dictionary<string, string>();

        private int equipId = 0;
        public static int EquipId
        {
            get { return ++m_source.equipId; }
        }

        private int skillId = 0;
        public static int SkillId
        {
            get { return ++m_source.skillId; }
        }

        private int setSkillId = 0;
        public static int SetSkillId
        {
            get { return ++m_source.setSkillId; }
        }

        public void AddEquipment(Equipment equip)
        {
            equipment[equip.id] = equip;
            namedEquipment[equip.name] = equip;
        }

        public int GetNamedSkillObjectCount(string name)
        {
            List<SkillObject> lsNamedEquipment = null;
            if (!skillName2SkillObject.TryGetValue(name, out lsNamedEquipment))
            {
                return 0;
            }

            return lsNamedEquipment.Count;
        }

        public void AddDecortaionEquip(Equipment equip)
        {
            foreach(var v in equip.decoration)
            {
                if(v>0)
                    decorationEquipment[v - 1].Add(equip);
            }
        }

        public void AddSkill(Skill skill)
        {
            skills[skill.id] = skill;

            List<Skill> lsSkill = null;
            if(!namedSkills.TryGetValue(skill.name, out lsSkill))
            {
                lsSkill = new List<Skill>();
                namedSkills[skill.name] = lsSkill;
            }
            lsSkill.Add(skill);
        }

        public Skill GetNamedSkill(string name, int value)
        {
            List<Skill> lsSkill = null;
            if (!namedSkills.TryGetValue(name, out lsSkill)||
                lsSkill == null)
                return null;

            foreach(var skill in lsSkill)
            {
                if (skill.value == value)
                    return skill;
            }

            return null;
        }

        public void AddSetSkill(Skill setskill)
        {
            setSkills[setskill.id] = setskill;
            namedSetSkills[setskill.name] = setskill;
        }

        public void AddSkill2SkillObject(SkillObject skillObject, Skill skill)
        {
            List<SkillObject> lsNamedEquipment = null;
            if (!skillName2SkillObject.TryGetValue(skill.name, out lsNamedEquipment))
            {
                lsNamedEquipment = new List<SkillObject>();
                skillName2SkillObject[skill.name] = lsNamedEquipment;
            }
            if (lsNamedEquipment == null)
                return;
            lsNamedEquipment.Add(skillObject);
        }

        public void AddSkill2Equipment(Equipment equip, Skill skill)
        {
            AddSkill2SkillObject(equip, skill);
        }

        public void SaveEquipmentExcel(string path)
        {
            StringBuilder sb = new StringBuilder();
            //header
            sb.Append("name\tid\t");

            // write utf-16
            System.IO.File.WriteAllText(path, sb.ToString(), Encoding.Unicode);
        }

        static string rootDir = System.Environment.CurrentDirectory + "/../data";

        public static void SaveExcel()
        {
            m_source.SaveEquipmentExcel(rootDir + "/equipment");
        }

        public static void Reload()
        {
            m_source.OnReload();
        }

        void OnReload()
        {
            equipment.Clear();
            setSkills.Clear();
            namedSkills.Clear();
            namedSetSkills.Clear();
            namedEquipment.Clear();
            namedCharms.Clear();
            namedDecoration.Clear();
            //skill2Equipment.Clear();
            skillName2SkillObject.Clear();
            equipJP2ZHCN.Clear();

            for (int i = 0; i < decorationEquipment.Length;++i)
            {
                if (decorationEquipment[i] == null)
                    decorationEquipment[i] = new List<Equipment>();
                else
                    decorationEquipment[i].Clear();
            }

            equipId = 0;
            skillId = 0;
            setSkillId = 0;
        }

        public static void Build()
        {
            m_source.OnBuild();
        }

        void OnBuild()
        {
            
        }

        public static SourceData m_source = new SourceData();
    };

}
