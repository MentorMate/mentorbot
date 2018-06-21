// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using MentorBot.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MentorBot.Functions.App.DbConfigurations
{
    /// <summary>
    /// Factory that initializes database connection string.
    /// </summary>
    public class DbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        /// <summary>
        /// Creates new instance of <see cref="ApplicationDbContext"/>
        /// </summary>
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // var configuration = new ConfigurationBuilder()
            //     .SetBasePath(Directory.GetCurrentDirectory())
            //     .AddJsonFile("local.settings.json")
            //     .Build()
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            builder.UseSqlServer("Server=localhost;Database=MentorBot;Trusted_Connection=True;MultipleActiveResultSets=True;");

            return new ApplicationDbContext(builder.Options);
        }
    }
}
