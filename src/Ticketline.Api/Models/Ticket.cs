namespace Ticketline.Api.Models;

public class Ticket
{
    public int Id { get; set; }

    public int OwnerId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string Status { get; set; } = TicketStatus.Open;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User Owner { get; set; } = null!;
}

public static class TicketStatus
{
    public const string Open = "Open";
    public const string InProgress = "InProgress";
    public const string Closed = "Closed";

    public static readonly string[] All = [Open, InProgress, Closed];

    public static void EnsureTransition(string from, string to)
    {
        if (!All.Contains(to))
        {
            throw new ArgumentException("Неизвестный статус заявки.");
        }

        if (from == Closed && to != Closed)
        {
            throw new InvalidOperationException("Закрытую заявку нельзя вернуть в работу.");
        }
    }
}
