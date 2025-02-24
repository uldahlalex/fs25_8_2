﻿using System;
using System.Collections.Generic;
using EFScaffold.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace EFScaffold;

public partial class KahootContext : DbContext
{
    public KahootContext(DbContextOptions<KahootContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Playeranswer> Playeranswers { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Questionoption> Questionoptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("game_pkey");

            entity.ToTable("game", "kahoot");

            entity.Property(e => e.Id).HasColumnName("id");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("player_pkey");

            entity.ToTable("player", "kahoot");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Gameid).HasColumnName("gameid");
            entity.Property(e => e.Nickname).HasColumnName("nickname");

            entity.HasOne(d => d.Game).WithMany(p => p.Players)
                .HasForeignKey(d => d.Gameid)
                .HasConstraintName("player_gameid_fkey");
        });

        modelBuilder.Entity<Playeranswer>(entity =>
        {
            entity.HasKey(e => new { e.Playerid, e.Questionid }).HasName("playeranswer_pkey");

            entity.ToTable("playeranswer", "kahoot");

            entity.Property(e => e.Playerid).HasColumnName("playerid");
            entity.Property(e => e.Questionid).HasColumnName("questionid");
            entity.Property(e => e.Answertimestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("answertimestamp");
            entity.Property(e => e.Optionid).HasColumnName("optionid");

            entity.HasOne(d => d.Option).WithMany(p => p.Playeranswers)
                .HasForeignKey(d => d.Optionid)
                .HasConstraintName("playeranswer_optionid_fkey");

            entity.HasOne(d => d.Player).WithMany(p => p.Playeranswers)
                .HasForeignKey(d => d.Playerid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("playeranswer_playerid_fkey");

            entity.HasOne(d => d.Question).WithMany(p => p.Playeranswers)
                .HasForeignKey(d => d.Questionid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("playeranswer_questionid_fkey");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("question_pkey");

            entity.ToTable("question", "kahoot");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Gameid).HasColumnName("gameid");
            entity.Property(e => e.Questiontext).HasColumnName("questiontext");

            entity.HasOne(d => d.Game).WithMany(p => p.Questions)
                .HasForeignKey(d => d.Gameid)
                .HasConstraintName("question_gameid_fkey");
        });

        modelBuilder.Entity<Questionoption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("questionoption_pkey");

            entity.ToTable("questionoption", "kahoot");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Iscorrect).HasColumnName("iscorrect");
            entity.Property(e => e.Optiontext).HasColumnName("optiontext");
            entity.Property(e => e.Questionid).HasColumnName("questionid");

            entity.HasOne(d => d.Question).WithMany(p => p.Questionoptions)
                .HasForeignKey(d => d.Questionid)
                .HasConstraintName("questionoption_questionid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
