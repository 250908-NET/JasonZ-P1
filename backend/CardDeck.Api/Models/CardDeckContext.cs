using System;
using System.Collections.Generic;
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost,1434;Database=CardDeckDB;User Id=sa;Password=LetMeIn1@AAAAAAAAA;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
