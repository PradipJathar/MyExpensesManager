using System;
using Microsoft.EntityFrameworkCore;
using ExpenseManager.API.Data;

namespace ExpenseManager.Tests
{
    public static class TestDatabaseFixture
    {
        public static AppDbContext CreateContext()
        {
            // Use Guid to ensure each test case gets a fresh, clean database instance
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
