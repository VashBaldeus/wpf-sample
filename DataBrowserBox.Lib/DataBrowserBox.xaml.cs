using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using DataBrowserBox.Lib.Models;

namespace DataBrowserBox.Lib
{
    public partial class DataBrowserBox : UserControl
    {
        public DataBrowserBox()
        {
            InitializeComponent();

            // Ensure each control instance gets its own collection instance.
            Columns ??= [];
        }

        // ItemSource Property
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(DataBrowserBox));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        // SelectedItem (TwoWay)
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(DataBrowserBox),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedItemChanged));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        // DisplayMemberPath
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(
                nameof(DisplayMemberPath),
                typeof(string),
                typeof(DataBrowserBox));

        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        // DialogTitle
        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(
                nameof(DialogTitle),
                typeof(string),
                typeof(DataBrowserBox));

        public string DialogTitle
        {
            get => (string)GetValue(DialogTitleProperty);
            set => SetValue(DialogTitleProperty, value);
        }

        public event EventHandler SelectionChanged;

        private static void OnSelectedItemChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (DataBrowserBox)d;
            control.SelectionChanged?.Invoke(control, EventArgs.Empty);
        }

        // Columns DP: default set to null
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(
                nameof(Columns),
                typeof(ObservableCollection<BrowserColumn>),
                typeof(DataBrowserBox),
                new PropertyMetadata(null));

        public ObservableCollection<BrowserColumn> Columns
        {
            get => (ObservableCollection<BrowserColumn>)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemsSource == null)
                return;

            var dialog = new DataBrowserDialog
            {
                Owner = Window.GetWindow(this),
                Title = DialogTitle,
                ItemsSource = ItemsSource,
                SelectedItem = SelectedItem,
                Columns = Columns
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedItem = dialog.SelectedItem;
            }
        }
    }
}
