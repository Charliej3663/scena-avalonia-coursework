namespace ScenaAvalonia.Models;

public sealed class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public List<Enrollment> Enrollments { get; set; } = [];
}

public sealed class Enrollment
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int CourseId { get; set; }
    public int GroupNumber { get; set; }
    public int Quantity { get; set; }
    public int Total { get; set; }
    public string PaymentMethod { get; set; } = "Картой в студии";
    public string Status { get; set; } = "Новый";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public sealed class SupportMessage
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "Гость";
    public string Text { get; set; } = "";
    public string Reply { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public sealed class StudioReview
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Text { get; set; } = "";
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
