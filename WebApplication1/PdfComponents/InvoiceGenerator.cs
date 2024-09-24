using Bogus;

namespace PdfFromRazor.Invoices;

public class InvoiceGenerator
{
   public static Invoice Generate()
   {
      var faker = new Faker();

      return new Invoice
      {
         Number = faker.Random.Number(1000, 9999).ToString(),
         IssueDate = faker.Date.SoonDateOnly(0),
         DueDate = faker.Date.SoonDateOnly(0).AddMonths(1),
         SellerAddress = new Address
         {
            CompanyName = "Dometrain",
            City = "London",
            Email = "nick@dometrain.com",
            Phone = faker.Phone.PhoneNumber(),
            Street = "Super London",
            State = "LD",
            ZipCode = "69 420"
         },
         CustomerAddress = new Address
         {
            CompanyName = faker.Company.CompanyName(),
            City = faker.Address.City(),
            Email = faker.Person.Email,
            Phone = faker.Phone.PhoneNumber(),
            Street = faker.Address.StreetAddress(),
            State = faker.Address.State(),
            ZipCode = faker.Address.ZipCode()
         },
         Items = new List<OrderItem>
         {
            new()
            {
               Name = "Getting Started: Boosting Developer Productivity with AI",
               Price = 69.99m,
               Quantity = 1
            },
            new()
            {
               Name = "From Zero to Hero: Configuration and Options in .NET",
               Price = 69.99m,
               Quantity = 1
            },
            new()
            {
               Name = "From Zero to Hero: Reflection in .NET",
               Price = 69.99m,
               Quantity = 1
            }
         }
      };
   }
}
