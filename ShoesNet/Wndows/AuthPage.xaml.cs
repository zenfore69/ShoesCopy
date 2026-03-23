using ShoesNet.Model;
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
using System.Windows.Shapes;
using ShoesNet.Classes;

namespace ShoesNet.Wndows
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        ShoesEntities db = new ShoesEntities();
        public AuthPage()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var user = db.Пользователи.FirstOrDefault(u => u.Логин == TxtBoxLogin.Text && u.Пароль == PassBox.Password);
            if (user != null)
            {
                CurrentUser.RoleId = user.РольСотрудника;
                CurrentUser.FullName = user.ФИО;
                CurrentUser.UserId = user.Код;
                NavigationService.Navigate(new CatalogPage());
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.RoleId = "Гость";
            CurrentUser.FullName = "Гость";
            CurrentUser.UserId = 0;
            NavigationService.Navigate(new CatalogPage());
        }
    }
}