using Microsoft.EntityFrameworkCore;

namespace Stilbaai_Tourism_Web_Portal.Context
{
   public class ApplicationDbContext : DbContext
   {
      public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
      {

      }
   }
}
