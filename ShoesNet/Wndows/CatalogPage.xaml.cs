using ShoesNet.Classes;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShoesNet.Wndows
{
    public partial class CatalogPage : Page
    {
        ShoesEntities db = new ShoesEntities();

        public CatalogPage()
        {
            InitializeComponent();
            BtnAddProduct.Click += BtnAddProduct_Click;


            if (CurrentUser.RoleId == "Менеджер" || CurrentUser.RoleId == "Администратор") 
            {
                BtnOrders.Visibility = Visibility.Visible;
            }
            if (CurrentUser.RoleId == "Администратор") 
            {
                BtnAddProduct.Visibility = Visibility.Visible;
                
            }

            if (CurrentUser.RoleId == "Гость" || CurrentUser.RoleId == "Авторизированный клиент")
            {
                TxtSearch.IsEnabled = false;
                CmbSort.IsEnabled = false;
                CmbFilter.IsEnabled = false;
            }

            ReloadSuppliers();
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (AddEditProductPage.IsEditorOpen)
            {
                MessageBox.Show("Откройте только одно окно редактирования товара за раз.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            NavigationService.Navigate(new AddEditProductPage(null));
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.RoleId == "Гость") return;
            NavigationService.Navigate(new OrdersPage());
        }

        private void LViewProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            
            if (CurrentUser.RoleId != "Администратор") return; 

            if (AddEditProductPage.IsEditorOpen)
            {
                MessageBox.Show("Откройте только одно окно редактирования товара за раз.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (LViewProducts.SelectedItem is Товар selectedProduct)
            {
                NavigationService.Navigate(new AddEditProductPage(selectedProduct));
            }
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var productForDelete = button.DataContext as Товар;
            if (productForDelete == null) return;

            if (CurrentUser.RoleId != "Администратор") return;
    
            bool hasOrders = db.СоставЗаказ.Any(sz => sz.Артикул == productForDelete.Артикул);
            if (hasOrders)
            {
                MessageBox.Show("Невозможно удалить товар, так как он используется в заказах.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (MessageBox.Show($"Вы уверены, что хотите удалить {productForDelete.НаименованиеТовара}?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    db.Товар.Remove(productForDelete);
                    db.SaveChanges();
                    MessageBox.Show("Товар успешно удален!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    UpdateData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void BtnDelete_Loaded(object senser, RoutedEventArgs e)
        {
            var btn = senser as Button;
            if(btn != null)
            {
                if (CurrentUser.RoleId == "Администратор")
                {
                    btn.Visibility = Visibility.Visible;
                }
                else
                {
                    btn.Visibility = Visibility.Collapsed;
                }
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }


        public void Refresh()
        {
            ReloadSuppliers();
            UpdateData();
        }

        private void ReloadSuppliers()
        {
            if (CmbFilter == null) return;

            var currentSelected = CmbFilter.SelectedItem?.ToString();
            var normalizedSelected = NormalizeSupplier(currentSelected);

            var rawSuppliers = db.Товар
                .Select(p => p.Поставщик)
                .Where(s => s != null && s != "")
                .ToList(); 

   
            var suppliers = rawSuppliers
                .Where(s => !string.IsNullOrWhiteSpace(s)) 
                .Select(s => NormalizeSupplier(s))         
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            suppliers.Insert(0, "Все поставщики");
            CmbFilter.ItemsSource = suppliers;

            if (string.IsNullOrWhiteSpace(normalizedSelected) || normalizedSelected == NormalizeSupplier("Все поставщики"))
            {
                CmbFilter.SelectedIndex = 0;
                return;
            }

            int idx = suppliers.FindIndex(s => NormalizeSupplier(s) == normalizedSelected);
            CmbFilter.SelectedIndex = idx >= 0 ? idx : 0;
        }

        private string NormalizeSupplier(string value)
        {
            return (value ?? string.Empty).Replace('\u00A0', ' ').Trim();
        }

        private void FilterData(object sender, SelectionChangedEventArgs e) => UpdateData();
        private void FilterData(object sender, TextChangedEventArgs e) => UpdateData();

        private void UpdateData()
        {
            if (CmbFilter == null || CmbSort == null || TxtSearch == null || LViewProducts == null || TxtCount == null)
                return;

            var currentProducts = db.Товар.ToList();
            int totalCount = currentProducts.Count;

            if (!string.IsNullOrWhiteSpace(TxtSearch.Text))
            {
                string searchText = TxtSearch.Text.ToLower();
                currentProducts = currentProducts.Where(p =>
                    (p.Артикул != null && p.Артикул.ToLower().Contains(searchText)) ||
                    (p.НаименованиеТовара != null && p.НаименованиеТовара.ToLower().Contains(searchText)) ||
                    (p.КатегорияТовара != null && p.КатегорияТовара.ToLower().Contains(searchText)) ||
                    (p.Производитель != null && p.Производитель.ToLower().Contains(searchText)) ||
                    (p.Поставщик != null && p.Поставщик.ToLower().Contains(searchText)) ||
                    (p.ЕдиницаИзмерения != null && p.ЕдиницаИзмерения.ToLower().Contains(searchText)) ||
                    (p.ОписаниеТовара != null && p.ОписаниеТовара.ToLower().Contains(searchText))
                ).ToList();
            }


            if (CmbFilter.SelectedIndex > 0)
            {

                if (CmbFilter.SelectedItem != null)
                {
                    string selectedSupplier = CmbFilter.SelectedItem.ToString();
                    currentProducts = currentProducts.Where(p => (p.Поставщик ?? string.Empty).Replace('\u00A0', ' ').Trim() == selectedSupplier).ToList();
                }
            }

            if (CmbSort.SelectedIndex == 1) 
            {
                currentProducts = currentProducts.OrderBy(p => p.КолВоНаСкладе ?? 0).ToList();
            }
            else if (CmbSort.SelectedIndex == 2) 
            {
                currentProducts = currentProducts.OrderByDescending(p => p.КолВоНаСкладе ?? 0).ToList();
            }

            LViewProducts.ItemsSource = currentProducts;
            TxtCount.Text = $"{currentProducts.Count} из {totalCount}";
        }
    }
}
