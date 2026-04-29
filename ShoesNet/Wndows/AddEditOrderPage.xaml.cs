using ShoesNet.Classes;
using ShoesNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
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
        private static bool _isEditorOpen = false;
        public static bool IsEditorOpen => _isEditorOpen;

        public AddEditOrderPage(Заказ selectedOrder)
        {
            InitializeComponent();
            if (CurrentUser.RoleId != "Администратор")
            {
                MessageBox.Show("Редактировать заказы может только администратор.", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService?.GoBack();
                return;
            }
            _isEditorOpen = true;
            Unloaded += (_, __) => { _isEditorOpen = false; };


            var statuses = db.Заказ
                .Select(o => o.СтатусЗаказа)
                .Distinct()
                .ToList()
                .Where(s => s != null && s.Trim() != string.Empty)
                .Select(s => s.Trim())
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            if (statuses.Count == 0)
            {

                statuses = new List<string> { "Новый", "В обработке", "Выдан" };
            }
            CmbStatus.ItemsSource = statuses;

            DpOrderDate.SelectedDate = DateTime.Today;
            DpPickupDate.SelectedDate = DateTime.Today;

            if (selectedOrder != null)
            {
                var loaded = db.Заказ.FirstOrDefault(o => o.НомерЗаказа == selectedOrder.НомерЗаказа);
                if (loaded == null)
                {
                    MessageBox.Show("Заказ не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    _isEditorOpen = false;
                    return;
                }

                currentOrder = loaded;
                isEditMode = true;
                this.Title = "Редактирование заказа";

                CmbStatus.SelectedItem = currentOrder.СтатусЗаказа?.Trim();
                TxtPickupAddress.Text = db.ПунктВыдачи.FirstOrDefault(p => p.Код == currentOrder.КодАдресПунктВыдачи)?.Адрес;

                TxtArticle.Text = db.СоставЗаказ
                    .Where(s => s.КодЗаказа == currentOrder.НомерЗаказа)
                    .Select(s => s.Артикул)
                    .FirstOrDefault();

                if (DateTime.TryParse(currentOrder.ДатаЗаказа, new CultureInfo("ru-RU"), DateTimeStyles.None, out var orderDate))
                    DpOrderDate.SelectedDate = orderDate;
                if (DateTime.TryParse(currentOrder.ДатаДоставки, new CultureInfo("ru-RU"), DateTimeStyles.None, out var pickupDate))
                    DpPickupDate.SelectedDate = pickupDate;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentUser.RoleId != "Администратор") return;
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

                string pickupAddress = TxtPickupAddress.Text?.Trim();
                pickupAddress = NormalizeText(pickupAddress);
                if (string.IsNullOrWhiteSpace(pickupAddress))
                {
                    MessageBox.Show("Введите адрес пункта выдачи.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!DpOrderDate.SelectedDate.HasValue || !DpPickupDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Укажите даты заказа и выдачи.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                if (!isEditMode)
                {
                    int maxOrderNumber = db.Заказ.Select(o => o.НомерЗаказа).DefaultIfEmpty(0).Max();
                    currentOrder.НомерЗаказа = maxOrderNumber + 1;
                    db.Заказ.Add(currentOrder);
                }

                currentOrder.СтатусЗаказа = status.Trim();

                currentOrder.ДатаЗаказа = DpOrderDate.SelectedDate.Value.ToString("dd.MM.yyyy", new CultureInfo("ru-RU"));
                currentOrder.ДатаДоставки = DpPickupDate.SelectedDate.Value.ToString("dd.MM.yyyy", new CultureInfo("ru-RU"));

                var pickups = db.ПунктВыдачи.ToList();
                var pickup = pickups.FirstOrDefault(p => NormalizeText(p.Адрес) == pickupAddress);
                if (pickup == null)
                {
                    MessageBox.Show("По введенному адресу пункт выдачи не найден в базе данных.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                currentOrder.КодАдресПунктВыдачи = pickup.Код;
                currentOrder.КодПользователя = CurrentUser.UserId;

                if (string.IsNullOrWhiteSpace(currentOrder.КодДляПолучения))
                    currentOrder.КодДляПолучения = Guid.NewGuid().ToString("N").Substring(0, 10);

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

        private string NormalizeText(string value)
        {
            return (value ?? string.Empty).Replace('\u00A0', ' ').Trim();
        }
    }
}

