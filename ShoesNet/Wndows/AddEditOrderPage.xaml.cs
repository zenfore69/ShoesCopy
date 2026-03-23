using ShoesNet.Classes;
using ShoesNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ShoesNet.Wndows
{
    public partial class AddEditOrderPage : Page
    {
        private Заказ currentOrder = new Заказ();
        private ShoesEntities db = new ShoesEntities();
        private bool isEditMode = false;

        public AddEditOrderPage(Заказ selectedOrder)
        {
            InitializeComponent();

            // Dropdown статусов и пунктов выдачи.
            var statuses = db.Заказ
                .Select(o => o.СтатусЗаказа)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            if (statuses.Count == 0)
            {
                // Fallback на случай пустых данных в БД.
                statuses = new List<string> { "Новый", "В обработке", "Выдан" };
            }
            CmbStatus.ItemsSource = statuses;

            CmbPickupPoint.ItemsSource = db.ПунктВыдачи.ToList();

            DpOrderDate.SelectedDate = DateTime.Today;
            DpPickupDate.SelectedDate = DateTime.Today;

            if (selectedOrder != null)
            {
                var loaded = db.Заказ.FirstOrDefault(o => o.НомерЗаказа == selectedOrder.НомерЗаказа);
                currentOrder = loaded ?? selectedOrder;
                isEditMode = true;
                this.Title = "Редактирование заказа";

                CmbStatus.SelectedItem = currentOrder.СтатусЗаказа;
                CmbPickupPoint.SelectedValue = currentOrder.КодАдресПунктВыдачи;

                TxtArticle.Text = db.СоставЗаказ
                    .Where(s => s.КодЗаказа == currentOrder.НомерЗаказа)
                    .Select(s => s.Артикул)
                    .FirstOrDefault();

                if (DateTime.TryParse(currentOrder.ДатаЗаказа, out var orderDate))
                    DpOrderDate.SelectedDate = orderDate;
                if (DateTime.TryParse(currentOrder.ДатаДоставки, out var pickupDate))
                    DpPickupDate.SelectedDate = pickupDate;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string article = TxtArticle.Text?.Trim();
                if (string.IsNullOrWhiteSpace(article))
                {
                    MessageBox.Show("Введите артикул товара.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!db.Товар.Any(t => t.Артикул == article))
                {
                    MessageBox.Show("Такого товара (артикула) не найдено в базе данных.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string status = CmbStatus.SelectedItem?.ToString();
                if (string.IsNullOrWhiteSpace(status))
                {
                    MessageBox.Show("Выберите статус заказа.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (CmbPickupPoint.SelectedValue == null)
                {
                    MessageBox.Show("Выберите пункт выдачи.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!DpOrderDate.SelectedDate.HasValue || !DpPickupDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Укажите даты заказа и выдачи.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Номер заказа: автоматически вычисляем (max + 1).
                if (!isEditMode)
                {
                    int maxOrderNumber = db.Заказ.Select(o => o.НомерЗаказа).DefaultIfEmpty(0).Max();
                    currentOrder.НомерЗаказа = maxOrderNumber + 1;
                    db.Заказ.Add(currentOrder);
                }

                currentOrder.СтатусЗаказа = status;
                currentOrder.ДатаЗаказа = DpOrderDate.SelectedDate.Value.ToString("yyyy-MM-dd");
                currentOrder.ДатаДоставки = DpPickupDate.SelectedDate.Value.ToString("yyyy-MM-dd");
                currentOrder.КодАдресПунктВыдачи = Convert.ToInt32(CmbPickupPoint.SelectedValue);
                currentOrder.КодПользователя = CurrentUser.UserId;

                if (string.IsNullOrWhiteSpace(currentOrder.КодДляПолучения))
                    currentOrder.КодДляПолучения = Guid.NewGuid().ToString("N").Substring(0, 10);

                // Для простоты задания: заменяем состав заказа на один элемент с указанным артикулом.
                var existingCompositions = db.СоставЗаказ.Where(s => s.КодЗаказа == currentOrder.НомерЗаказа).ToList();
                foreach (var c in existingCompositions)
                    db.СоставЗаказ.Remove(c);

                int maxCompCode = db.СоставЗаказ.Select(s => s.Код).DefaultIfEmpty(0).Max();
                var newComp = new СоставЗаказ
                {
                    Код = maxCompCode + 1,
                    КодЗаказа = currentOrder.НомерЗаказа,
                    Артикул = article,
                    Колчество = 1
                };
                db.СоставЗаказ.Add(newComp);

                db.SaveChanges();

                MessageBox.Show("Данные заказа успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

