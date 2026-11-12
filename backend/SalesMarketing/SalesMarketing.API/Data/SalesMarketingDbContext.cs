using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SalesMarketing.API.Models;

namespace SalesMarketing.API.Data;

public partial class SalesMarketingDbContext : DbContext
{
    public SalesMarketingDbContext(DbContextOptions<SalesMarketingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Marketingteam> Marketingteams { get; set; }

    public virtual DbSet<Marketingteamdetail> Marketingteamdetails { get; set; }

    public virtual DbSet<Salesman> Salesmen { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Marketingteam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marketingteam_pkey");

            entity.ToTable("marketingteam", table =>
                table.HasCheckConstraint(
                    "ck_marketingteam_status",
                    "status IN (1, 2, 3)"
                ));

            entity.HasIndex(e => e.Teamcode, "uq_marketingteam_code").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("now()")
                .HasColumnName("createddate");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Modifieddate).HasColumnName("modifieddate");
            entity.Property(e => e.Status)
                .HasDefaultValue((short)1)
                .HasColumnName("status");
            entity.Property(e => e.Teamcode)
                .HasMaxLength(20)
                .HasColumnName("teamcode");
            entity.Property(e => e.Teamleaderid).HasColumnName("teamleaderid");
            entity.Property(e => e.Teamname)
                .HasMaxLength(100)
                .HasColumnName("teamname");

            entity.HasOne(d => d.Teamleader).WithMany(p => p.Marketingteams)
                .HasForeignKey(d => d.Teamleaderid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_marketingteam_leader");
        });

        modelBuilder.Entity<Marketingteamdetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("marketingteamdetail_pkey");

            entity.ToTable("marketingteamdetail", table =>
            {
                table.HasCheckConstraint("ck_mtdetail_role", "role IN (1, 2, 3, 4)");
                table.HasCheckConstraint("ck_mtdetail_target", "targetamount >= 0");
                table.HasCheckConstraint("ck_mtdetail_status", "status IN (1, 2)");
            });

            entity.HasIndex(e => e.Salesmanid, "ix_mtdetail_salesmanid");

            entity.HasIndex(e => e.Marketingteamid, "ix_mtdetail_teamid");

            entity.HasIndex(e => new { e.Marketingteamid, e.Salesmanid }, "uq_mtdetail_team_salesman").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Marketingteamid).HasColumnName("marketingteamid");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Salesmanid).HasColumnName("salesmanid");
            entity.Property(e => e.Status)
                .HasDefaultValue((short)1)
                .HasColumnName("status");
            entity.Property(e => e.Targetamount)
                .HasPrecision(18, 2)
                .HasColumnName("targetamount");

            entity.HasOne(d => d.Marketingteam).WithMany(p => p.Marketingteamdetails)
                .HasForeignKey(d => d.Marketingteamid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_mtdetail_team");

            entity.HasOne(d => d.Salesman).WithMany(p => p.Marketingteamdetails)
                .HasForeignKey(d => d.Salesmanid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_mtdetail_salesman");
        });

        modelBuilder.Entity<Salesman>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("salesman_pkey");

            entity.ToTable("salesman", table => table.HasCheckConstraint("ck_salesman_status", "status IN (1, 2)"));

            entity.HasIndex(e => e.Salesmancode, "uq_salesman_code").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("now()")
                .HasColumnName("createddate");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Joiningdate).HasColumnName("joiningdate");
            entity.Property(e => e.Mobileno)
                .HasMaxLength(20)
                .HasColumnName("mobileno");
            entity.Property(e => e.Modifieddate).HasColumnName("modifieddate");
            entity.Property(e => e.Salesmancode)
                .HasMaxLength(20)
                .HasColumnName("salesmancode");
            entity.Property(e => e.Salesmanname)
                .HasMaxLength(100)
                .HasColumnName("salesmanname");
            entity.Property(e => e.Status)
                .HasDefaultValue((short)1)
                .HasColumnName("status");
        });



        



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
