using System;
using System.Data.SQLite;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string DbFile = @"F:\SqLiteBdsForLabs\Lab1FullStack.db";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnGenerateRequest_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Виконання запиту...";
            progressBar.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = true;
            try
            {
                string url = "https://randomuser.me/api/?results=3";
                string response = await GetApiResponse(url);

                JObject json = JObject.Parse(response);
                var results = json["results"];

                txtOutput.Text = "";

                using (var conn = new SQLiteConnection($"Data Source={DbFile};Version=3;"))
                {
                    conn.Open();

                    foreach (var user in results)
                    {
                        string title = user["name"]["title"].ToString();
                        string firstName = user["name"]["first"].ToString();
                        string lastName = user["name"]["last"].ToString();
                        string fullName = $"{title} {firstName} {lastName}";
                        string email = user["email"].ToString();
                        string country = user["location"]["country"].ToString();
                        string city = user["location"]["city"].ToString();
                        string street = $"{user["location"]["street"]["number"]} {user["location"]["street"]["name"]}";
                        string phone = user["phone"].ToString();
                        string cell = user["cell"].ToString();
                        string dob = user["dob"]["date"].ToString();
                        int age = (int)user["dob"]["age"];
                        string uuid = user["login"]["uuid"].ToString();
                        string picture = user["picture"]["large"].ToString();

                        // Вивiд iнф. в TextBlock
                        txtOutput.Text +=
                            $"Name: {fullName}\n" +
                            $"Email: {email}\n" +
                            $"Adress: {country}, {city}, {street}\n" +
                            $"Phone: {phone} | Моб: {cell}\n" +
                            $"Birthday Date: {dob} (Age: {age})\n" +
                            $"UUID: {uuid}\n" +
                            $"Photo: {picture}\n" +
                            $"-----------------------------------------\n";

                        // Збереження в БД
                        string insertSql = @"
                        INSERT OR IGNORE INTO Users 
                        (UUID, Title, FirstName, LastName, Email, Country, City, Street, Phone, Cell, DateOfBirth, Age, PictureUrl) 
                        VALUES (@UUID, @Title, @FirstName, @LastName, @Email, @Country, @City, @Street, @Phone, @Cell, @DateOfBirth, @Age, @PictureUrl)";

                        using (var cmd = new SQLiteCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@UUID", uuid);
                            cmd.Parameters.AddWithValue("@Title", title);
                            cmd.Parameters.AddWithValue("@FirstName", firstName);
                            cmd.Parameters.AddWithValue("@LastName", lastName);
                            cmd.Parameters.AddWithValue("@Email", email);
                            cmd.Parameters.AddWithValue("@Country", country);
                            cmd.Parameters.AddWithValue("@City", city);
                            cmd.Parameters.AddWithValue("@Street", street);
                            cmd.Parameters.AddWithValue("@Phone", phone);
                            cmd.Parameters.AddWithValue("@Cell", cell);
                            cmd.Parameters.AddWithValue("@DateOfBirth", dob);
                            cmd.Parameters.AddWithValue("@Age", age);
                            cmd.Parameters.AddWithValue("@PictureUrl", picture);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                txtStatus.Text = "Запит виконаний та данi були збереженi в БД";
            }
            catch (Exception ex)
            {
                txtOutput.Text = $"Помилка: {ex.Message}";
                txtStatus.Text = "Помилка";
            }
            finally
            {
                progressBar.Visibility = Visibility.Collapsed;
                progressBar.IsIndeterminate = false;
            }
        }

        private async Task<string> GetApiResponse(string url)
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private void BtnViewUsers_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Завантаження користувачiв з БД...";
            txtOutput.Text = "";
            progressBar.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = true;

            try
            {
                using (var conn = new SQLiteConnection($"Data Source={DbFile};Version=3;"))
                {
                    conn.Open();

                    string selectSql = "SELECT * FROM Users";
                    using (var cmd = new SQLiteCommand(selectSql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string fullName = $"{reader["Title"]} {reader["FirstName"]} {reader["LastName"]}";
                            string email = reader["Email"].ToString();
                            string country = reader["Country"].ToString();
                            string city = reader["City"].ToString();
                            string street = reader["Street"].ToString();
                            string phone = reader["Phone"].ToString();
                            string cell = reader["Cell"].ToString();
                            string dob = reader["DateOfBirth"].ToString();
                            string age = reader["Age"].ToString();
                            string uuid = reader["UUID"].ToString();
                            string picture = reader["PictureUrl"].ToString();

                            txtOutput.Text +=
                                $"Name: {fullName}\n" +
                                $"Email: {email}\n" +
                                $"Adress: {country}, {city}, {street}\n" +
                                $"Phone: {phone} | Моб: {cell}\n" +
                                $"Birthday Date: {dob} (Age: {age})\n" +
                                $"UUID: {uuid}\n" +
                                $"Photo: {picture}\n" +
                                $"-----------------------------------------\n";
                        }
                    }
                }

                txtStatus.Text = "Завантаженi данi з БД";
            }
            catch (Exception ex)
            {
                txtOutput.Text = $"Помилка: {ex.Message}";
                txtStatus.Text = "Помилка";
            }
            finally
            {
                progressBar.Visibility = Visibility.Collapsed;
                progressBar.IsIndeterminate = false;
            }
        }

        private void BtnClearOutput_Click(object sender, RoutedEventArgs e)
        {
            txtOutput.Text = "";
            txtStatus.Text = "";
        }

    }
}
