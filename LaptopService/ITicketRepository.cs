using LaptopService.Models;

namespace LaptopService.Data;

// The service only knows this interface, not where the data is really stored.
public interface ITicketRepository
{
    IReadOnlyList<RepairTicket> GetAll();
    RepairTicket? GetById(int id);
    int NextId();
    void Add(RepairTicket ticket);
    bool Remove(int id);
    void Save();
}
