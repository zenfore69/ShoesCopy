using ShoesNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;


namespace ShoesNet.Wndows
{
    /// <summary>
    /// Логика взаимодействия для AddEditProductPage.xaml
    /// </summary>
    public partial class AddEditProductPage : Page
    {
        private Товар currentProduct = new Товар();
        private ShoesEntities db = new ShoesEntities();
        private string newPhotoPath = null;
        private bool isEditMode = false;
        private static bool _isEditorOpen = false;
        public static bool IsEditorOpen => _isEditorOpen;

        public AddEditProductPage(Товар selectedProduct)
        {
            InitializeComponent();
            _isEditorOpen = true;
            Unloaded += (_, __) => { _isEditorOpen = false; };
            if (selectedProduct != null)
            {
                var loaded = db.Товар.FirstOrDefault(t => t.Артикул == selectedProduct.Артикул);
                currentProduct = loaded ?? selectedProduct;
                isEditMode = true;
                TxtArticle.IsReadOnly = true;
                TxtArticle.Visibility = Visibility.Visible;
                TxtArticle.Text = currentProduct.Артикул;
                this.Title = "Редактирование товара";
            }
            else
            {
                isEditMode = false;
                TxtArticle.IsReadOnly = false;
                TxtArticle.Visibility = Visibility.Visible;
                TxtArticle.Text = string.Empty;
                this.Title = "Добавление нового товара";
            }
            DataContext = currentProduct;
            CmbCategory.ItemsSource = db.Товар.Select(t => t.КатегорияТовара).Distinct().ToList();
            CmbManufacturer.ItemsSource = db.Товар.Select(t => t.Производитель).Distinct().ToList();
            UpdateImageDisplay();
        }
        private void UpdateImageDisplay()
        {
            try
            {
                if (!string.IsNullOrEmpty(currentProduct.Фото))
                {

                    string path = System.IO.Path.Combine(Environment.CurrentDirectory, "Assets", currentProduct.Фото);
                    if (File.Exists(path))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.UriSource = new Uri(path);
                        bitmap.EndInit();

                        ProductImage.Source = bitmap;
                    }
                }
            }
            catch { }
        }

        private void BtnSelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
            if (openDialog.ShowDialog() == true)
            {
                try
                {

                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.UriSource = new Uri(openDialog.FileName);
                    img.EndInit();
                    if (img.PixelWidth > 300 || img.PixelHeight > 200)
                    {
                        MessageBox.Show("Размер фото не должен превышать 300x200 пикселей!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);

                        return;
                    }
                    newPhotoPath = openDialog.FileName;
                    ProductImage.Source = img;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка чтения изображения: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {

            currentProduct.Артикул = TxtArticle.Text?.Trim();
            currentProduct.НаименованиеТовара = TxtName.Text?.Trim();
            currentProduct.ОписаниеТовара = TxtDescription.Text;
            currentProduct.КатегорияТовара = CmbCategory.Text?.Trim();
            currentProduct.Производитель = CmbManufacturer.Text?.Trim();
            currentProduct.Поставщик = TxtSupplier.Text?.Trim();
            currentProduct.ЕдиницаИзмерения = TxtUnit.Text?.Trim();

            if (string.IsNullOrWhiteSpace(currentProduct.Артикул))
            {
                MessageBox.Show("Введите артикул товара!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(currentProduct.НаименованиеТовара))
            {
                MessageBox.Show("Укажите наименование товара!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(currentProduct.КатегорияТовара))
            {
                MessageBox.Show("Выберите категорию товара!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(currentProduct.Производитель))
            {
                MessageBox.Show("Выберите производителя!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            decimal price;
            if (!decimal.TryParse(TxtPrice.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out price) &&
                !decimal.TryParse(TxtPrice.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out price))
            {
                MessageBox.Show("Введите корректную цену товара (число).", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (price < 0)
            {
                MessageBox.Show("Стоимость товара не может быть отрицательной!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            currentProduct.Цена = price;

            int discount;
            if (!int.TryParse(TxtDiscount.Text, out discount))
            {
                MessageBox.Show("Введите корректную скидку (целое число).", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (discount < 0)
            {
                MessageBox.Show("Скидка не может быть отрицательной!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (discount > 99)
            {
                MessageBox.Show("Скидка не может превышать 99%!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            currentProduct.ДействующаяСкидка = discount;

            int qty;
            if (!int.TryParse(TxtQuantity.Text, out qty))
            {
                MessageBox.Show("Введите корректное количество на складе (целое число).", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (qty < 0)
            {
                MessageBox.Show("Количество на складе не может быть отрицательным!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            currentProduct.КолВоНаСкладе = qty;

            try
            {
                if (newPhotoPath != null)
                {
                    string assetsFolder = System.IO.Path.Combine(Environment.CurrentDirectory, "Assets");
                    if (!System.IO.Directory.Exists(assetsFolder))
                    {
                        System.IO.Directory.CreateDirectory(assetsFolder);
                    }
                    string newFileName = System.IO.Path.GetFileName(newPhotoPath);
                    string destinationPath = System.IO.Path.Combine(assetsFolder, newFileName);

                    if (isEditMode && !string.IsNullOrEmpty(currentProduct.Фото))
                    {
                        string oldPath = System.IO.Path.Combine(assetsFolder, currentProduct.Фото);
                        if (File.Exists(oldPath) && oldPath != destinationPath)
                        {
                            File.Delete(oldPath);
                        }
                    }
                    if (!File.Exists(destinationPath))
                    {
                        File.Copy(newPhotoPath, destinationPath);
                    }
                    currentProduct.Фото = newFileName;
                }

                if (!isEditMode)
                {
                    if (db.Товар.Any(t => t.Артикул == currentProduct.Артикул))
                    {
                        MessageBox.Show("Товар с таким артикулом уже существует.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    db.Товар.Add(currentProduct);
                }

                db.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

       
                if (NavigationService != null)
                {
                    NavigationService.Navigate(new CatalogPage());

              
                    if (NavigationService.CanGoBack)
                        NavigationService.RemoveBackEntry();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += "\n\nДетали: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += "\n\nПричина БД: " + ex.InnerException.InnerException.Message;
                    }
                }
                MessageBox.Show(errorMessage, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}