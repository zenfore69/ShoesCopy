using ShoesNet.Classes;
using ShoesNet.Model;
using ShoesNet.Wndows;
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

namespace ShoesNet
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new Wndows.AuthPage());


        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack) MainFrame.GoBack();
        }
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            BtnBack.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Hidden;

            // Обновляем данные на страницах после навигации (в т.ч. при возврате назад).
            if (e.Content is CatalogPage catalog)
                catalog.Refresh();
            else if (e.Content is OrdersPage orders)
                orders.Refresh();

            // Сообщение о текущем пользователе наверху справа (рядом с кнопкой "Назад").
            if (e.Content is AuthPage)
            {
                TxtUserTop.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtUserTop.Visibility = Visibility.Visible;
                TxtUserTop.Text = $"Вы вошли как: {CurrentUser.FullName}";
            }
        }

    }
}
