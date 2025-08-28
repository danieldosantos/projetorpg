using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using RpgRooms.Core.Entities;

#nullable disable

namespace RpgRooms.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity<Campaign>(b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd();
                b.Property<string>("Name").IsRequired();
                b.Property<string>("Description").IsRequired();
                b.Property<string>("OwnerUserId").IsRequired();
                b.Property<int>("Status");
                b.Property<bool>("IsRecruiting");
                b.Property<int>("MaxPlayers");
                b.Property<DateTime>("CreatedAt");
                b.Property<DateTime?>("UpdatedAt");
                b.Property<DateTime?>("FinalizedAt");
                b.HasKey("Id");
                b.HasIndex("OwnerUserId");
                b.ToTable("Campaigns");
            });

            modelBuilder.Entity<CampaignMember>(b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd();
                b.Property<int>("CampaignId");
                b.Property<string>("UserId").IsRequired();
                b.Property<DateTime>("JoinedAt");
                b.HasKey("Id");
                b.HasIndex("CampaignId");
                b.HasIndex("UserId");
                b.ToTable("CampaignMembers");
            });

            modelBuilder.Entity<JoinRequest>(b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd();
                b.Property<int>("CampaignId");
                b.Property<string>("UserId").IsRequired();
                b.Property<string>("Message").IsRequired();
                b.Property<int>("Status");
                b.Property<DateTime>("CreatedAt");
                b.Property<DateTime?>("RespondedAt");
                b.HasKey("Id");
                b.HasIndex("CampaignId");
                b.HasIndex("UserId");
                b.ToTable("JoinRequests");
            });

            modelBuilder.Entity<ChatMessage>(b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd();
                b.Property<int>("CampaignId");
                b.Property<string>("UserId").IsRequired();
                b.Property<string>("DisplayName").IsRequired();
                b.Property<string>("Message").IsRequired();
                b.Property<DateTime>("SentAt");
                b.HasKey("Id");
                b.HasIndex("CampaignId");
                b.HasIndex("UserId");
                b.ToTable("ChatMessages");
            });

            modelBuilder.Entity<AuditEntry>(b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd();
                b.Property<int>("CampaignId");
                b.Property<string>("UserId").IsRequired();
                b.Property<string>("Action").IsRequired();
                b.Property<DateTime>("CreatedAt");
                b.HasKey("Id");
                b.HasIndex("CampaignId");
                b.HasIndex("UserId");
                b.ToTable("AuditEntries");
            });
        }
    }
}
