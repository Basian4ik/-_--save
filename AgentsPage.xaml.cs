using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
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

namespace Баязитов_глазки_save
{
    /// <summary>
    /// Логика взаимодействия для AgentsPage.xaml
    /// </summary>
    public partial class AgentsPage : Page
    {
        private IEnumerable<Agent> allAgents;

        int CountRecords=0;

        int CountPage=1;

        int CurrentPage = 0;

        int NumberAgents = 10;

        List<Agent> CurrentPageList = new List<Agent>();
        List<Agent> TableList=new List<Agent>();
        public AgentsPage()
        {
            InitializeComponent();

            var currentAgents = БаязитовГлазкиEntities.GetContext().Agent.ToList();

            AgentListView.ItemsSource = currentAgents;

            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        

        
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                // Загружаем всех агентов один раз
                allAgents = БаязитовГлазкиEntities.GetContext().Agent.ToList();

                // Загружаем типы агентов для фильтрации
                LoadFilterTypes();

                // Устанавливаем сортировку по умолчанию (по наименованию)
                SortComboBox.SelectedIndex = 0;

                // Обновляем список
                UpdateAgents();
            }
        }

        private void LoadFilterTypes()
        {
            var types = БаязитовГлазкиEntities.GetContext().AgentType.ToList();
            types.Insert(0, new AgentType { Title = "Все типы" });
            FilterComboBox.ItemsSource = types;
            FilterComboBox.SelectedIndex = 0;
        }

        private void UpdateAgents()
        {
            if (allAgents == null) return;

            // Начинаем со всех агентов
            var filteredAgents = allAgents;

            // 1. ФИЛЬТРАЦИЯ по типу агента
            if (FilterComboBox.SelectedItem != null && FilterComboBox.SelectedIndex > 0)
            {
                var selectedType = FilterComboBox.SelectedItem as AgentType;
                if (selectedType != null)
                {
                    filteredAgents = filteredAgents.Where(a =>
                        a.AgentType != null && a.AgentType.ID == selectedType.ID);
                }
            }

            // 2. ПОИСК по наименованию, email и телефону
            if (!string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                string searchText = SearchTextBox.Text.ToLower();
                string digitsOnlySearch = new string(searchText.Where(char.IsDigit).ToArray());
                filteredAgents = filteredAgents.Where(a =>
                    (a.Title != null && a.Title.ToLower().Contains(searchText)) ||
                    (a.Email != null && a.Email.ToLower().Contains(searchText)) ||
                    (a.Phone != null && IsPhoneMatch(a.Phone, searchText, digitsOnlySearch))
                );
            }

            // 3. СОРТИРОВКА
            if (SortComboBox.SelectedItem is ComboBoxItem selectedSort)
            {
                string sortTag = selectedSort.Tag as string;

                switch (sortTag)
                {
                    case "NameAsc":
                        filteredAgents = filteredAgents.OrderBy(a => a.Title);
                        break;
                    case "NameDesc":
                        filteredAgents = filteredAgents.OrderByDescending(a => a.Title);
                        break;
                    case "DiscountAsc":
                        filteredAgents = filteredAgents.OrderBy(a => a.Discount);
                        break;
                    case "DiscountDesc":
                        filteredAgents = filteredAgents.OrderByDescending(a => a.Discount);
                        break;
                    case "PriorityAsc":
                        filteredAgents = filteredAgents.OrderBy(a => a.Priority);
                        break;
                    case "PriorityDesc":
                        filteredAgents = filteredAgents.OrderByDescending(a => a.Priority);
                        break;
                }
            }

            // Сохраняем в TableList
            TableList = filteredAgents.ToList();

            // Вызываем ChangePage для начальной загрузки
            ChangePage(0, 0);
        }
        private bool IsPhoneMatch(string phone, string searchText, string digitsOnlySearch)
        {
            if (string.IsNullOrEmpty(phone))
                return false;

            // Поиск только по цифрам
            if (!string.IsNullOrEmpty(digitsOnlySearch))
            {
                // Извлекаем только цифры из телефона
                string digitsOnlyPhone = new string(phone.Where(char.IsDigit).ToArray());

                // Проверяем, содержатся ли цифры поиска в цифрах телефона
                if (digitsOnlyPhone.Contains(digitsOnlySearch))
                    return true;
            }

            //Поиск по тексту (если ввели что-то кроме цифр)
            if (!string.IsNullOrEmpty(searchText))
            {
                return phone.ToLower().Contains(searchText.ToLower());
            }

            // Если ничего не подошло
            return false;
        }

        // Обработчики событий
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAgents();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PageListBox.SelectedItem != null)
            {
                int selectedPage = Convert.ToInt32(PageListBox.SelectedItem) - 1;
                ChangePage(0, selectedPage); // 0 - переход на выбранную страницу
            }
        }

 
        private void PageListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
        private void UpdatePageControls()
        {
            PageListBox.Items.Clear();
            for (int i = 1; i <= CountPage; i++)
            {
                PageListBox.Items.Add(i);
            }
            PageListBox.SelectedIndex = CurrentPage;

           

            AgentListView.ItemsSource = CurrentPageList;
            AgentListView.Items.Refresh();
        }
        private void ChangePage(int direction, int? selectedPage)
        {
            CurrentPageList.Clear();
            CountRecords = TableList.Count;
            CountPage = (int)Math.Ceiling((double)CountRecords / NumberAgents); // Упрощённый расчёт количества страниц

            // Определяем новую страницу
            if (selectedPage.HasValue && selectedPage >= 0 && selectedPage < CountPage)
            {
                CurrentPage = selectedPage.Value;
            }
            else
            {
                switch (direction)
                {
                    case 1 when CurrentPage > 0: CurrentPage--; break;
                    case 2 when CurrentPage < CountPage - 1: CurrentPage++; break;
                }
            }

            // Заполняем текущую страницу
            int startIndex = CurrentPage * NumberAgents;
            int endIndex = Math.Min(startIndex + NumberAgents, CountRecords);
            CurrentPageList = TableList.Skip(startIndex).Take(NumberAgents).ToList();

            // Обновляем UI
            UpdatePageControls();
        }

        private void ChangePriorityBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddAgentBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
