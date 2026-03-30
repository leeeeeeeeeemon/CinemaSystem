using System.Windows;
using System.Windows.Input;
using CinemaSystem.Models;
using CinemaSystem.Data;

namespace CinemaSystem.Views
{
    public partial class AddEditHallWindow : Window
    {
        private readonly Hall _hall;
        private readonly bool _isEditMode;

        public AddEditHallWindow(Hall? hallToEdit = null)
        {
            InitializeComponent();

            if (hallToEdit != null)
            {
                _isEditMode = true;
                _hall = hallToEdit;
                Title = "Редактирование кинозала";
                LoadData();
            }
            else
            {
                _hall = new Hall();
                Title = "Добавление нового кинозала";
            }
        }

        private void LoadData()
        {
            txtHallNumber.Text = _hall.HallNumber.ToString();
            txtCapacity.Text = _hall.Capacity.ToString();
            txtDescription.Text = _hall.Description;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtHallNumber.Text, out int hallNumber) || hallNumber <= 0)
            {
                MessageBox.Show("Номер зала должен быть положительным числом!", "Ошибка");
                return;
            }

            if (!int.TryParse(txtCapacity.Text, out int capacity) || capacity <= 0)
            {
                MessageBox.Show("Вместимость должна быть больше 0!", "Ошибка");
                return;
            }

            _hall.HallNumber = hallNumber;
            _hall.Capacity = capacity;
            _hall.Description = txtDescription.Text.Trim();

            using (var db = new CinemaDbContext())
            {
                if (_isEditMode)
                {
                    var existing = db.Halls.Find(_hall.Id);
                    if (existing != null)
                    {
                        existing.HallNumber = _hall.HallNumber;
                        existing.Capacity = _hall.Capacity;
                        existing.Description = _hall.Description;
                    }
                }
                else
                {
                    db.Halls.Add(_hall);
                }
                db.SaveChanges();
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text[0]);
        }
    }
}