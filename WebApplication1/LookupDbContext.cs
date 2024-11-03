using Microsoft.EntityFrameworkCore;

namespace WebApplication1;

public class LookupDbContext : DbContext
{
    public LookupDbContext(DbContextOptions<LookupDbContext> options) : base(options)
    {
    }
}