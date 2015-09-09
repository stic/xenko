﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Core;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Controls
{
    [TemplatePart(Name = "PART_EditableTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_ListBox", Type = typeof(ListBox))]
    public class FilteringComboBox : Selector
    {
        /// <summary>
        /// A dependency property used to safely evaluate the value of an item given a path.
        /// </summary>
        private static readonly DependencyProperty InternalValuePathProperty = DependencyProperty.Register("InternalValuePath", typeof(object), typeof(FilteringComboBox));
        /// <summary>
        /// The input text box.
        /// </summary>
        private TextBox editableTextBox;
        /// <summary>
        /// The filtered list box.
        /// </summary>
        private ListBox listBox;
        /// <summary>
        /// Indicates that the selection is being internally cleared and that the drop down should not be opened nor refreshed.
        /// </summary>
        private bool clearing;
        /// <summary>
        /// Indicates that the selection is being internally updated and that the text should not be cleared.
        /// </summary>
        private bool updatingSelection;
        /// <summary>
        /// Indicates that the text box is being validated and that the update of the text should not impact the selected item.
        /// </summary>
        private bool validating;

        /// <summary>
        /// Identifies the <see cref="RequireSelectedItemToValidate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RequireSelectedItemToValidateProperty = DependencyProperty.Register("RequireSelectedItemToValidate", typeof(bool), typeof(FilteringComboBox));

        /// <summary>
        /// Identifies the <see cref="Text"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FilteringComboBox), new FrameworkPropertyMetadata { DefaultUpdateSourceTrigger = UpdateSourceTrigger.Explicit, BindsTwoWayByDefault = true });

        /// <summary>
        /// Identifies the <see cref="IsDropDownOpen"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(FilteringComboBox));

        /// <summary>
        /// Identifies the <see cref="UpdateSelectionOnValidation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UpdateSelectionOnValidationProperty = DependencyProperty.Register("UpdateSelectionOnValidation", typeof(bool), typeof(FilteringComboBox));

        /// <summary>
        /// Identifies the <see cref="ClearTextAfterValidation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ClearTextAfterValidationProperty = DependencyProperty.Register("ClearTextAfterValidation", typeof(bool), typeof(FilteringComboBox));

        /// <summary>
        /// Identifies the <see cref="WatermarkContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkContentProperty = DependencyProperty.Register("WatermarkContent", typeof(object), typeof(FilteringComboBox), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ItemsToExclude"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemsToExcludeProperty = DependencyProperty.Register("ItemsToExclude", typeof(IEnumerable), typeof(FilteringComboBox));

        /// <summary>
        /// Identifies the <see cref="Sort"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SortProperty = DependencyProperty.Register("Sort", typeof(FilteringComboBoxSort), typeof(FilteringComboBox), new FrameworkPropertyMetadata(OnItemsSourceRefresh));

        /// <summary>
        /// Identifies the <see cref="ValidatedValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidatedValueProperty = DependencyProperty.Register("ValidatedValue", typeof(object), typeof(FilteringComboBox));

        /// <summary>
        /// Identifies the <see cref="ValidatedItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidatedItemProperty = DependencyProperty.Register("ValidatedItem", typeof(object), typeof(FilteringComboBox));

        /// <summary>
        /// Raised just before the TextBox changes are validated. This event is cancellable
        /// </summary>
        public static readonly RoutedEvent ValidatingEvent = EventManager.RegisterRoutedEvent("Validating", RoutingStrategy.Bubble, typeof(CancelRoutedEventHandler), typeof(FilteringComboBox));

        /// <summary>
        /// Raised when TextBox changes have been validated.
        /// </summary>
        public static readonly RoutedEvent ValidatedEvent = EventManager.RegisterRoutedEvent("Validated", RoutingStrategy.Bubble, typeof(ValidationRoutedEventHandler<string>), typeof(FilteringComboBox));

        static FilteringComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilteringComboBox), new FrameworkPropertyMetadata(typeof(FilteringComboBox)));
        }

        public FilteringComboBox()
        {
            IsTextSearchEnabled = false;
        }

        /// <summary>
        /// Gets or sets whether the drop down is open.
        /// </summary>
        public bool IsDropDownOpen { get { return (bool)GetValue(IsDropDownOpenProperty); } set { SetValue(IsDropDownOpenProperty, value); } }

        /// <summary>
        /// Gets or sets whether the validation will be cancelled if <see cref="SelectedItem"/> is null.
        /// </summary>
        public bool RequireSelectedItemToValidate { get { return (bool)GetValue(RequireSelectedItemToValidateProperty); } set { SetValue(RequireSelectedItemToValidateProperty, value); } }

        /// <summary>
        /// Gets or sets the text of this <see cref="FilteringComboBox"/>
        /// </summary>
        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }

        /// <summary>
        /// Gets or sets whether to clear the text after the validation.
        /// </summary>
        public bool ClearTextAfterValidation { get { return (bool)GetValue(ClearTextAfterValidationProperty); } set { SetValue(ClearTextAfterValidationProperty, value); } }

        /// <summary>
        /// Gets or sets the content to display when the TextBox is empty.
        /// </summary>
        public object WatermarkContent { get { return GetValue(WatermarkContentProperty); } set { SetValue(WatermarkContentProperty, value); } }

        [Obsolete]
        public IEnumerable ItemsToExclude { get { return (IEnumerable)GetValue(ItemsToExcludeProperty); } set { SetValue(ItemsToExcludeProperty, value); } }

        /// <summary>
        /// Defines how choices are sorted.
        /// </summary>
        public FilteringComboBoxSort Sort { get { return (FilteringComboBoxSort)GetValue(SortProperty); } set { SetValue(SortProperty, value); } }

        public object ValidatedValue { get { return GetValue(ValidatedValueProperty); } set { SetValue(ValidatedValueProperty, value); } }

        public object ValidatedItem { get { return GetValue(ValidatedItemProperty); } set { SetValue(ValidatedItemProperty, value); } }

        /// <summary>
        /// Raised just before the TextBox changes are validated. This event is cancellable
        /// </summary>
        public event CancelRoutedEventHandler Validating { add { AddHandler(ValidatingEvent, value); } remove { RemoveHandler(ValidatingEvent, value); } }

        /// <summary>
        /// Raised when TextBox changes have been validated.
        /// </summary>
        public event ValidationRoutedEventHandler<string> Validated { add { AddHandler(ValidatedEvent, value); } remove { RemoveHandler(ValidatedEvent, value); } }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            if (newValue != null)
            {
                var cvs = (CollectionView)CollectionViewSource.GetDefaultView(newValue);
                cvs.Filter = InternalFilter;
                var listCollectionView = cvs as ListCollectionView;
                if (listCollectionView != null)
                {
                    listCollectionView.CustomSort = Sort;
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            editableTextBox = GetTemplateChild("PART_EditableTextBox") as TextBox;
            if (editableTextBox == null)
                throw new InvalidOperationException("A part named 'PART_EditableTextBox' must be present in the ControlTemplate, and must be of type 'SiliconStudio.Presentation.Controls.Input.TextBox'.");

            listBox = GetTemplateChild("PART_ListBox") as ListBox;
            if (listBox == null)
                throw new InvalidOperationException("A part named 'PART_ListBox' must be present in the ControlTemplate, and must be of type 'ListBox'.");

            editableTextBox.TextChanged += EditableTextBoxTextChanged;
            editableTextBox.PreviewKeyDown += EditableTextBoxPreviewKeyDown;
            editableTextBox.Validating += EditableTextBoxValidating;
            editableTextBox.Validated += EditableTextBoxValidated;
            editableTextBox.Cancelled += EditableTextBoxCancelled;
            editableTextBox.LostFocus += EditableTextBoxLostFocus;
            listBox.PreviewMouseUp += ListBoxMouseUp;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (SelectedItem == null && !updatingSelection)
            {
                clearing = true;
                editableTextBox.Clear();
                clearing = false;
            }
        }

        private static void OnItemsSourceRefresh(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var filteringComboBox = (FilteringComboBox)d;
            filteringComboBox.OnItemsSourceChanged(filteringComboBox.ItemsSource, filteringComboBox.ItemsSource);
        }

        private void EditableTextBoxValidating(object sender, CancelRoutedEventArgs e)
        {
            // This may happens somehow when the template is refreshed.
            if (!ReferenceEquals(sender, editableTextBox))
                return;

            // If we require a selected item but there is none, cancel the validation
            BindingExpression expression;
            if (RequireSelectedItemToValidate && SelectedItem == null)
            {
                e.Cancel = true;
                expression = GetBindingExpression(TextProperty);
                expression?.UpdateTarget();
                editableTextBox.Cancel();
                return;
            }

            validating = true;

            // Update the validated properties
            ValidatedValue = SelectedValue;
            ValidatedItem = SelectedItem;

            // If the dropdown is still open and something is selected, use the string from the selected item
            if (SelectedItem != null && IsDropDownOpen)
            {
                var displayValue = ResolveDisplayMemberValue(SelectedItem);
                editableTextBox.Text = displayValue?.ToString();
                editableTextBox.CaretIndex = editableTextBox.Text.Length;
            }

            // Update the source of the text property binding
            expression = GetBindingExpression(TextProperty);
            expression?.UpdateSource();

            // Close the dropdown
            if (IsDropDownOpen)
                IsDropDownOpen = false;

            validating = false;

            var cancelRoutedEventArgs = new CancelRoutedEventArgs(ValidatingEvent);
            RaiseEvent(cancelRoutedEventArgs);
            if (cancelRoutedEventArgs.Cancel)
                e.Cancel = true;
        }

        private void EditableTextBoxValidated(object sender, ValidationRoutedEventArgs<string> e)
        {
            // This may happens somehow when the template is refreshed.
            if (!ReferenceEquals(sender, editableTextBox))
                return;

            var validatedArgs = new RoutedEventArgs(ValidatedEvent);
            RaiseEvent(validatedArgs);

            if (ClearTextAfterValidation)
            {
                clearing = true;
                editableTextBox.Text = string.Empty;
                clearing = false;
            }
        }

        private async void EditableTextBoxCancelled(object sender, RoutedEventArgs e)
        {
            // This may happens somehow when the template is refreshed.
            if (!ReferenceEquals(sender, editableTextBox))
                return;

            var expression = GetBindingExpression(TextProperty);
            expression?.UpdateTarget();

            clearing = true;
            await Task.Delay(100);
            IsDropDownOpen = false;
            clearing = false;
        }

        private async void EditableTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            // This may happens somehow when the template is refreshed.
            if (!ReferenceEquals(sender, editableTextBox))
                return;
            
            clearing = true;
            editableTextBox.Validate();

            // Defer closing the popup in case we lost the focus because of a click in the list box - so it can still raise the correct event
            // This is a very hackish, we should find a better way to do it!
            await Task.Delay(100);
            IsDropDownOpen = false;
            clearing = false;
        }

        private void EditableTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ItemsSource == null)
                return;

            updatingSelection = true;
            if (!IsDropDownOpen && !clearing && IsKeyboardFocusWithin)
            {
                // Setting IsDropDownOpen to true will select all the text. We don't want this behavior, so let's save and restore the caret index.
                var index = editableTextBox.CaretIndex;
                IsDropDownOpen = true;
                editableTextBox.CaretIndex = index;
            }
            if (Sort != null)
                Sort.Token = editableTextBox.Text;

            // TODO: this will update the selected index because the collection view is shared. If UpdateSelectionOnValidation is true, this will still modify the SelectedIndex
            var collectionView = CollectionViewSource.GetDefaultView(ItemsSource);
            collectionView.Filter = InternalFilter;
            var listCollectionView = collectionView as ListCollectionView;
            if (listCollectionView != null)
            {
                listCollectionView.CustomSort = Sort;
            }

            collectionView.Refresh();
            if (!validating)
            {
                if (listCollectionView?.Count > 0 || collectionView.Cast<object>().Any())
                {
                    listBox.SelectedIndex = 0;
                }
            }
            updatingSelection = false;
        }

        private void EditableTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (listBox.Items.Count > 0)
            {
                updatingSelection = true;
                if (e.Key == Key.Escape)
                {
                    if (IsDropDownOpen)
                    {
                        IsDropDownOpen = false;
                        if (RequireSelectedItemToValidate)
                            editableTextBox.Cancel();
                    }
                    else
                    {
                        editableTextBox.Cancel();
                    }
                }
                if (e.Key == Key.Up)
                {
                    listBox.SelectedIndex = Math.Max(listBox.SelectedIndex - 1, 0);
                    BringSelectedItemIntoView();
                }
                if (e.Key == Key.Down)
                {
                    listBox.SelectedIndex = Math.Min(listBox.SelectedIndex + 1, listBox.Items.Count - 1);
                    BringSelectedItemIntoView();
                }
                if (e.Key == Key.PageUp)
                {
                    var stackPanel = listBox.FindVisualChildOfType<VirtualizingStackPanel>();
                    if (stackPanel != null)
                    {
                        var count = stackPanel.Children.Count;
                        listBox.SelectedIndex = Math.Max(listBox.SelectedIndex - count, 0);
                    }
                    else
                    {
                        listBox.SelectedIndex = 0;
                    }
                    BringSelectedItemIntoView();
                }
                if (e.Key == Key.PageDown)
                {
                    var stackPanel = listBox.FindVisualChildOfType<VirtualizingStackPanel>();
                    if (stackPanel != null)
                    {
                        var count = stackPanel.Children.Count;
                        listBox.SelectedIndex = Math.Min(listBox.SelectedIndex + count, listBox.Items.Count - 1);
                    }
                    else
                    {
                        listBox.SelectedIndex = listBox.Items.Count - 1;
                    }
                    BringSelectedItemIntoView();
                }
                if (e.Key == Key.Home)
                {
                    listBox.SelectedIndex = 0;
                    BringSelectedItemIntoView();
                }
                if (e.Key == Key.End)
                {
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                    BringSelectedItemIntoView();
                }
                updatingSelection = false;
            }
        }

        private void ListBoxMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && listBox.SelectedIndex > -1)
            {
                editableTextBox.Validate();
            }
        }

        private void BringSelectedItemIntoView()
        {
            var selectedItem = listBox.SelectedItem;
            if (selectedItem != null)
                listBox.ScrollIntoView(selectedItem);
        }

        private bool InternalFilter(object obj)
        {
            var filter = editableTextBox?.Text;
            if (string.IsNullOrWhiteSpace(filter))
                return true;

            if (obj == null)
                return false;

            if (ItemsToExclude != null && ItemsToExclude.Cast<object>().Contains(obj))
                return false;

            var value = ResolveDisplayMemberValue(obj);
            var text = value?.ToString();
            return MatchText(filter, text);
        }

        private static bool MatchText(string inputText, string text)
        {
            return text.IndexOf(inputText, StringComparison.InvariantCultureIgnoreCase) > -1 || MatchCamelCase(inputText, text);
        }

        private object ResolveDisplayMemberValue(object obj)
        {
            var value = obj;
            try
            {
                SetBinding(InternalValuePathProperty, new Binding(DisplayMemberPath) { Source = obj });
                value = GetValue(InternalValuePathProperty);
            }
            catch (Exception e)
            {
                e.Ignore();
            }
            finally
            {
                BindingOperations.ClearBinding(this, InternalValuePathProperty);
            }
            return value;
        }

        private static bool MatchCamelCase(string inputText, string text)
        {
            var camelCaseSplit = text.CamelCaseSplit();
            var filter = inputText.ToLowerInvariant();
            int currentFilterChar = 0;

            foreach (var word in camelCaseSplit)
            {
                int currentWordChar = 0;
                while (currentFilterChar > 0)
                {
                    if (char.ToLower(word[currentWordChar]) == filter[currentFilterChar])
                        break;
                    --currentFilterChar;
                }

                while (char.ToLower(word[currentWordChar]) == filter[currentFilterChar])
                {
                    ++currentWordChar;
                    ++currentFilterChar;
                    if (currentFilterChar == filter.Length)
                        return true;

                    if (currentWordChar == word.Length)
                        break;
                }
            }
            return currentFilterChar == filter.Length;
        }
    }
}
