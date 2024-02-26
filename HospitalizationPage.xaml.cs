using MedicalWPF.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;

namespace MedicalWPF
{
    /// <summary>
    /// Логика взаимодействия для HospitalizationPage.xaml
    /// </summary>
    public partial class HospitalizationPage : Window
    {
        public HospitalizationPage()
        {
            InitializeComponent();
            GetData();
        }

        private const string API_HOSPITALIZATON_URL = "http://localhost:5036/api/Hospitalizations";

        public async void GetData()
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
                    DataGrid.ItemsSource = await responce.Content.ReadFromJsonAsync<List<Hospitalization>>();
                }
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
            => GetData();

        private void AddHospitalizationBtn_Click(object sender, RoutedEventArgs e)
        {
            new CheckHospitalizationPage().Show();
            Close();
        }

        private void RegBtn_Click(object sender, RoutedEventArgs e)
        {
            new RegistrationPage().Show();
            Close();
        }
    }
}
