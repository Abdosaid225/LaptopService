using System.Text;
using LaptopService.Data;
using LaptopService.Services;
using LaptopService.UI;

namespace LaptopService;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        // Wire everything together in one place (manual dependency injection).
        string filePath = Path.Combine(Environment.CurrentDirectory, "tickets.json");
        ITicketRepository repository = new JsonTicketRepository(filePath);
        var service = new TicketService(repository);
        var ui = new ConsoleUI(service);

        ui.Run();
    }
}