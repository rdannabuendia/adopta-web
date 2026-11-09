using System;
using System.Collections.Generic;
using Adopta.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Api.Data;

public partial class AdoptaDbContext : DbContext
{
    public AdoptaDbContext(DbContextOptions<AdoptaDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
