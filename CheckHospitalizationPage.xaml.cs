using MedicalWPF.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MedicalWPF
{
    /// <summary>
    /// Логика взаимодействия для CheckHospitalizationPage.xaml
    /// </summary>
    public partial class CheckHospitalizationPage : Window
    {
        public CheckHospitalizationPage()
        {
            InitializeComponent();
        }

        private const string API_HOSPITALIZATON_URL = "http://localhost:5036/api/Hospitalizations";
        private const string API_PATIENTS_URL = "http://localhost:5036/api/Patients";

        private List<Patient> patients;
        private List<Hospitalization> hospitalizations;

        Hospitalization hospitalization;
        Patient patient;

        private async void CheckCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            await GetPatients();
            await GetHospitalizationList();

            hospitalization = hospitalizations.FirstOrDefault(x => x.HospitalCode == int.Parse(CodeTxb.Text));

            patient = patients.FirstOrDefault(x => x.Id == hospitalization.IdPatient);

            // Заполнение текстовых полей полученными данными
            FirstNameTxb.Text = patient.Name;
            SecondNameTxb.Text = patient.Surname;
            PatronymicTxb.Text = patient.Patronymic;

            FirstDayTxb.SelectedDate = hospitalization.FirstDay;
            LastDayTxb.SelectedDate = hospitalization.LastDay;

            IsPaidTxb.Text = (bool)hospitalization.IsPaid ? "Да" : "Нет";

            StatusTxb.Text = hospitalization.Status ?? "Нет статуса";
        }

        private async void CancelHospitalizationBtn_Click(object sender, RoutedEventArgs e)
            => await CancelHospitalization();

        private void CancelByTerapistBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ApproveHospitalizationBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        public async Task GetPatients()
        {
            using (HttpClient client = new HttpClient())
            {
                var responce = await client.GetAsync(API_PATIENTS_URL);

                if (!responce.IsSuccessStatusCode)
                {
                    MessageBox.Show(responce.Content.ReadAsStringAsync().ToString());
                }
                else
                {
                    patients = new List<Patient>();

                    patients = JsonConvert.DeserializeObject<List<Patient>>(await responce.Content.ReadAsStringAsync());
                }
            }
        }

        public async Task GetHospitalizationList()
        {
            using (HttpClient client = new HttpClient())
            {
                var responce = await client.GetAsync(API_HOSPITALIZATON_URL);

                if (!responce.IsSuccessStatusCode)
                {
                    MessageBox.Show(responce.Content.ReadAsStringAsync().ToString());
                }
                else
                {
                    hospitalizations = new List<Hospitalization>();

                    hospitalizations = JsonConvert.DeserializeObject<List<Hospitalization>>(await responce.Content.ReadAsStringAsync());
                }
            }
        }

        public async Task CancelHospitalization()
        {
            try
            {
                hospitalization.Status = "Отказ от госпитализации";
                hospitalization.IdPatientNavigation = patient;

                using (var client = new HttpClient())
                {
                    var responce = await client.PutAsJsonAsync($"{API_HOSPITALIZATON_URL}/{hospitalization.Id}", hospitalization);

                    if (!responce.IsSuccessStatusCode)
                    {
                        MessageBox.Show(responce.ReasonPhrase);
                    }
                    else
                    {
                        MessageBox.Show("Успешно!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        
    }
}
