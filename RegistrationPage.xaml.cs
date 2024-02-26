using MedicalWPF.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MedicalWPF
{
    /// <summary>
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Window
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        private const string API_PATIENT_URL = "http://localhost:5036/api/Patients";
        private const string API_MEDCARD_URL = "http://localhost:5036/api/MedCards";
        private int maxMedCardId;
        private List<MedCard> MedCardList;
        private BitmapImage bitmapImage;

        private async void RegPatientBtn_Click(object sender, RoutedEventArgs e)
        {
            await CreateMedCardAndPatient();
        }

        private void SearchPatientBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async Task CreateMedCardAndPatient()
        {
            try
            {
                await CreateMedCard();
                await CreatePatientViaApi();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public async Task CreateMedCard()
        {
            MedCardList = new List<MedCard>();

            try
            {
                var MedCard = new MedCard()
                {
                    DateOfIssue = DateTime.Now,
                    DateOfLastAccess = DateTime.Now,
                    Diagnosis = "Не указан"
                };

                using (var httpClient = new HttpClient())
                {
                    var responce = await httpClient.PostAsJsonAsync(API_MEDCARD_URL, MedCard);
                    responce.EnsureSuccessStatusCode();

                    if (!responce.IsSuccessStatusCode)
                    {
                        MessageBox.Show(await responce.Content.ReadAsStringAsync());
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public async Task GetMedCard()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {

                    var responce = await httpClient.GetAsync(API_MEDCARD_URL);
                    responce.EnsureSuccessStatusCode();

                    if (!responce.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Данные медкарт не получены!");
                    }
                    else
                    {
                        string json = await responce.Content.ReadAsStringAsync();
                        MedCardList = JsonConvert.DeserializeObject<List<MedCard>>(json);
                    }

                    // Получаем максимальный ID медкарт
                    maxMedCardId = MedCardList.Max(m => m.Id);
                }
            }
            catch
            {

            }
        }

        // Отправка данных пациента через API
        public async Task CreatePatientViaApi()
        {
            await GetMedCard();

            var Patient = new Patient
            {
                Name = FirstNameTxtB.Text,
                Surname = SecondNameTxtB.Text,
                Patronymic = PatronymicTxtB.Text,
                Passport = (string)PassportTxtB.Text,
                Birthdate = BirthDayPicker.SelectedDate.Value,
                Sex = SexCmbBox.SelectedIndex == 0 ? "Мужской" : "Женский",
                Address = AddressTxtB.Text,
                Phonenumber = (string)PassportTxtB.Text,
                Email = EmailTxtB.Text,
                InsurancePolicyNum = (string)PolicyNumTxtB.Text,
                InsurancePolicyExpDate = PolicyExpDatePicker.SelectedDate.Value,

                IdMedCard = maxMedCardId,
                Photo = PatientPhotoToByteArray(bitmapImage)

            };

            using (HttpClient client = new HttpClient())
            {

                // Отправка POST-запроса
                var responce = await client.PostAsJsonAsync(API_PATIENT_URL, Patient);

                if (!responce.IsSuccessStatusCode)
                {
                    MessageBox.Show(await responce.Content.ReadAsStringAsync());
                }
                else
                {
                    MessageBox.Show("Пациент успешно зарегистрирован в системе!", "Успешная регистрация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void SelectPhotoBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Filter = "Изображения (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
                };

                if ((bool)openFileDialog.ShowDialog())
                {
                    string selectedImagePath = openFileDialog.FileName;

                    // Отображение фото в форме
                    try
                    {
                        bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(selectedImagePath);
                        bitmapImage.EndInit();

                        PatientPhoto.Source = bitmapImage;
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка при отображении фото");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private byte[] PatientPhotoToByteArray(BitmapImage bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(memoryStream);

                return memoryStream.ToArray();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            => new Launcher().Show();
    }
}
