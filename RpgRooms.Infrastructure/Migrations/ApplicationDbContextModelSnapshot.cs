using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RpgRooms.Infrastructure;

#nullable disable

namespace RpgRooms.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("RpgRooms.Core.Entities.User", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<bool>("IsGameMaster").HasColumnType("INTEGER");
                b.Property<string>("PasswordHash").IsRequired().HasColumnType("TEXT");
                b.Property<string>("UserName").IsRequired().HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Users");
            });

            modelBuilder.Entity("RpgRooms.Core.Entities.Campaign", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                b.Property<int>("GameMasterId").HasColumnType("INTEGER");
                b.Property<bool>("IsFinished").HasColumnType("INTEGER");
                b.Property<string>("Name").IsRequired().HasColumnType("TEXT");
                b.HasKey("Id");
                b.HasIndex("GameMasterId");
                b.ToTable("Campaigns");
            });

            modelBuilder.Entity("CampaignPlayers", b =>
            {
                b.Property<int>("CampaignsId").HasColumnType("INTEGER");
                b.Property<int>("PlayersId").HasColumnType("INTEGER");
                b.HasKey("CampaignsId", "PlayersId");
                b.HasIndex("PlayersId");
                b.ToTable("CampaignPlayers");
            });

            modelBuilder.Entity("RpgRooms.Core.Entities.Campaign", b =>
            {
                b.HasOne("RpgRooms.Core.Entities.User", "GameMaster")
                    .WithMany()
                    .HasForeignKey("GameMasterId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("GameMaster");
            });

            modelBuilder.Entity("CampaignPlayers", b =>
            {
                b.HasOne("RpgRooms.Core.Entities.Campaign", null)
                    .WithMany("Players")
                    .HasForeignKey("CampaignsId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("RpgRooms.Core.Entities.User", null)
                    .WithMany("Campaigns")
                    .HasForeignKey("PlayersId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });
        }
    }
}

