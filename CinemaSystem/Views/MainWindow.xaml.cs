using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CinemaSystem.Views.Pages;


namespace CinemaSystem.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();   

            // Устанавливаем начальное состояние после загрузки XAML
            if (MenuListBox != null)
                MenuListBox.SelectedIndex = 0;

            // Загружаем первую страницу
            if (MainFrame != null)
                MainFrame.Navigate(new FilmsPage());

            // Устанавливаем breadcrumbs безопасно
            UpdateBreadcrumb("Фильмы");
        }

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuListBox.SelectedIndex < 0 || BreadcrumbText == null)
                return;

            // Получаем название раздела из пункта меню
            if (MenuListBox.SelectedItem is ListBoxItem selectedItem &&
                selectedItem.Content is StackPanel panel &&
                panel.Children.Count > 1 &&
                panel.Children[1] is TextBlock textBlock)
            {
                string sectionName = textBlock.Text;
                UpdateBreadcrumb(sectionName);

                // Навигация по страницам
                switch (MenuListBox.SelectedIndex)
                {
                    case 0: MainFrame.Navigate(new FilmsPage()); break;
                    case 1: MainFrame.Navigate(new SessionsPage()); break;
                    case 2: MainFrame.Navigate(new HallsPage()); break;
                    case 3: MainFrame.Navigate(new VisitorsPage()); break;
                    case 4: MainFrame.Navigate(new TicketSalesPage()); break;
                    case 5: MainFrame.Navigate(new ReportsPage()); break;
                }
            }
        }

        // Безопасный метод обновления breadcrumbs
        private void UpdateBreadcrumb(string sectionName)
        {
            if (BreadcrumbText != null)
                BreadcrumbText.Text = $"Главная → {sectionName}";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                MessageBox.Show("Контекстная помощь по текущему разделу.\n\n" +
                                "F1 — помощь\n" +
                                "Esc — возврат назад (в будущем)\n" +
                                "Enter — подтверждение\n" +
                                "Tab / Shift+Tab — переход между полями",
                                "Помощь", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Автоматизированная система кинотеатра «StarLight»\n\n" +
                            "Тема курсовой: Разработка автоматизированной системы кинотеатра\n" +
                            "Выполнил: Барышев Эмиль Евгеньевич\n" +
                            "Группа: 831-21\n" +
                            "2026 год\n\n" +
                            "Система включает базу фильмов, сеансов, кинозалов, посетителей с бонусной картой и скидками.",
                            "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти из программы?\nВсе несохранённые данные будут потеряны.",
                                "Подтверждение выхода",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}