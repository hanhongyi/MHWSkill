// Core algorithm
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWSkill.logic
{
    public class EquipSet
    {
        public SkillObject[] equipment = new SkillObject[(int)Equipment.Slot.MAX];
        public Decoration[,] decortaions = new Decoration[(int)Equipment.Slot.MAX, Decoration.MaxSlot];

        public int hr = 0;
        public int defense = 0;

        public int countIndex = 0;

        public bool filled = false;
        public int failedTurn = 0;

        public int[] lsSkill;

        public EquipSet(int skillCount)
        {
            lsSkill = new int[skillCount];
        }

        public void Copy(EquipSet right)
        {
            int i = 0;
            //foreach(var equip in right.equipment)
            for (; i < (int)Equipment.Slot.MAX;++i)
            {
                equipment[i] = right.equipment[i];
                for(int j = 0;j<Decoration.MaxSlot;++j)
                {
                    decortaions[i, j] = right.decortaions[i, j];
                }
            }

            for(i = 0;i<lsSkill.Length;++i)
            {
                lsSkill[i] = right.lsSkill[i];
            }

            countIndex = right.countIndex;
        }

        public void CalculateBySkillName(int idx, string skillName)
        {
            if(countIndex>=idx)
                return;
            lsSkill[idx] = 0; 
            countIndex = idx;
            for (int i = 0; i < (int)Equipment.Slot.MAX; ++i)
            {
                var equip = equipment[i];
                if (equip == null)
                    continue;
                lsSkill[idx] += equip.GetSkillValue(skillName);
                for (int j = 0; j < Decoration.MaxSlot; ++j)
                {
                    var decortaion = decortaions[i, j];
                    if(decortaion!=null)
                    {
                        lsSkill[idx] += decortaion.GetSkillValue(skillName);
                    }
                }
            }
        }

        public void DoCheck(int skillIndex,int value)
        {
            if (lsSkill[skillIndex] >= value)
            {
                filled = true;
            }
            else
            {
                filled = false;
                failedTurn = skillIndex;
            }
        }

        public void AddEquipment(SkillObject skillObject, int skillIndex, int value, string skillName)
        {
            equipment[(int)skillObject.slot] = skillObject;
            if (skillObject.slot != SkillObject.Slot.Charm)
            {
                Equipment equip = skillObject as Equipment;
                if (equip != null)
                {
                    hr += equip.hr;
                    defense += equip.defense;
                }
            }

            lsSkill[skillIndex] += skillObject.GetSkillValue(skillName);
            DoCheck(skillIndex, value);
        }

        public void AddDecoration(SkillObject skillObject, SkillObject equipment, int equipDecSlot, int skillIndex, int skillValue,
                    string skillName)
        {
            Decoration decoration = skillObject as Decoration;
            if (decoration == null ||
                equipment == null)
            {
                return;
            }

            hr += decoration.hr;
            decortaions[(int)equipment.slot, equipDecSlot] = decoration;

            AddEquipment(equipment, skillIndex, skillValue, skillName);
            lsSkill[skillIndex] += skillObject.GetSkillValue(skillName);

            DoCheck(skillIndex, skillValue);
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;

            return IsEqual(this, obj as EquipSet);
        }

        public static bool IsEqual(EquipSet left, EquipSet right)
        {
            if (right == null)
                return false;

            for(int i = 0;i<(int)Equipment.Slot.MAX;++i)
            {
                if (left.equipment[i] != right.equipment[i])
                    return false;

                var leftDecortaions = left.decortaions;
                var rightDecortaions = right.decortaions;

                if(leftDecortaions != null &&
                    rightDecortaions != null)
                {
                    for (int j = 0; j < logic.Decoration.MaxSlot; ++j)
                    {
                        if (leftDecortaions[i,j] != rightDecortaions[i,j])
                            return false;
                    }
                }
            }
            return true;
        }

        public static int SortCompare(EquipSet left, EquipSet right)
        {
            if (left.hr > right.hr)
                return 1;
            else if (left.hr < right.hr)
                return -1;

            if (left.defense > right.defense)
                return 1;
            else if (left.defense > right.defense)
                return -1;

            return 0;
        }
    }

    public partial class SourceData
    {
        public SearchParam m_param;

        public static EquipSet AddDecoration(Decoration decoration, EquipSet equipSet,
                List<EquipSet> tempEquipSet, int skillCount, int skillIndex, int skillValue, string skillName, 
                Predicate<Equipment> predicateEquip = null)
        {
            if (decoration == null)
                return null;

            for (int decIndex = decoration.slotLevel - 1; decIndex < Decoration.MaxLevel; ++decIndex)
            {
                foreach (var equip in SourceData.m_source.decorationEquipment[decIndex])
                {
                    if (predicateEquip!=null &&
                        !predicateEquip(equip))
                        continue;

                    // add single 
                    for (int equipDecSlot = 0; equipDecSlot < logic.Decoration.MaxSlot; ++equipDecSlot)
                    {
                        int slotLevel = equip.decoration[equipDecSlot];
                        if (slotLevel > decoration.slotLevel)
                        {
                            EquipSet newEquipSet = new EquipSet(skillCount);
                            if (equipSet!=null)
                            {
                                newEquipSet.Copy(equipSet);
                            }

                            Equipment newEquipment = new Equipment();
                            newEquipment.name = "任意";
                            newEquipment.nameZHCN = "任意";
                            newEquipment.decoration = equip.decoration;
                            newEquipment.slot = equip.slot;

                            newEquipSet.AddDecoration(decoration, newEquipment, equipDecSlot, skillIndex, skillValue, skillName);
                            tempEquipSet.Add(newEquipSet);

                            // only find one : fast mode
                            return newEquipSet;
                        }
                    }
                }
            }

            return null;
        }

        protected List<EquipSet> BuildEquipmentSet(List<Skill> skills)
        {
            List<EquipSet> tempEquipSet = new List<EquipSet>();

            // build equipset
            for (int skillIndex = 0; skillIndex < skills.Count; ++skillIndex)
            {
                var skill = skills[skillIndex];
                List<SkillObject> skillObjects;
                if (!skillName2SkillObject.TryGetValue(skill.name, out skillObjects))
                    return null;

                var skillName = skill.name;

                // First turn create all EquipSet
                if(skillIndex == 0)
                {
                    foreach (var skillObject in skillObjects)
                    {
                        if(skillObject.slot != SkillObject.Slot.Decoration)
                        {
                            EquipSet equipset = new EquipSet(skills.Count);
                            equipset.AddEquipment(skillObject, skillIndex, skill.value, skill.name);
                            tempEquipSet.Add(equipset);
                        }
                        else
                        {
                            if (!m_param.useDecoration)
                                continue;

                            // Decoration !!!
                            var decoration = skillObject as Decoration;
                            if (decoration == null)
                                continue;

                            //Add Decoration !!!
                            AddDecoration(decoration, null, tempEquipSet, skills.Count, skillIndex, skill.value, skill.name);
                        }
                    }
                }

                // Fill equipset
                for (int j = 0; j < tempEquipSet.Count;++j)
                {
                    var equipSet = tempEquipSet[j];
                    if (!equipSet.filled &&
                        equipSet.failedTurn < skillIndex)
                    {
                        continue;
                    }

                    if (equipSet.lsSkill[skillIndex] >= skill.value)
                    {
                        equipSet.filled = true;
                        continue;
                    }

                    equipSet.filled = false;
                    equipSet.failedTurn = skillIndex - 1;

                    // Calculate current skill
                    equipSet.CalculateBySkillName(skillIndex, skill.name);

                    foreach (var skillObject in skillObjects)
                    {
                        if (skillObject.slot != SkillObject.Slot.Decoration)
                        {
                            if (equipSet.equipment[(int)skillObject.slot] == null)
                            {
                                EquipSet newEquipSet = new EquipSet(skills.Count);
                                newEquipSet.Copy(equipSet);
                                newEquipSet.AddEquipment(skillObject, skillIndex, skill.value, skill.name);
                                newEquipSet.DoCheck(skillIndex, skill.value);
                                tempEquipSet.Add(newEquipSet);
                            }
                        }
                        else
                        {
                            if (!m_param.useDecoration)
                                continue;

                            // Decoration !!!
                            var decoration = skillObject as Decoration;
                            if (decoration == null)
                                continue;

                            //Add Decoration !!!

                            // equip empty slot
                            for (int equipSlot = (int)SkillObject.Slot.Helms; equipSlot <= (int)SkillObject.Slot.Legs; ++equipSlot)
                            {
                                if(equipSet.equipment[equipSlot] == null)
                                {
                                    // all empty slot !!!
                                    AddDecoration(decoration, equipSet, tempEquipSet, skills.Count, skillIndex, skill.value, skill.name,
                                        (testEquip)=>{
                                            if (testEquip == null ||
                                                (int)testEquip.slot != equipSlot)
                                                return false;
                                            return true;
                                    });
                                }
                                else
                                {
                                    // try insert!
                                    var equipment = equipSet.equipment[equipSlot] as Equipment;
                                    if (equipment == null)
                                        continue;
                                    for (int equipDecSlot = 0; equipDecSlot < Decoration.MaxSlot; ++equipDecSlot)
                                    {
                                        if(equipSet.decortaions[equipSlot,equipDecSlot] == null &&
                                           equipment.decoration[equipDecSlot] >= decoration.slotLevel)
                                        {
                                            EquipSet newEquipSet = new EquipSet(skills.Count);
                                            if (equipSet != null)
                                            {
                                                newEquipSet.Copy(equipSet);
                                            }
                                            newEquipSet.AddDecoration(decoration, equipment, equipDecSlot, skillIndex, skill.value, skillName);
                                            tempEquipSet.Add(newEquipSet);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return tempEquipSet;
        }

        //public static bool ContainsEquipSet(List<EquipSet> ls, EquipSet equipSet)
        //{
        //    foreach(var left in ls)
        //    {
        //        if (EquipSet.Equal(left, equipSet))
        //            return true;
        //    }
        //    return false;
        //}

        public static void InsertIntoOrModify(List<SkillValue> skillsFiter, Skill skill)
        {
            bool insert = true;
            for (int j = 0; j < skillsFiter.Count; ++j)
            {
                var s = skillsFiter[j];
                if (s.skill.name == skill.name)
                {
                    insert = false;
                    if (s.skill.value < skill.value)
                        skillsFiter[j].skill = skill;
                    break;
                }
            }
            if (insert)
            {
                SkillValue sv = new SkillValue();
                sv.skill = skill;
                sv.count = SourceData.m_source.GetNamedSkillObjectCount(skill.name);
                skillsFiter.Add(sv);
            }
        }

        public class SkillValue
        {
            public Skill skill;
            public int count;

            public static int CompareTo(SkillValue left, SkillValue right)
            {
                return left.count.CompareTo(right.count);
            }
        }

        public class SearchParam
        {
            public bool useDecoration = true;
        }

        public List<EquipSet> GetEquipmentSet(List<Skill> skills, SearchParam param)
        {
            if(skills == null)
                return null;

            m_param = param;

            //
            List<SkillValue> skillsFiter = new List<SkillValue>(skills.Count);
            List<SkillValue> setSkills = new List<SkillValue>(skills.Count);
            for(int i = 0;i<skills.Count;++i)
            {
                if(skills[i].setId == 0)
                {
                    InsertIntoOrModify(skillsFiter, skills[i]);
                }
                else
                {
                    InsertIntoOrModify(skillsFiter, skills[i]);
                }
            }

            skills.Clear();
            
            // sort search condition

            setSkills.Sort(SkillValue.CompareTo);
            skillsFiter.Sort(SkillValue.CompareTo);

            foreach(var v in setSkills)
            {
                skills.Add(v.skill);
            }

            foreach (var v in skillsFiter)
            {
                skills.Add(v.skill);
            }
      
            var tempEquipSet = BuildEquipmentSet(skills);
            if (tempEquipSet == null)
                return null;

            Dictionary<EquipSet, int> lsEquipSet = null;// new List<EquipSet>();
            foreach(var equipSet in tempEquipSet)
            {
                if(!equipSet.filled)
                    continue;
                if (lsEquipSet == null)
                {
                    lsEquipSet = new Dictionary<EquipSet, int>();
                }

                // contains?
                if (lsEquipSet.ContainsKey(equipSet))
                    continue;

                lsEquipSet[equipSet] = 1;
            }

            if (lsEquipSet == null)
                return null;

            List<EquipSet> ls = new List<EquipSet>();
            ls.AddRange(lsEquipSet.Keys);

            ls.Sort(EquipSet.SortCompare);
            
            return ls;
        }
    }
}
