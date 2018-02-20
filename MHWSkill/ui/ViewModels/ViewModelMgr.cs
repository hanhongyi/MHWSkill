using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWSkill.ui
{
    public class ViewModelMgr : ViewModelBase
    {
        public ViewModelSkillList SkillList { get;set; }
        public ViewModelEquips EquipList { get; set; }
        public ViewModelSelectedSkillList SelectedSkillList { get; set; }
        public ViewModelResultEquipList ResultEquipList { get; set; }

        public void Init()
        {
            SkillList = new ViewModelSkillList();
            SkillList.Mapping(false);
            SkillList.AttachParent(this);

            EquipList = new ViewModelEquips();
            EquipList.Mapping(false);
            EquipList.AttachParent(this);

            SelectedSkillList = new ViewModelSelectedSkillList();
            SelectedSkillList.AttachParent(this);

            ResultEquipList = new ViewModelResultEquipList();
            ResultEquipList.refSkills = SelectedSkillList.Skills;
            ResultEquipList.AttachParent(this);
        }

        public void DoMapping()
        {
            SkillList.Mapping(true);
            EquipList.Mapping(true);
            SelectedSkillList.Clear();
            ResultEquipList.Clear();
        }
    }
}
