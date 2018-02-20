using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MHWSkill
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected ui.ViewModelMgr ModelView;
        public MainWindow()
        {
            InitializeComponent();

            // reload all skills
            logic.HtmlDumper.Do();

            ModelView = new ui.ViewModelMgr();
            ModelView.Init();
            this.DataContext = ModelView;
        }

        private void MenuItemDump_Click(object sender, RoutedEventArgs e)
        {
            logic.HtmlDumper.Do();
            ModelView.DoMapping();
        }

        private void MenuItemSaveExcel_Click(object sender, RoutedEventArgs e)
        {
            logic.SourceData.SaveExcel();
        }

        private void MenuItemReload_Click(object sender, RoutedEventArgs e)
        {
        }

        private void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MHWSkill.ui.ViewModelSkillList.HandleDoubleClick(sender, e);
        }

        private void HandleSelectedSkillListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MHWSkill.ui.ViewModelSelectedSkillList.HandleDoubleClick(sender, e);
        }
    }
}
