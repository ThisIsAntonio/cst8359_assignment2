namespace Assignment2.Data
{
    public class DbInitializer
    {
        public static void Initialize(SportsDbContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}
