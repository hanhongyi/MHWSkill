using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MHWSkill.ui
{
    public class ViewModelResultEquip : ViewModelBase
    {
        public enum Type
        {
            Background = 0,
            Equipment = 1,
            Charm = 2,
            Decoration = 3,
        }
        public int background { get; set; }

        public string name{ get; set; }
        public string slot{ get; set; }
        public int hr{ get; set; }
        public int defense{ get; set; }
        public int defenseMax { get; set; }
        public int fire { get; set; }
        public int water { get; set; }
        public int thunder { get; set; }
        public int ice { get; set; }
        public int dragon { get; set; }
        public string skills{ get; set; }
        public string decoration{ get; set; }
        
        public int idx = 0;
        public Type type;
        public object souce;
    }

    public class ViewModelResultEquipSet : ViewModelBase
    {
        public logic.EquipSet equipmentSet;
        public ViewModelResultEquip all;

        protected void DumpDecoration(ViewModelResultEquip viewEquipment, logic.Decoration decoration)
        {
            viewEquipment.slot = "镶嵌";
            viewEquipment.hr = decoration.hr;
        }

        protected void DumpCharm(ViewModelResultEquip viewEquipment, logic.Charm charm)
        {
            viewEquipment.slot = "护石";
        }

        protected void DumpEquipment(ViewModelResultEquip viewEquipment, logic.Equipment equip)
        {
            if (equip == null)
                return;

            viewEquipment.hr = equip.hr;
            viewEquipment.defense = equip.defense;
            all.defense += equip.defense;
            all.defenseMax += equip.defenseMax;
            viewEquipment.fire = equip.vs.fire;
            viewEquipment.water = equip.vs.water;
            viewEquipment.thunder = equip.vs.thunder;
            viewEquipment.ice = equip.vs.ice;
            viewEquipment.dragon = equip.vs.dragon;

            all.fire += equip.vs.fire;
            all.water += equip.vs.water;
            all.thunder += equip.vs.thunder;
            all.ice += equip.vs.ice;
            all.dragon += equip.vs.dragon;

            viewEquipment.decoration = "";
            foreach (var decoration in equip.decoration)
            {
                switch (decoration)
                {
                    case 0:
                        {
                            viewEquipment.decoration += '-';
                        }
                        break;
                    case 1:
                        {
                            viewEquipment.decoration += '①';
                        }
                        break;
                    case 2:
                        {
                            viewEquipment.decoration += '②';
                        }
                        break;
                    case 3:
                        {
                            viewEquipment.decoration += '③';
                        }
                        break;
                }
            }

            switch (equip.slot)
            {
                case logic.Equipment.Slot.Helms:
                    {
                        viewEquipment.slot = "头";
                    }
                    break;
                case logic.Equipment.Slot.Arms:
                    {
                        viewEquipment.slot = "手";
                    }
                    break;
                case logic.Equipment.Slot.Chests:
                    {
                        viewEquipment.slot = "身";
                    }
                    break;
                case logic.Equipment.Slot.Waist:
                    {
                        viewEquipment.slot = "腿";
                    }
                    break;
                case logic.Equipment.Slot.Legs:
                    {
                        viewEquipment.slot = "脚";
                    }
                    break;
            }
        }

        public void DumpSingle(ObservableCollection<ViewModelResultEquip> data, logic.SkillObject skillObject, ref int count)
        {
            ++count;
            ViewModelResultEquip viewEquipment = new ViewModelResultEquip();
            viewEquipment.souce = skillObject;
            viewEquipment.name = skillObject.DisplayName;
            viewEquipment.skills = "";
            foreach (var skill in skillObject.skills)
            {
                var skillName = skill.DisplayName;

                if (viewEquipment.skills.Length > 0)
                {
                    viewEquipment.skills = string.Format("{0};{1}", viewEquipment.skills, skillName);
                }
                else
                    viewEquipment.skills += skillName;
            }

            if (skillObject.slot == logic.SkillObject.Slot.Charm)
            {
                DumpCharm(viewEquipment, skillObject as logic.Charm);
            }
            else if (skillObject.slot == logic.SkillObject.Slot.Decoration)
            {
                DumpDecoration(viewEquipment, skillObject as logic.Decoration);
            }
            else
            {
                DumpEquipment(viewEquipment, skillObject as logic.Equipment);
            }

            data.Add(viewEquipment);

            if (skillObject.slot != logic.SkillObject.Slot.Decoration)
            {
                for (int i = 0; i < logic.Decoration.MaxSlot; ++i)
                {
                    if (equipmentSet.decortaions[(int)skillObject.slot, i] != null)
                    {
                        DumpSingle(data, equipmentSet.decortaions[(int)skillObject.slot, i], ref count);
                    }
                }
            }
        }

        public void Dump(ObservableCollection<ViewModelResultEquip> data)
        {
            all = new ViewModelResultEquip();
            int count = 0;
            foreach(var equip in equipmentSet.equipment)
            {
                if (equip == null)
                    continue;
                DumpSingle(data, equip, ref count);
            }
            all.name = string.Format("合计[{0}]", count);
            all.type = ViewModelResultEquip.Type.Background;
            data.Add(all);
            ViewModelResultEquip gray = new ViewModelResultEquip();
            gray.background = 1;
            data.Add(gray);
        }
    }


    public class ViewModelResultEquipList : ViewModelBase
    {
        public ObservableCollection<ViewModelResultEquip> Data { get; set; }
        public ObservableCollection<ViewModelSkill> refSkills { get; set; }

        public bool IsUseDecoration { get; set; }

        public ViewModelResultEquipList()
        {
            Data = new ObservableCollection<ViewModelResultEquip>();
            IsUseDecoration = true;
        }

        void RebuildResultEquipmentList()
        {
            if (refSkills.Count <= 0)
                return;

            List<logic.Skill> lsSkills = new List<logic.Skill>();
            foreach(var value in refSkills)
            {
                lsSkills.Add(value.skill);
            }

            logic.SourceData.SearchParam param = new logic.SourceData.SearchParam();
            param.useDecoration = IsUseDecoration;

            var equipSets = logic.SourceData.m_source.GetEquipmentSet(lsSkills, param);
            if (equipSets == null)
                return;

            ViewModelResultEquipSet viewEquipSet = new ViewModelResultEquipSet();
            viewEquipSet.AttachParent(this);
            
            //foreach (var value in equipSets)
            int count = equipSets.Count > 1000 ? 1000 : equipSets.Count;

            ViewModelResultEquip gray = new ViewModelResultEquip();
            gray.background = 1;
            gray.name = string.Format("合计方案:{0}", equipSets.Count);
            Data.Add(gray);

            for (int i = 0; i < count; ++i)
            {
                var value = equipSets[i];
                viewEquipSet.equipmentSet = value;
                viewEquipSet.Dump(Data);
            }
        }

        public void HandleChangeSkill()
        {
            Data.Clear();
            RebuildResultEquipmentList();
            NotifyPropertyChanged(() => Data);
        }

        public void Clear()
        {
            Data.Clear();
            NotifyPropertyChanged(() => Data);
        }
    }
}
