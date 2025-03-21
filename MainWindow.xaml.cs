using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace _4sl6
{
    public partial class MainWindow : Window
    {
        private Regex currentPhoneRegex;
        private string phoneMaskTemplate;
        private string phoneMaskExample;
        private string initialPhoneMask;

        public MainWindow()
        {
            InitializeComponent();
            PhoneTextBox.IsEnabled = false;
        }

        private void UpdatePhoneMask()
        {
            if (CountryComboBox.SelectedItem == null)
                return;

            var countryItem = (ComboBoxItem)CountryComboBox.SelectedItem;
            string country = countryItem.Content.ToString();
            switch (country)
            {
                case "Россия":
                    CountryCodeTextBlock.Text = "+7";
                    break;
                case "Китай":
                    CountryCodeTextBlock.Text = "+86";
                    break;
                case "Эстония":
                    CountryCodeTextBlock.Text = "+372";
                    break;
            }

            if (PhoneTypeListBox.SelectedItem == null)
            {
                PhoneTextBox.Text = "";
                PhoneTextBox.IsEnabled = false;
                return;
            }

            PhoneTextBox.IsEnabled = true;
            var phoneTypeItem = (ListBoxItem)PhoneTypeListBox.SelectedItem;
            string phoneType = phoneTypeItem.Content.ToString();

            currentPhoneRegex = null;
            phoneMaskTemplate = "";
            phoneMaskExample = "";

            switch (country)
            {
                case "Россия":
                    if (phoneType == "Мобильный")
                    {
                        phoneMaskTemplate = "(___) ___-__-__";
                        phoneMaskExample = "(912) 345-67-89";
                        currentPhoneRegex = new Regex(@"^\(\d{3}\)\s\d{3}-\d{2}-\d{2}$");
                    }
                    else
                    {
                        phoneMaskTemplate = "(4012) __-__-__";
                        phoneMaskExample = "(4012) 12-34-56";
                        currentPhoneRegex = new Regex(@"^\(4012\)\s\d{2}-\d{2}-\d{2}$");
                    }
                    break;
                case "Китай":
                    if (phoneType == "Мобильный")
                    {
                        phoneMaskTemplate = "(1__) ____-____";
                        phoneMaskExample = "(138) 1234-5678";
                        currentPhoneRegex = new Regex(@"^\(1\d{2}\)\s\d{4}-\d{4}$");
                    }
                    else
                    {
                        phoneMaskTemplate = "(0__) ____-____";
                        phoneMaskExample = "(010) 1234-5678";
                        currentPhoneRegex = new Regex(@"^\(0\d{2}\)\s\d{4}-\d{4}$");
                    }
                    break;
                case "Эстония":
                    if (phoneType == "Мобильный")
                    {
                        phoneMaskTemplate = "(5__) ___-__";
                        phoneMaskExample = "(512) 345-67";
                        currentPhoneRegex = new Regex(@"^\(5\d{2}\)\s\d{3}-\d{2}$");
                    }
                    else
                    {
                        phoneMaskTemplate = "(6) ___-___";
                        phoneMaskExample = "(6) 123-456";
                        currentPhoneRegex = new Regex(@"^\(6\)\s\d{3}-\d{3}$");
                    }
                    break;
            }

            initialPhoneMask = phoneMaskTemplate;
            PhoneTextBox.Text = phoneMaskTemplate;
            SetCaretToNextPlaceholder();
        }

        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
                return;
            }

            string text = PhoneTextBox.Text;
            List<int> editablePositions = new List<int>();
            for (int i = 0; i < initialPhoneMask.Length; i++)
            {
                if (initialPhoneMask[i] == '_')
                    editablePositions.Add(i);
            }

            int caretPos = PhoneTextBox.SelectionStart;
            int firstFree = PhoneTextBox.Text.IndexOf('_');
            if (firstFree < 0)
                firstFree = PhoneTextBox.Text.Length;
            if (caretPos > firstFree)
            {
                caretPos = firstFree;
                PhoneTextBox.SelectionStart = firstFree;
            }

            int insertionIndex = 0;
            foreach (int pos in editablePositions)
            {
                if (pos < caretPos)
                    insertionIndex++;
            }

            List<char> digits = new List<char>();
            foreach (int pos in editablePositions)
            {
                if (pos < text.Length && char.IsDigit(text[pos]))
                    digits.Add(text[pos]);
            }

            if (digits.Count >= editablePositions.Count)
            {
                e.Handled = true;
                return;
            }

            digits.Insert(insertionIndex, e.Text[0]);

            char[] newText = initialPhoneMask.ToCharArray();
            for (int i = 0; i < digits.Count && i < editablePositions.Count; i++)
            {
                newText[editablePositions[i]] = digits[i];
            }
            PhoneTextBox.Text = new string(newText);

            int newCaret = editablePositions[insertionIndex] + 1;
            PhoneTextBox.SelectionStart = newCaret;
            e.Handled = true;
        }

        private void PhoneTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            string text = PhoneTextBox.Text;
            List<int> editablePositions = new List<int>();
            for (int i = 0; i < initialPhoneMask.Length; i++)
            {
                if (initialPhoneMask[i] == '_')
                    editablePositions.Add(i);
            }

            if (e.Key == Key.Right)
            {
                int firstFree = PhoneTextBox.Text.IndexOf('_');
                if (firstFree < 0)
                    firstFree = PhoneTextBox.Text.Length;
                int newPos = PhoneTextBox.SelectionStart + 1;
                if (newPos > firstFree)
                {
                    PhoneTextBox.SelectionStart = firstFree;
                    e.Handled = true;
                    return;
                }
            }

            if (e.Key == Key.Back)
            {
                int caretPos = PhoneTextBox.SelectionStart;
                int deleteIndex = -1;
                for (int i = editablePositions.Count - 1; i >= 0; i--)
                {
                    if (editablePositions[i] < caretPos && i < editablePositions.Count)
                    {
                        deleteIndex = i;
                        break;
                    }
                }
                if (deleteIndex != -1)
                {
                    List<char> digits = new List<char>();
                    foreach (int pos in editablePositions)
                    {
                        if (pos < text.Length && char.IsDigit(text[pos]))
                            digits.Add(text[pos]);
                    }
                    digits.RemoveAt(deleteIndex);
                    char[] newText = initialPhoneMask.ToCharArray();
                    for (int i = 0; i < digits.Count && i < editablePositions.Count; i++)
                    {
                        newText[editablePositions[i]] = digits[i];
                    }
                    PhoneTextBox.Text = new string(newText);
                    PhoneTextBox.SelectionStart = editablePositions[deleteIndex];
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                int caretPos = PhoneTextBox.SelectionStart;
                int deleteIndex = -1;
                for (int i = 0; i < editablePositions.Count; i++)
                {
                    if (editablePositions[i] >= caretPos)
                    {
                        deleteIndex = i;
                        break;
                    }
                }
                List<char> digits = new List<char>();
                foreach (int pos in editablePositions)
                {
                    if (pos < text.Length && char.IsDigit(text[pos]))
                        digits.Add(text[pos]);
                }
                if (deleteIndex != -1 && deleteIndex < digits.Count)
                {
                    digits.RemoveAt(deleteIndex);
                    char[] newText = initialPhoneMask.ToCharArray();
                    for (int i = 0; i < digits.Count && i < editablePositions.Count; i++)
                    {
                        newText[editablePositions[i]] = digits[i];
                    }
                    PhoneTextBox.Text = new string(newText);
                    PhoneTextBox.SelectionStart = editablePositions[deleteIndex];
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Space || Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;
            }
        }

        private void PhoneTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                int caret = PhoneTextBox.SelectionStart;
                int firstFree = PhoneTextBox.Text.IndexOf('_');
                if (firstFree < 0)
                    firstFree = PhoneTextBox.Text.Length;
                if (caret > firstFree)
                {
                    PhoneTextBox.SelectionStart = firstFree;
                }
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void PhoneTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();
        }

        private void SetCaretToNextPlaceholder()
        {
            int index = PhoneTextBox.Text.IndexOf('_');
            PhoneTextBox.SelectionStart = index >= 0 ? index : PhoneTextBox.Text.Length;
        }

        private void CountryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePhoneMask();
        }

        private void PhoneTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePhoneMask();
        }

        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (!string.IsNullOrWhiteSpace(tb.Text))
            {
                string trimmed = tb.Text.Trim();
                char[] seps = new char[] { ' ' };
                var parts = trimmed.Split(seps, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Length > 0)
                    {
                        string[] subParts = parts[i].Split('-');
                        for (int j = 0; j < subParts.Length; j++)
                        {
                            if (subParts[j].Length > 0)
                            {
                                subParts[j] = char.ToUpper(subParts[j][0]) + subParts[j].Substring(1).ToLower();
                            }
                        }
                        parts[i] = string.Join("-", subParts);
                    }
                }
                tb.Text = string.Join(" ", parts);
            }
        }

        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) &&
                   Regex.IsMatch(name, @"^[A-Za-zА-ЯЁа-яё]+(?:[ -][A-Za-zА-ЯЁа-яё]+)*$");
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (CountryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите страну");
                return;
            }
            if (PhoneTypeListBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип телефона");
                return;
            }

            string lastName = LastNameTextBox.Text.Trim();
            string firstName = FirstNameTextBox.Text.Trim();
            if (!IsValidName(lastName))
            {
                MessageBox.Show("Некорректная фамилия");
                return;
            }
            if (!IsValidName(firstName))
            {
                MessageBox.Show("Некорректное имя");
                return;
            }

            if (!int.TryParse(DayTextBox.Text.Trim(), out int day))
            {
                MessageBox.Show("Введите корректный день рождения");
                return;
            }
            if (MonthComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите месяц рождения");
                return;
            }
            int month = int.Parse(((ComboBoxItem)MonthComboBox.SelectedItem).Tag.ToString());
            if (!int.TryParse(YearTextBox.Text.Trim(), out int year))
            {
                MessageBox.Show("Введите корректный год рождения");
                return;
            }
            DateTime birthDate;
            try
            {
                birthDate = new DateTime(year, month, day);
            }
            catch
            {
                MessageBox.Show("Неверная дата рождения");
                return;
            }
            int age = DateTime.Now.Year - birthDate.Year;
            if (birthDate.Date > DateTime.Now.AddYears(-age))
                age--;
            if (age < 18 || age > 90)
            {
                MessageBox.Show("Возраст должен быть от 18 до 90 лет");
                return;
            }

            string phoneText = PhoneTextBox.Text;
            if (phoneText.Contains("_") || currentPhoneRegex == null || !currentPhoneRegex.IsMatch(phoneText))
            {
                MessageBox.Show($"Некорректный номер телефона. Пример: {phoneMaskExample}");
                return;
            }

            try
            {
                string data = $"{DateTime.Now:yyyy-MM-dd HH:mm}|"
                              + $"{((ComboBoxItem)CountryComboBox.SelectedItem).Content}|"
                              + $"{((ListBoxItem)PhoneTypeListBox.SelectedItem).Content}|"
                              + $"{CountryCodeTextBlock.Text} {phoneText}|"
                              + $"{lastName}|{firstName}|"
                              + $"{birthDate:dd.MM.yyyy}";
                File.AppendAllText("applications.log", data + Environment.NewLine);
                MessageBox.Show("Заявка успешно отправлена!");
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            CountryComboBox.SelectedIndex = -1;
            PhoneTypeListBox.SelectedIndex = -1;
            PhoneTextBox.Text = "";
            CountryCodeTextBlock.Text = "";
            LastNameTextBox.Text = "";
            FirstNameTextBox.Text = "";
            DayTextBox.Text = "";
            YearTextBox.Text = "";
            MonthComboBox.SelectedIndex = -1;
            PhoneTextBox.IsEnabled = false;
        }
    }
}
