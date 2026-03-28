using System.Windows;
using System.Windows.Input;
using CinemaSystem.Models;
using CinemaSystem.Data;

namespace CinemaSystem.Views
{
    public partial class AddEditVisitorWindow : Window
    {
        private readonly Visitor _visitor;
        private readonly bool _isEditMode;

        public AddEditVisitorWindow()
        {
            InitializeComponent();
            _visitor = new Visitor();
            _isEditMode = false;
            Title = "Добавление посетителя";
        }

        public AddEditVisitorWindow(Visitor visitorToEdit)
        {
            InitializeComponent();
            _visitor = visitorToEdit;
            _isEditMode = true;
            Title = "Редактирование посетителя";
            LoadData();
        }

        private void LoadData()
        {
            txtFullName.Text = _visitor.FullName;
            txtPhone.Text = _visitor.Phone ?? "";
            txtBonusCard.Text = _visitor.BonusCardNumber ?? "";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("ФИО посетителя обязательно!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return;
            }

            _visitor.FullName = txtFullName.Text.Trim();
            _visitor.Phone = txtPhone.Text.Trim();
            _visitor.BonusCardNumber = string.IsNullOrWhiteSpace(txtBonusCard.Text)
                ? null
                : txtBonusCard.Text.Trim().ToUpper();

            using (var db = new CinemaDbContext())
            {
                if (_isEditMode)
                {
                    var existing = db.Visitors.Find(_visitor.Id);
                    if (existing != null)
                    {
                        existing.FullName = _visitor.FullName;
                        existing.Phone = _visitor.Phone;
                        existing.BonusCardNumber = _visitor.BonusCardNumber;
                    }
                }
                else
                {
                    db.Visitors.Add(_visitor);
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
    }
}