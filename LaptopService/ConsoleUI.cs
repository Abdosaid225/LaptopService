using System.Text.RegularExpressions;
using LaptopService.Models;
using LaptopService.Services;

namespace LaptopService.UI;

public class ConsoleUI
{
    private const string RowFormat = "{0,-4} {1,-18} {2,-14} {3,-18} {4,-16} {5,-16}";

    private readonly TicketService _service;

    public ConsoleUI(TicketService service)
    {
        _service = service;
    }

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            ShowMenu();
            string choice = ReadText("Your choice: ");
            Console.WriteLine();

            try
            {
                switch (choice)
                {
                    case "1": AddTicket(); break;
                    case "2": PrintTable(_service.GetAll()); break;
                    case "3": SearchTickets(); break;
                    case "4": FilterByStatus(); break;
                    case "5": ShowDetails(); break;
                    case "6": UpdateStatus(); break;
                    case "7": DeleteTicket(); break;
                    case "8": ShowStatistics(); break;
                    case "0": return;
                    default: ShowError("Invalid choice."); break;
                }
            }
            catch (InvalidOperationException ex)
            {
                // Business-rule errors thrown by the model (e.g. editing a delivered ticket).
                ShowError(ex.Message);
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }
    }

    // ---------- Menu actions ----------

    private static void ShowMenu()
    {
        Console.WriteLine("=== Laptop Repair Shop ===");
        Console.WriteLine("1. Add new ticket");
        Console.WriteLine("2. List all tickets");
        Console.WriteLine("3. Search (name / phone / laptop)");
        Console.WriteLine("4. Filter by status");
        Console.WriteLine("5. View ticket details");
        Console.WriteLine("6. Update ticket status");
        Console.WriteLine("7. Delete ticket");
        Console.WriteLine("8. Statistics");
        Console.WriteLine("0. Exit");
        Console.WriteLine();
    }

    private void AddTicket()
    {
        Console.WriteLine("--- New repair ticket ---");
        string name = ReadText("Customer name: ");
        string phone = ReadValidated("Customer phone: ", IsValidPhone,
                                     "Enter 7-15 digits (a leading + is allowed).");
        phone = phone.Replace(" ", "").Replace("-", "");
        string laptop = ReadText("Laptop type: ");
        string problem = ReadText("Problem: ");

        RepairTicket ticket = _service.CreateTicket(name, phone, laptop, problem);
        ShowSuccess($"Ticket #{ticket.Id} created on {ticket.CreatedAt:yyyy-MM-dd} at {ticket.CreatedAt:HH:mm}.");
    }

    private void SearchTickets()
    {
        string text = ReadText("Search text: ");
        PrintTable(_service.Search(text));
    }

    private void FilterByStatus()
    {
        TicketStatus status = ReadStatus();
        Console.WriteLine();
        PrintTable(_service.FilterByStatus(status));
    }

    private void ShowDetails()
    {
        int id = ReadInt("Ticket ID: ");
        RepairTicket? ticket = _service.GetById(id);
        if (ticket is null)
        {
            ShowError("Ticket not found.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Ticket #{ticket.Id}");
        Console.WriteLine($"Customer : {ticket.Customer.Name}");
        Console.WriteLine($"Phone    : {ticket.Customer.Phone}");
        Console.WriteLine($"Laptop   : {ticket.LaptopType}");
        Console.WriteLine($"Problem  : {ticket.Problem}");
        Console.WriteLine($"Status   : {ticket.Status}");
        Console.WriteLine($"Created  : {ticket.CreatedAt:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"Closed   : {(ticket.ClosedAt.HasValue ? ticket.ClosedAt.Value.ToString("yyyy-MM-dd HH:mm") : "-")}");
    }

    private void UpdateStatus()
    {
        int id = ReadInt("Ticket ID: ");
        if (_service.GetById(id) is null)
        {
            ShowError("Ticket not found.");
            return;
        }

        TicketStatus newStatus = ReadStatus();
        _service.UpdateStatus(id, newStatus);
        ShowSuccess($"Ticket #{id} is now {newStatus}.");
    }

    private void DeleteTicket()
    {
        int id = ReadInt("Ticket ID: ");
        if (!Confirm($"Delete ticket #{id}?"))
        {
            Console.WriteLine("Cancelled.");
            return;
        }

        if (_service.Delete(id))
            ShowSuccess("Ticket deleted.");
        else
            ShowError("Ticket not found.");
    }

    private void ShowStatistics()
    {
        Dictionary<TicketStatus, int> stats = _service.GetStatistics();

        Console.WriteLine("--- Statistics ---");
        foreach (var (status, count) in stats)
            Console.WriteLine($"{status,-16} {count}");

        Console.WriteLine($"{"Total",-16} {stats.Values.Sum()}");
    }

    // ---------- Display helpers ----------

    private static void PrintTable(IEnumerable<RepairTicket> tickets)
    {
        List<RepairTicket> list = tickets.ToList();
        if (list.Count == 0)
        {
            Console.WriteLine("No tickets found.");
            return;
        }

        Console.WriteLine(RowFormat, "ID", "Customer", "Phone", "Laptop", "Status", "Created");
        Console.WriteLine(new string('-', 91));

        foreach (RepairTicket t in list)
        {
            Console.WriteLine(RowFormat,
                t.Id,
                Cut(t.Customer.Name, 18),
                t.Customer.Phone,
                Cut(t.LaptopType, 18),
                t.Status,
                t.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
        }

        Console.WriteLine($"\nTotal: {list.Count}");
    }

    private static string Cut(string text, int max) =>
        text.Length <= max ? text : text.Substring(0, max - 3) + "...";

    private static void ShowError(string message) => WriteColored(message, ConsoleColor.Red);

    private static void ShowSuccess(string message) => WriteColored(message, ConsoleColor.Green);

    private static void WriteColored(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    // ---------- Input helpers ----------

    private static string ReadText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = (Console.ReadLine() ?? "").Trim();
            if (input.Length > 0)
                return input;

            ShowError("This field cannot be empty.");
        }
    }

    private static string ReadValidated(string prompt, Func<string, bool> isValid, string errorMessage)
    {
        while (true)
        {
            string input = ReadText(prompt);
            if (isValid(input))
                return input;

            ShowError(errorMessage);
        }
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            string input = ReadText(prompt);
            if (int.TryParse(input, out int value))
                return value;

            ShowError("Please enter a valid number.");
        }
    }

    private static TicketStatus ReadStatus()
    {
        TicketStatus[] values = Enum.GetValues<TicketStatus>();
        for (int i = 0; i < values.Length; i++)
            Console.WriteLine($"  {i + 1}. {values[i]}");

        while (true)
        {
            int number = ReadInt("Status number: ");
            if (number >= 1 && number <= values.Length)
                return values[number - 1];

            ShowError("Number out of range.");
        }
    }

    private static bool Confirm(string question)
    {
        string answer = ReadText($"{question} (y/n): ");
        return answer.Equals("y", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidPhone(string input)
    {
        string cleaned = input.Replace(" ", "").Replace("-", "");
        return Regex.IsMatch(cleaned, @"^\+?[0-9]{7,15}$");
    }
}
