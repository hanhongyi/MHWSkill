using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MHWSkill.ui
{
    public class ViewModelEquips : ViewModelBase
    {
        public void SelectEquip_Execute(int idx, object p)
        {

        }

        public const int maxEquipCount = (int)logic.SkillObject.Slot.MAX;
        public ICommand[] mDoClickSlots;
        public ICommand[] DoClickSlots
        {
            get
            {
                if(mDoClickSlots == null)
                {
                    mDoClickSlots = new ICommand[maxEquipCount];
                    for (int i = 0; i < maxEquipCount; ++i)
                    {
                        int value = i;
                        mDoClickSlots[i] = new RelayCommand<object>((p) =>
                        {
                            SelectEquip_Execute(value, p);
                        });
                    }
                }

                return mDoClickSlots;
            }
        }

        public void ToggleUseDecoration_Executed()
        {
            ViewModelMgr mgr = m_Parent as ViewModelMgr;
            if(mgr!=null)
            {
                mgr.ResultEquipList.IsUseDecoration = IsUseDecoration;
                mgr.ResultEquipList.HandleChangeSkill();
            }
        }

        public ICommand mToggleUseDecoration;
        public ICommand ToggleUseDecoration
        {
            get
            {
                if(mToggleUseDecoration == null)
                {
                    mToggleUseDecoration = new RelayCommand<object>((p) =>
                    {
                        this.ToggleUseDecoration_Executed();
                    });
                }
                return mToggleUseDecoration;
            }
        }

        public ObservableCollection<string>[] Slots { get; set; }
        public ObservableCollection<string>[] SelectedItem { get; set; }
        public string[] SlotText { get; set; }
        public bool[] IsEnable { get; set; }

        public bool IsUseDecoration { get; set; }
        
        public ViewModelEquips()
        {
            IsEnable = new bool[maxEquipCount];
            SlotText = new string[maxEquipCount];
            SelectedItem = new ObservableCollection<string>[maxEquipCount];
            Slots = new ObservableCollection<string>[maxEquipCount];
            for(int i = 0;i<maxEquipCount;++i)
            {
                IsEnable[i] = true;
                SelectedItem[i] = new ObservableCollection<string>();
                Slots[i] = new ObservableCollection<string>();
            }
            IsUseDecoration = true;
        }

        public void Mapping(bool raise)
        {

        }
    }
}
