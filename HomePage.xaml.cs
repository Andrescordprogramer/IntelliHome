using Microsoft.Data.SqlClient; // Para SQL
using System;
using System.IO; // Para el manejo de archivos
using System.Threading.Tasks; // Para tareas asíncronas

namespace Proyecto_de_prueba
{
    public partial class HomePage : ContentPage
    {
        // Cadena de conexión correcta
        private readonly string connectionString = "Data Source=172.18.131.231;Initial Catalog=Usuario;User ID=admin;Password=admin;Encrypt=True;TrustServerCertificate=True;Pooling=False;MultiSubnetFailover=True;Trusted_Connection=False;";

        // Variable para almacenar la ruta de la imagen
        private string profileImagePath;

        public HomePage()
        {
            InitializeComponent();
        }

        // Método para manejar la carga de la foto de perfil
        private async void OnUploadPhotoClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecciona una foto",
                FileTypes = FilePickerFileType.Images // Limitar a imágenes
            });

            if (result != null)
            {
                var stream = await result.OpenReadAsync();
                ProfilePicture.Source = ImageSource.FromStream(() => stream);

                // Guarda la imagen en el sistema de archivos
                string fileName = Path.GetFileName(result.FullPath);
                profileImagePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                using (var fileStream = new FileStream(profileImagePath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }

        // Método para manejar la acción del botón de guardar
        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            string accountNumber = AccountNumberEntry.Text;
            DateTime birthDate = BirthDatePicker.Date;

            // Llama al método para guardar en la base de datos
            bool isSaved = await SaveUserInfo(accountNumber, birthDate, profileImagePath);

            if (isSaved)
            {
                await DisplayAlert("Éxito", "Información guardada correctamente.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo guardar la información.", "OK");
            }
        }

        private async Task<bool> SaveUserInfo(string numero_cuenta, DateTime fecha_nacimiento, string foto_perfil)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Leer el archivo de imagen y convertirlo a bytes
                    byte[] imageBytes = await File.ReadAllBytesAsync(foto_perfil);

                    // Consulta SQL para insertar la información
                    string query = "INSERT INTO Usuarios (numero_cuenta, fecha_nacimiento, foto_perfil) VALUES (@numero_cuenta, @fecha_nacimiento, @foto_perfil)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@numero_cuenta", numero_cuenta);
                        command.Parameters.AddWithValue("@fecha_nacimiento", fecha_nacimiento);
                        command.Parameters.AddWithValue("@foto_perfil", imageBytes); // Guardar la imagen como bytes

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0; // Devuelve true si se insertaron filas
                    }
                }
            }
            catch (SqlException ex)
            {
                await DisplayAlert("Error SQL", $"Error al guardar la información: {ex.Message}", "OK");
                return false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
                return false;
            }
        }

    }
}













