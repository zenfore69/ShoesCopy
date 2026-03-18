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
            TxtUserName.Text = $"Вы вошли как: {CurrentUser.FullName}";

            if (CurrentUser.RoleId == "Менеджер" || CurrentUser.RoleId == "Администратор") 
            {
                BtnOrders.Visibility = Visibility.Visible;
            }
            if (CurrentUser.RoleId == "Администратор") 
            {
                BtnAddProduct.Visibility = Visibility.Visible;
                
            }

            if (CurrentUser.RoleId == "Гость")
            {
                TxtSearch.IsEnabled = false;
                CmbSort.IsEnabled = false;
                CmbFilter.IsEnabled = false;
            }

            var manufacturers = db.Товар.Select(p => p.Производитель).Distinct().ToList();
            manufacturers.Insert(0, "Все производители");
            CmbFilter.ItemsSource = manufacturers;
            CmbFilter.SelectedIndex = 0;
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            NavigationService.Navigate(new AddEditProductPage(null));
        }

        private void LViewProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            
            if (CurrentUser.RoleId != "Администратор") return; 

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
                    (p.НаименованиеТовара != null && p.НаименованиеТовара.ToLower().Contains(searchText)) ||
                    (p.ОписаниеТовара != null && p.ОписаниеТовара.ToLower().Contains(searchText))
                ).ToList();
            }


            if (CmbFilter.SelectedIndex > 0)
            {

                if (CmbFilter.SelectedItem != null)
                {
                    string selectedMaker = CmbFilter.SelectedItem.ToString();
                    currentProducts = currentProducts.Where(p => p.Производитель == selectedMaker).ToList();
                }
            }

            if (CmbSort.SelectedIndex == 1) 
            {
                currentProducts = currentProducts.OrderBy(p => p.Цена).ToList();
            }
            else if (CmbSort.SelectedIndex == 2) 
            {
                currentProducts = currentProducts.OrderByDescending(p => p.Цена).ToList();
            }

            LViewProducts.ItemsSource = currentProducts;
            TxtCount.Text = $"{currentProducts.Count} из {totalCount}";
        }
    }
}
