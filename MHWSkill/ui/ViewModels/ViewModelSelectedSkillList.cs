using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MHWSkill.ui
{
    public class ViewModelSelectedSkillList : ViewModelBase
    {
        public ObservableCollection<ViewModelSkill> Skills { get; set; }

        public ViewModelSelectedSkillList()
        {
            Skills = new ObservableCollection<ViewModelSkill>();
        }

        public void Clear()
        {
            Skills.Clear();
            NotifyPropertyChanged(() => Skills);
        }

        public void OnChangeSkill()
        {
            NotifyPropertyChanged(() => Skills);

            ViewModelMgr parent = m_Parent as ViewModelMgr;
            if(parent!=null)
                parent.ResultEquipList.HandleChangeSkill();
        }

        public void AddSkill(ViewModelSkill rightSkill)
        {
            foreach(var view in Skills)
            {
                if (view.skill == rightSkill.skill)
                    return;
            }

            ViewModelSkill skill = new ViewModelSkill();
            skill.skillName = rightSkill.skillName;
            skill.skill = rightSkill.skill;
            skill.category = rightSkill.category;
            skill.AttachParent(this);

            Skills.Add(skill);
            OnChangeSkill();
        }

        public void DeleteSkill(ViewModelSkill rightSkill)
        {
            for (int i = 0;i<Skills.Count;++i)
            {
                var view = Skills[i];
                if (view.skill == rightSkill.skill)
                {
                    Skills.RemoveAt(i);
                    OnChangeSkill();
                    return;
                }
            }
        }

        public ICommand mDeleteCommand;
        public ICommand DeleteCommand
        {
            get
            {
                if(mDeleteCommand == null)
                {
                    mDeleteCommand = new RelayCommand<object>((p) =>
                    {
                        ViewModelSkill viewModel = p as ViewModelSkill;
                        if(viewModel!=null)
                        {
                            this.DeleteSkill(viewModel);
                        }
                    });
                }
                return mDeleteCommand;
            }
        }

        public static void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ListView listView = sender as System.Windows.Controls.ListView;
            if (listView == null)
                return;

            var obj = listView.SelectedItem as ViewModelSkill;
            if (obj == null)
                return;

            ViewModelSelectedSkillList skillList = obj.Parent as ViewModelSelectedSkillList;
            if (skillList == null)
                return;

            skillList.DeleteSkill(obj);
        }
    }
}
