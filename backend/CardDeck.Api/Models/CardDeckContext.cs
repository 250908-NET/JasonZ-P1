using System;
using System.Collections.Generic;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

namespace CardDeck.Api.Models;

public partial class CardDeckContext : DbContext
{
    public CardDeckContext()
    {
    }

    public CardDeckContext(DbContextOptions<CardDeckContext> options)
        : base(options)
    {
    }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     => optionsBuilder.UseSqlServer(_connectionString);
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
