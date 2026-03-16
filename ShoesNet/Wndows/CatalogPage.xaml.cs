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
            TxtUserName.Text = $"Вы вошли как: {CurrentUser.FullName}";

            if (CurrentUser.RoleId == "Менеджер" || CurrentUser.RoleId == "Админ") 
            {
                BtnOrders.Visibility = Visibility.Visible;
            }
            if (CurrentUser.RoleId == "Админ") 
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
