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
        public AddEditProductPage(Товар selectedProduct)
        {
            InitializeComponent();
            if (selectedProduct != null)
            {
                currentProduct = selectedProduct;
                isEditMode = true;
                TxtArticle.IsReadOnly = true;
                this.Title = "Редактирование товара";
            }
            else
            {
                isEditMode = false;
                TxtArticle.Visibility = Visibility.Collapsed;
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
                        ProductImage.Source = new BitmapImage(new Uri(path));
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
                    
                    BitmapImage img = new BitmapImage(new Uri(openDialog.FileName));
                    if (img.PixelWidth > 300 || img.PixelHeight > 200)
                    {
                        MessageBox.Show("Размер фото не должен превышать 300x200 пикселей!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);

                        return;
                    }
                    newPhotoPath = openDialog.FileName;
                    ProductImage.Source = img;
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Ошибка чтения изображения: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrWhiteSpace(currentProduct.НаименованиеТовара))
            {
                MessageBox.Show("Укажите наименование товара!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            
            if (currentProduct.Цена < 0)
            {
                MessageBox.Show("Стоимость товара не может быть отрицательной!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            
            if (currentProduct.КолВоНаСкладе < 0)
            {
                MessageBox.Show("Количество на складе не может быть отрицательным!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                
                if (newPhotoPath != null)
                {
                    string assetsFolder = System.IO.Path.Combine(Environment.CurrentDirectory, "Assets");
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
                    
                   
                    if (string.IsNullOrEmpty(currentProduct.Артикул))
                    {
                        
                        currentProduct.Артикул = "A" + new Random().Next(1000, 9999).ToString();
                    }

                    db.Товар.Add(currentProduct);
                }

                db.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        
        }
    }
}
