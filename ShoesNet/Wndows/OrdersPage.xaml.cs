using ShoesNet.Classes;
using ShoesNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShoesNet.Wndows
{
    public partial class OrdersPage : Page
    {
        private ShoesEntities db = new ShoesEntities();

        public class OrderRowViewModel
        {
            public Заказ Заказ { get; set; }
            public string Артикул { get; set; }
            public string СтатусЗаказа { get; set; }
            public string АдресПунктаВыдачи { get; set; }
            public string ДатаЗаказа { get; set; }
            public string ДатаДоставки { get; set; }
        }

        public OrdersPage()
        {
            InitializeComponent();
            if (CurrentUser.RoleId == "Администратор")
            {
                BtnAddOrder.Visibility = Visibility.Visible;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }

        // Вызывается из MainWindow при навигации.
        public void Refresh() => UpdateData();

        private void UpdateData()
        {
            var rows = new List<OrderRowViewModel>();

            var orders = db.Заказ.ToList();
            foreach (var order in orders)
            {
                try
                {
                    var pickup = db.ПунктВыдачи.FirstOrDefault(p => p.Код == order.КодАдресПунктВыдачи);
                    var articles = db.СоставЗаказ
                        .Where(s => s.КодЗаказа == order.НомерЗаказа)
                        .Select(s => s.Артикул)
                        .Distinct()
                        .ToList();

                    rows.Add(new OrderRowViewModel
                    {
                        Заказ = order,
                        Артикул = string.Join(", ", articles),
                        СтатусЗаказа = order.СтатусЗаказа,
                        АдресПунктаВыдачи = pickup?.Адрес,
                        ДатаЗаказа = order.ДатаЗаказа,
                        ДатаДоставки = order.ДатаДоставки
                    });
                }
                catch
                {
                    // Если конкретная строка не смогла загрузиться (например, битые ссылки в данных),
                    // всё равно показываем остальные заказы.
                }
            }

            LViewOrders.ItemsSource = rows;
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.RoleId != "Администратор") return;
            NavigationService.Navigate(new AddEditOrderPage(null));
        }

        private void LViewOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CurrentUser.RoleId != "Администратор") return;
            if (LViewOrders.SelectedItem is OrderRowViewModel selectedRow)
            {
                NavigationService.Navigate(new AddEditOrderPage(selectedRow.Заказ));
            }
        }

        private void BtnDelete_Loaded(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            btn.Visibility = CurrentUser.RoleId == "Администратор" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.RoleId != "Администратор") return;

            var button = sender as Button;
            var selectedRow = button?.DataContext as OrderRowViewModel;
            if (selectedRow == null || selectedRow.Заказ == null) return;

            try
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить заказ?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;

                // Сначала удаляем состав заказа, затем сам заказ (чтобы не упереться в FK).
                var compositions = db.СоставЗаказ.Where(s => s.КодЗаказа == selectedRow.Заказ.НомерЗаказа).ToList();
                foreach (var c in compositions)
                    db.СоставЗаказ.Remove(c);

                db.Заказ.Remove(selectedRow.Заказ);
                db.SaveChanges();

                UpdateData();
                MessageBox.Show("Заказ успешно удален!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

