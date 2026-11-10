using System.Text.Json;
using System.Text.Json.Serialization;
using LaptopService.Models;

namespace LaptopService.Data;

public class JsonTicketRepository : ITicketRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _filePath;
    private readonly List<RepairTicket> _tickets;

    public JsonTicketRepository(string filePath)
    {
        _filePath = filePath;
        _tickets = Load();
    }

    public IReadOnlyList<RepairTicket> GetAll() => _tickets.AsReadOnly();

    public RepairTicket? GetById(int id) => _tickets.FirstOrDefault(t => t.Id == id);

    public int NextId() => _tickets.Count == 0 ? 1 : _tickets.Max(t => t.Id) + 1;

    public void Add(RepairTicket ticket) => _tickets.Add(ticket);

    public bool Remove(int id) => _tickets.RemoveAll(t => t.Id == id) > 0;

    public void Save()
    {
        string json = JsonSerializer.Serialize(_tickets, Options);
        File.WriteAllText(_filePath, json);
    }

    private List<RepairTicket> Load()
    {
        if (!File.Exists(_filePath))
            return new List<RepairTicket>();

        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<RepairTicket>>(json, Options)
                   ?? new List<RepairTicket>();
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException)
        {
            // Keep a copy of the damaged file so no data is lost.
            File.Copy(_filePath, _filePath + ".bak", overwrite: true);
            Console.WriteLine("Warning: the data file is damaged. A backup was saved as .bak and a new list was started.");
            return new List<RepairTicket>();
        }
    }
}
