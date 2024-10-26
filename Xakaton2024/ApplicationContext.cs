using Microsoft.EntityFrameworkCore;
using Xakaton2024.Controllers;

namespace WAYMORR_MS_Product;
public class ApplicationContext: DbContext
{
    public DbSet<Person> Users { get; set; } = null!;
    
    public ApplicationContext(DbContextOptions<ApplicationContext> options): base(options)
    {
        
    }
}