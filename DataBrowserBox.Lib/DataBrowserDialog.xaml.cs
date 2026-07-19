using System;
using DataBrowserBox.Lib.Models;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DataBrowserBox.Lib
{
    public partial class DataBrowserDialog : Window
    {
        private ICollectionView _view;

        public DataBrowserDialog()
        {
            InitializeComponent();

            // Keyboard support:
            // - Enter to accept when an item is selected
            // - Escape to cancel
            // - Typing while focus is not in a text input moves focus to the filter and inserts the char
            PreviewKeyDown += Window_PreviewKeyDown;
            PreviewTextInput += Window_PreviewTextInput;
        }

        public IEnumerable ItemsSource
        {
            get => Grid.ItemsSource as IEnumerable;
            set
            {
                // Clear filter on previous view to avoid the view holding a reference
                if (_view != null)
                {
                    _view.Filter = null;
                    _view = null;
                }

                if (value == null)
                {
                    Grid.ItemsSource = null;
                    return;
                }

                _view = CollectionViewSource.GetDefaultView(value);
                _view.Filter = Filter;
                Grid.ItemsSource = _view;
            }
        }

        public object SelectedItem
        {
            get => Grid.SelectedItem;
            set => Grid.SelectedItem = value;
        }

        public ObservableCollection<BrowserColumn> Columns
        {
            get { return (ObservableCollection<BrowserColumn>)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(
                nameof(Columns),
                typeof(ObservableCollection<BrowserColumn>),
                typeof(DataBrowserDialog),
                new PropertyMetadata(null, OnColumnsChanged));

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataBrowserDialog dialog)
            {
                dialog.BuildColumns(dialog.ItemsSource);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Grid.SelectedItem != null)
                {
                    SelectedItem = Grid.SelectedItem;
                    DialogResult = true;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
            }
        }

        private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // If the focus is already inside a text input (FilterTextBox or other), let it handle input.
            if (Keyboard.FocusedElement is TextBox)
                return;

            // Only handle printable text
            if (string.IsNullOrEmpty(e.Text))
                return;

            FilterTextBox.Focus();

            // Preserve current caret position; if selection, replace selection.
            var tb = FilterTextBox;
            var selStart = tb.SelectionStart >= 0 ? tb.SelectionStart : tb.Text.Length;
            var selLength = tb.SelectionLength;

            if (selLength > 0)
            {
                tb.Text = tb.Text.Remove(selStart, selLength);
            }

            tb.Text = tb.Text.Insert(selStart, e.Text);
            tb.SelectionStart = selStart + e.Text.Length;
            tb.SelectionLength = 0;

            // Trigger the filter refresh immediately
            _view?.Refresh();

            e.Handled = true;
        }

        private void Grid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (Grid.SelectedItem == null)
                return;

            SelectedItem = Grid.SelectedItem;
        }

        private bool Filter(object item)
        {
            if (string.IsNullOrWhiteSpace(FilterTextBox.Text))
                return true;

            var filterText = FilterTextBox.Text.ToLowerInvariant();

            if (Columns != null && Columns.Any())
            {
                foreach (var col in Columns)
                {
                    var prop = item.GetType().GetProperty(col.DataField);
                    if (prop?.PropertyType == typeof(string))
                    {
                        var value = prop.GetValue(item) as string;
                        if (!string.IsNullOrEmpty(value) &&
                            value.ToLowerInvariant().Contains(filterText))
                            return true;
                    }
                }
                return false;
            }

            foreach (var prop in item.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(string))
                {
                    var value = prop.GetValue(item) as string;
                    if (!string.IsNullOrEmpty(value) &&
                        value.ToLowerInvariant().Contains(filterText))
                        return true;
                }
            }

            return false;
        }

        private void BuildColumns(IEnumerable itemsSource)
        {
            Grid.Columns.Clear();

            if (Columns != null && Columns.Any())
            {
                BuildCustomColumns();
                Grid.AutoGenerateColumns = false;
            }
            else
            {
                Grid.AutoGenerateColumns = true;
            }
        }

        private void BuildCustomColumns()
        {
            foreach (var col in Columns)
            {
                var binding = new Binding(col.DataField);

                if (!string.IsNullOrWhiteSpace(col.Format))
                    binding.StringFormat = col.Format;

                // Width handling:
                // - null/empty or "Auto" => Auto
                // - "123" => fixed pixel width
                // - "*" => star (1 unit)
                // - "#*" where # is a number (e.g. "2*") => star with weight # (takes multiple shares of remaining space)
                // Any invalid values fall back to Auto.
                DataGridLength width;

                var raw = (col.Width ?? string.Empty).Trim();

                if (string.IsNullOrEmpty(raw) ||
                    string.Equals(raw, "Auto", System.StringComparison.OrdinalIgnoreCase))
                {
                    width = DataGridLength.Auto;
                }
                else if (raw.EndsWith("*"))
                {
                    // support "*" and "N*"
                    var prefix = raw.Substring(0, raw.Length - 1).Trim();
                    double weight = 1.0;
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        if (!double.TryParse(prefix, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out weight) || weight <= 0)
                        {
                            weight = 1.0;
                        }
                    }
                    width = new DataGridLength(weight, DataGridLengthUnitType.Star);
                }
                else if (double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsedWidth) && parsedWidth > 0)
                {
                    width = new DataGridLength(parsedWidth, DataGridLengthUnitType.Pixel);
                }
                else
                {
                    width = DataGridLength.Auto;
                }

                var gridColumn = new DataGridTextColumn
                {
                    Header = col.Header,
                    Binding = binding,
                    Width = width
                };

                Grid.Columns.Add(gridColumn);
            }
        }

        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _view?.Refresh();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ClearFilter_OnClick(object sender, RoutedEventArgs e)
        {
            FilterTextBox.Text = string.Empty;
        }

        // Remove references that can keep this dialog alive after close.
        protected override void OnClosed(EventArgs e)
        {
            if (_view != null)
            {
                _view.Filter = null;
                _view = null;
            }

            PreviewKeyDown -= Window_PreviewKeyDown;
            PreviewTextInput -= Window_PreviewTextInput;

            Grid.SelectionChanged -= Grid_SelectionChanged;
            base.OnClosed(e);
        }

        private void Grid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(SelectedItem != null)
                DialogResult = true;
        }
    }
}
