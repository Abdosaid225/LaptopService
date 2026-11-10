using System.Text.Json.Serialization;

namespace LaptopService.Models;

public class RepairTicket
{
    public int Id { get; }
    public Customer Customer { get; }
    public string LaptopType { get; }
    public string Problem { get; }
    public DateTime CreatedAt { get; }
    public TicketStatus Status { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    // Used by the JSON loader to rebuild saved tickets.
    [JsonConstructor]
    public RepairTicket(int id, Customer customer, string laptopType, string problem,
                        DateTime createdAt, TicketStatus status, DateTime? closedAt)
    {
        if (string.IsNullOrWhiteSpace(laptopType))
            throw new ArgumentException("Laptop type is required.", nameof(laptopType));
        if (string.IsNullOrWhiteSpace(problem))
            throw new ArgumentException("Problem description is required.", nameof(problem));

        Id = id;
        Customer = customer;
        LaptopType = laptopType.Trim();
        Problem = problem.Trim();
        CreatedAt = createdAt;
        Status = status;
        ClosedAt = closedAt;
    }

    // Used when a brand new ticket is created: date/time are recorded automatically.
    public static RepairTicket Create(int id, Customer customer, string laptopType, string problem)
    {
        return new RepairTicket(id, customer, laptopType, problem,
                                DateTime.Now, TicketStatus.Open, null);
    }

    // Business rule lives inside the class: a delivered ticket is locked.
    public void ChangeStatus(TicketStatus newStatus)
    {
        if (Status == TicketStatus.Delivered)
            throw new InvalidOperationException("A delivered ticket can no longer be changed.");

        Status = newStatus;

        if (newStatus == TicketStatus.Done || newStatus == TicketStatus.Delivered)
        {
            if (ClosedAt is null)
                ClosedAt = DateTime.Now;
        }
        else
        {
            ClosedAt = null;
        }
    }
}
