using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

public class MsslqDbContext : DbContext
{

    public MsslqDbContext(DbContextOptions<MsslqDbContext> options)
        : base(options)
    {
    }

    // Define DbSets for your entities
    public DbSet<CstCase> CstCases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CstCaseConfiguration());

        // Configure your model with Fluent API if needed
    }
}