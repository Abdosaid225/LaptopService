using LaptopService.Data;
using LaptopService.Models;

namespace LaptopService.Services;

public class TicketService
{
    private readonly ITicketRepository _repository;

    // The repository is "injected" from outside (dependency injection).
    public TicketService(ITicketRepository repository)
    {
        _repository = repository;
    }

    public RepairTicket CreateTicket(string customerName, string phone, string laptopType, string problem)
    {
        var customer = new Customer(customerName, phone);
        var ticket = RepairTicket.Create(_repository.NextId(), customer, laptopType, problem);

        _repository.Add(ticket);
        _repository.Save();
        return ticket;
    }

    public IEnumerable<RepairTicket> GetAll() =>
        _repository.GetAll().OrderByDescending(t => t.CreatedAt);

    public RepairTicket? GetById(int id) => _repository.GetById(id);

    public IEnumerable<RepairTicket> Search(string text) =>
        _repository.GetAll().Where(t =>
            t.Customer.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
            t.Customer.Phone.Contains(text) ||
            t.LaptopType.Contains(text, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<RepairTicket> FilterByStatus(TicketStatus status) =>
        _repository.GetAll().Where(t => t.Status == status);

    public bool UpdateStatus(int id, TicketStatus newStatus)
    {
        RepairTicket? ticket = _repository.GetById(id);
        if (ticket is null)
            return false;

        ticket.ChangeStatus(newStatus);
        _repository.Save();
        return true;
    }

    public bool Delete(int id)
    {
        bool removed = _repository.Remove(id);
        if (removed)
            _repository.Save();
        return removed;
    }

    public Dictionary<TicketStatus, int> GetStatistics()
    {
        var tickets = _repository.GetAll();
        return Enum.GetValues<TicketStatus>()
                   .ToDictionary(s => s, s => tickets.Count(t => t.Status == s));
    }
}
