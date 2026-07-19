using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using DataBrowserBox.Demo.Models;

namespace DataBrowserBox.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<Product> _products;
        public List<Product> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(); }
        }

        private Product? _auto;
        public Product? Auto
        {
            get => _auto;
            set { _auto = value; OnPropertyChanged(); }
        }

        private Product? _custom;
        public Product? Custom
        {
            get => _custom;
            set { _custom = value; OnPropertyChanged(); }
        }

        public MainWindow()
        {
            InitializeComponent();
            FillProducts();
            DataContext = this;
        }

        private void FillProducts()
        {
            Products = new List<Product>
            {
                new Product { Id = 1, Code = "LAP-001", Name = "Laptop Pro 15", Category = "Computers", Price = 1299.99m, Stock = 45 },
                new Product { Id = 2, Code = "LAP-002", Name = "Laptop Air 13", Category = "Computers", Price = 999.99m, Stock = 78 },
                new Product { Id = 3, Code = "MON-001", Name = "Ultra Monitor 27", Category = "Displays", Price = 449.99m, Stock = 32 },
                new Product { Id = 4, Code = "KEY-001", Name = "Mechanical Keyboard", Category = "Peripherals", Price = 149.99m, Stock = 156 },
                new Product { Id = 5, Code = "SSD-001", Name = "Portable SSD 1TB", Category = "Storage", Price = 129.99m, Stock = 198 }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}