namespace PdfFromRazor.Invoices;

public class Invoice
{
    public string Number { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }

    public Address SellerAddress { get; set; }
    public Address CustomerAddress { get; set; }

    public List<OrderItem> Items { get; set; }
    public string Comments { get; set; }
}

public class OrderItem
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class Address
{
    public string CompanyName { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }

    public object Email { get; set; }
    public string Phone { get; set; }
}