using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Program
{
    private static readonly string apiKey = "abf3fcd8418efce10f1e3e433f91c951";
    private static readonly string hash = "da846e9b969242a8a37d574812963605";
    private static readonly string ts = "1";
    private static readonly string baseUrl = "https://gateway.marvel.com/v1/public/characters";

    static async Task Main(string[] args)
    {
        // Solicitar al usuario cuántos héroes quiere ver
        Console.Write("¿Cuántos héroes deseas ver? ");
        int cantidadHeroes;
        while (!int.TryParse(Console.ReadLine(), out cantidadHeroes) || cantidadHeroes <= 0)
        {
            Console.Write("Por favor ingresa un número válido mayor a 0: ");
        }

        // Solicitar al usuario un filtro por letra o número inicial
        Console.Write("¿Quieres buscar héroes por una letra o número inicial? (deja vacío para ver todos): ");
        string filtro = Console.ReadLine();

        // Inicializar variables para la paginación
        int heroesMostrados = 0;
        int offset = 0;
        int heroesPorPagina = 20;

        // Usar HttpClient para hacer una solicitud GET
        var client = new HttpClient();

        try
        {
            while (heroesMostrados < cantidadHeroes)
            {
                // Construir la URL de la solicitud con el parámetro nameStartsWith y offset para paginación
                string requestUrl = $"{baseUrl}?apikey={apiKey}&hash={hash}&ts={ts}&offset={offset}";

                if (!string.IsNullOrEmpty(filtro))
                {
                    requestUrl += $"&nameStartsWith={Uri.EscapeDataString(filtro)}";
                }

                // Hacer la solicitud GET y leer la respuesta como cadena
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                // Verificar si la respuesta fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Deserializar directamente la respuesta JSON a la clase MarvelApiResponse
                    var marvelResponse = JsonConvert.DeserializeObject<MarvelApiResponse>(await response.Content.ReadAsStringAsync());

                    // Verificar si hay héroes en la respuesta
                    if (marvelResponse?.Data?.Results != null && marvelResponse.Data.Results.Length > 0)
                    {
                        Console.WriteLine("Héroes Marvel:");

                        // Enumerar y mostrar los héroes seleccionados en la página actual
                        foreach (var hero in marvelResponse.Data.Results)
                        {
                            heroesMostrados++;
                            string description = string.IsNullOrEmpty(hero.Description)
                                ? "Descripción no disponible"
                                : hero.Description;

                            Console.WriteLine($"[{heroesMostrados}] Nombre: {hero.Name}");
                            Console.WriteLine($"    Descripción: {description}");
                            Console.WriteLine();

                            if (heroesMostrados >= cantidadHeroes)
                            {
                                break;
                            }
                        }

                        // Incrementar el offset para la siguiente página
                        offset += heroesPorPagina;
                    }
                    else
                    {
                        Console.WriteLine($"No se encontraron héroes con el filtro '{filtro}'.");
                        break;
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            // Manejar excepciones
            Console.WriteLine($"Excepción: {ex.Message}");
        }

        // Mantener la consola abierta hasta que el usuario presione una tecla
        Console.WriteLine("Presiona cualquier tecla para salir...");
        Console.ReadKey();
    }

    // Clases para deserializar la respuesta JSON
    public class MarvelApiResponse
    {
        public MarvelData Data { get; set; }
    }

    public class MarvelData
    {
        public Hero[] Results { get; set; }
    }

    public class Hero
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
