﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TennisClubRanking.Data;

#nullable disable

namespace TennisClubRanking.Migrations
{
    [DbContext(typeof(TennisContext))]
    [Migration("20250108141802_UpdateMatchModel")]
    partial class UpdateMatchModel
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("TennisClubRanking.Models.Match", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AwayPlayerId")
                        .HasColumnType("int");

                    b.Property<int>("Court")
                        .HasColumnType("int");

                    b.Property<int>("HomePlayerId")
                        .HasColumnType("int");

                    b.Property<int>("MatchType")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<int?>("Player3Id")
                        .HasColumnType("int");

                    b.Property<int?>("Player4Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("ScheduledDateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Score")
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int?>("TournamentId")
                        .HasColumnType("int");

                    b.Property<bool>("ValidForRanking")
                        .HasColumnType("tinyint(1)");

                    b.Property<int?>("WinnerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AwayPlayerId");

                    b.HasIndex("HomePlayerId");

                    b.HasIndex("Player3Id");

                    b.HasIndex("Player4Id");

                    b.HasIndex("TournamentId");

                    b.HasIndex("WinnerId");

                    b.ToTable("Matches");
                });

            modelBuilder.Entity("TennisClubRanking.Models.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("MatchesLost")
                        .HasColumnType("int");

                    b.Property<int>("MatchesWon")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Phone")
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<string>("PhotoUrl")
                        .HasColumnType("longtext");

                    b.Property<int>("Points")
                        .HasColumnType("int");

                    b.Property<int>("Ranking")
                        .HasColumnType("int");

                    b.Property<DateTime>("RegistrationDate")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("TennisClubRanking.Models.Tournament", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int?>("WinnerId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("WinnerId");

                    b.ToTable("Tournaments");
                });

            modelBuilder.Entity("TennisClubRanking.Models.TournamentPlayer", b =>
                {
                    b.Property<int>("TournamentId")
                        .HasColumnType("int");

                    b.Property<int>("PlayerId")
                        .HasColumnType("int");

                    b.Property<DateTime>("RegistrationDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int?>("Seed")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("TournamentId", "PlayerId");

                    b.HasIndex("PlayerId");

                    b.ToTable("TournamentPlayers");
                });

            modelBuilder.Entity("TennisClubRanking.Models.Match", b =>
                {
                    b.HasOne("TennisClubRanking.Models.Player", "AwayPlayer")
                        .WithMany("AwayMatches")
                        .HasForeignKey("AwayPlayerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TennisClubRanking.Models.Player", "HomePlayer")
                        .WithMany("HomeMatches")
                        .HasForeignKey("HomePlayerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TennisClubRanking.Models.Player", "Player3")
                        .WithMany()
                        .HasForeignKey("Player3Id")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("TennisClubRanking.Models.Player", "Player4")
                        .WithMany()
                        .HasForeignKey("Player4Id")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("TennisClubRanking.Models.Tournament", "Tournament")
                        .WithMany("Matches")
                        .HasForeignKey("TournamentId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("TennisClubRanking.Models.Player", "Winner")
                        .WithMany()
                        .HasForeignKey("WinnerId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("AwayPlayer");

                    b.Navigation("HomePlayer");

                    b.Navigation("Player3");

                    b.Navigation("Player4");

                    b.Navigation("Tournament");

                    b.Navigation("Winner");
                });

            modelBuilder.Entity("TennisClubRanking.Models.Tournament", b =>
                {
                    b.HasOne("TennisClubRanking.Models.Player", "Winner")
                        .WithMany()
                        .HasForeignKey("WinnerId");

                    b.Navigation("Winner");
                });

            modelBuilder.Entity("TennisClubRanking.Models.TournamentPlayer", b =>
                {
                    b.HasOne("TennisClubRanking.Models.Player", "Player")
                        .WithMany("TournamentPlayers")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TennisClubRanking.Models.Tournament", "Tournament")
                        .WithMany("TournamentPlayers")
                        .HasForeignKey("TournamentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");

                    b.Navigation("Tournament");
                });

            modelBuilder.Entity("TennisClubRanking.Models.Player", b =>
                {
                    b.Navigation("AwayMatches");

                    b.Navigation("HomeMatches");

                    b.Navigation("TournamentPlayers");
                });

            modelBuilder.Entity("TennisClubRanking.Models.Tournament", b =>
                {
                    b.Navigation("Matches");

                    b.Navigation("TournamentPlayers");
                });
#pragma warning restore 612, 618
        }
    }
}
