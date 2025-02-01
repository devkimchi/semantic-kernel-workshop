using System.ComponentModel;
using System.Text.Json;

using Microsoft.SemanticKernel;

namespace Workshop.ConsoleApp.Plugins.BookingAgent;

public class TrainBookingPlugin
{
    private const string Database = "trains.json";

    private static string filepath = Path.Combine(AppContext.BaseDirectory, "../../..", "Plugins", "BookingAgent", Database);
    private static JsonSerializerOptions options = new() { WriteIndented = true };

    private readonly List<TrainModel> _trains;

    public TrainBookingPlugin()
    {
        this._trains = this.LoadTrainsFromFile();
    }

    [KernelFunction("search_trains")]
    [Description("Searches for available trains based on the destination and departure date in the format YYYY-MM-DD")]
    [return: Description("A list of available trains")]
    public List<TrainModel> SearchTrains(string destination, string departureDate)
    {
        return this._trains.Where(train => train.Destination.Equals(destination, StringComparison.InvariantCultureIgnoreCase) &&
                                           train.DepartureDate.Equals(departureDate))
                           .ToList();
    }

    [KernelFunction("book_train")]
    [Description("Books a train based on the train ID provided")]
    [return: Description("Booking confirmation message")]
    public string BookTrain(int trainId)
    {
        var train = this._trains.SingleOrDefault(train => train.Id == trainId);
        if (train == null)
        {
            return "Train not found. Please provide a valid train ID.";
        }

        if (train.IsBooked == true)
        {
            return $"You've already booked this train.";
        }

        train.IsBooked = true;
        this.SaveTrainsToFile();
        
        return $"Train booked successfully! Train name: {train.TrainName}, Destination: {train.Destination}, Departure: {train.DepartureDate}, Price: ${train.Price}.";
    }

    private void SaveTrainsToFile()
    {
        var json = JsonSerializer.Serialize(this._trains, options);
        File.WriteAllText(filepath, json);
    }

    private List<TrainModel> LoadTrainsFromFile()
    {
        if (File.Exists(filepath))
        {
            var json = File.ReadAllText(filepath);
            return JsonSerializer.Deserialize<List<TrainModel>>(json)!;
        }

        throw new FileNotFoundException($"The file '{Database}' was not found. Please provide a valid {Database} file.");
    }
}

public class TrainModel
{
    public int Id { get; set; }
    public required string TrainName { get; set; }
    public required string Destination { get; set; }
    public required string DepartureDate { get; set; }
    public decimal Price { get; set; }
    public bool IsBooked { get; set; } = false;
}
