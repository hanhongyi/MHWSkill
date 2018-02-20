using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MHWSkill.ui
{
    public class ViewModelSkill : ViewModelBase
    {
        public string skillName { get; set; }
        public int category { get; set; }
        public MHWSkill.logic.Skill skill;
    }

    public class ViewModelFilterItem : ViewModelBase
    {

    }

    public class ViewModelFilter : ViewModelBase
    {
        public ObservableCollection<ViewModelFilterItem> History { get; set; }

        public bool IsFiltered { get; set; }

        public void ModifyCurrentText(string value, bool notify)
        {
            currentText = value;
            ViewModelSkillList skillList = Parent as ViewModelSkillList;
            if (skillList != null)
            {
                skillList.DoFilter(true);
            }
        }

        public string currentText = "";
        public string Current 
        { 
            get
            {
                return currentText;
            }
            set
            {
                bool same = currentText == value;
                if(!same)
                {
                    ModifyCurrentText(value, false);
                }
            }
        }
        public ViewModelFilterItem SelectedItem { get; set; }
    }

    public class ViewModelSkillList : ViewModelBase
    {
        public ViewModelSkillList()
        {
            IsShowCategory = new bool[maxShowCategory];
            for (int i = 0; i < maxShowCategory; ++i)
            {
                IsShowCategory[i] = true;
            }

            Skills = new ObservableCollection<ViewModelSkill>();
            Filter = new ViewModelFilter();
            Filter.AttachParent(this);
            Filter.IsFiltered = true;
        }

        #region Toggles
        private bool isShowAllCategory = true;
        public bool IsShowAllCategory
        {
            get
            {
                return this.isShowAllCategory;
            }
            set
            {
                this.isShowAllCategory = !this.isShowAllCategory;
                this.NotifyPropertyChanged(() => this.IsShowAllCategory);
            }
        }
        private const int maxShowCategory = 7;
        public bool[] IsShowCategory { get; set; }
       
        private void ToggleIsShowAllCategory_Executed()
        {
            //if (this.IsShowAllCategory)
            {
                bool notify = false;
                for (int i = 0; i < maxShowCategory; ++i)
                {
                    if (IsShowCategory[i] != this.IsShowAllCategory)
                    {
                        IsShowCategory[i] = this.IsShowAllCategory;
                        notify = true;
                    }
                }

                if (notify)
                    this.NotifyPropertyChanged(() => this.IsShowCategory);
            }
            //this.PopulateView();

            DoFilter(true);
        }

        private void ToggleIsShowCategory_Executed(int idx)
        {
            if (!IsShowCategory[idx])
            {
                if (this.IsShowAllCategory)
                {
                    this.IsShowAllCategory = false;
                }
            }
            else
            {
                bool showAll = true;
                for (int i = 0; i < maxShowCategory; ++i)
                {
                    if (!IsShowCategory[i])
                    {
                        showAll = false;
                        break;
                    }
                }
                if (showAll)
                    this.IsShowAllCategory = showAll;
            }

            DoFilter(true);
        }

        private ICommand mToggleIsShowAllCategory;
        public ICommand ToggleIsShowAllCategory
        {
            get
            {
                if (this.mToggleIsShowAllCategory == null)
                    this.mToggleIsShowAllCategory = new RelayCommand<object>(
                        (p) => this.ToggleIsShowAllCategory_Executed()
                    );

                return this.mToggleIsShowAllCategory;
            }
        }

        private ICommand[] mToggleIsShowCategory;
        public ICommand[] ToggleIsShowCategory
        {
            get
            {
                if (mToggleIsShowCategory == null)
                {
                    mToggleIsShowCategory = new ICommand[maxShowCategory];
                    for(int i = 0;i<maxShowCategory;++i)
                    {
                        int value = i;
                        mToggleIsShowCategory[i] = new RelayCommand<object>((p) =>
                        {
                            this.ToggleIsShowCategory_Executed(value);
                        });
                    }
                }
                return this.mToggleIsShowCategory;
            }
        }
        #endregion

        public void DoFilter(bool raise)
        {
            Skills.Clear();

            foreach(var skill in SourceSkills)
            {
                bool isNotFilterName = string.IsNullOrEmpty(Filter.Current);
                if (isNotFilterName &&
                    IsShowAllCategory)
                {
                    Skills.Add(skill);
                    continue;
                }

                // filter name
                if (!isNotFilterName &&
                    !skill.skillName.Contains(Filter.Current))
                {
                    if(string.IsNullOrEmpty(skill.skill.name) ||
                        !skill.skill.name.Contains(Filter.Current))
                    {
                        continue;
                    }
                }

                // category
                if(!IsShowAllCategory)
                {
                    int category = skill.category - 1;
                    if (category < 0 ||
                        category >= maxShowCategory ||
                        !IsShowCategory[category])
                    {
                        continue;
                    }
                }

                Skills.Add(skill);
            }

            if(raise)
                this.NotifyPropertyChanged(() => this.Skills);
        }

        public void Mapping(bool raise)
        {
            Skills.Clear();
            SourceSkills.Clear();

            // all skills
            Action<Dictionary<int, logic.Skill>> mapAction = (logicSkills) =>
            {
                foreach(var kv in logicSkills)
                {
                    var skill = kv.Value;
                    if (skill == null)
                        continue;

                    ViewModelSkill viewModelSkill = new ViewModelSkill();
                    viewModelSkill.skillName = skill.DisplayName;
                    viewModelSkill.category = (int)skill.category;
                    viewModelSkill.skill = skill;
                    viewModelSkill.AttachParent(this);

                    SourceSkills.Add(viewModelSkill);
                }
            };

            mapAction(logic.SourceData.m_source.skills);
            mapAction(logic.SourceData.m_source.setSkills);

            DoFilter(raise);
        }

        protected ObservableCollection<ViewModelSkill> SourceSkills = new ObservableCollection<ViewModelSkill>();
        public ObservableCollection<ViewModelSkill> Skills { get; set; }
        public ViewModelFilter Filter { get; set; }

        public static void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ListView listView = sender as System.Windows.Controls.ListView;
            if (listView == null)
                return;

            var obj = listView.SelectedItem as ViewModelSkill;
            if (obj == null)
                return;

            ViewModelSkillList skillList = obj.Parent as ViewModelSkillList;
            if (skillList == null)
                return;

            ViewModelMgr mgr = skillList.Parent as ViewModelMgr;
            if (mgr == null)
                return;

            ViewModelSelectedSkillList selectedSkillList = mgr.SelectedSkillList;
            if (selectedSkillList!=null)
                selectedSkillList.AddSkill(obj);
        }
    }
}
