﻿// <auto-generated />
using System;
using System.Numerics;
using MM_API;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SharedGameFramework.Game.Armoury;
using SharedGameFramework.Game.Character;
using SharedGameFramework.Game.Kingdom.Map;

#nullable disable

namespace MM_API.Migrations
{
    [DbContext(typeof(MM_DbContext))]
    [Migration("20240825081208_Initial2")]
    partial class Initial2
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Armoury", b =>
                {
                    b.Property<int>("armoury_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("armoury_id"));

                    b.Property<Armoury_Inventory>("armoury_inventory")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("fk_kingdom_id")
                        .HasColumnType("integer");

                    b.HasKey("armoury_id");

                    b.HasIndex("fk_kingdom_id")
                        .IsUnique();

                    b.ToTable("t_armoury");
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Character", b =>
                {
                    b.Property<int>("character_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("character_id"));

                    b.Property<Character_Inventory>("character_inventory")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("character_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<CharacterSheet>("character_sheet")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<CharacterState>("character_state")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("fk_kingdom_id")
                        .HasColumnType("integer");

                    b.Property<int>("political_points")
                        .HasColumnType("integer");

                    b.HasKey("character_id");

                    b.HasIndex("fk_kingdom_id");

                    b.ToTable("t_character");
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Kingdom", b =>
                {
                    b.Property<int>("kingdom_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("kingdom_id"));

                    b.Property<int>("fk_user_id")
                        .HasColumnType("integer");

                    b.Property<Map>("kingdom_map")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("kingdom_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("kingdom_id");

                    b.HasIndex("fk_user_id")
                        .IsUnique();

                    b.ToTable("t_kingdom");
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Session", b =>
                {
                    b.Property<int>("session_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("session_id"));

                    b.Property<int>("fk_user_id")
                        .HasColumnType("integer");

                    b.Property<string>("session_authtoken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("session_loggedin")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("session_loggedout")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("session_refreshtoken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("session_id");

                    b.HasIndex("fk_user_id");

                    b.ToTable("t_session");
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Soupkitchen", b =>
                {
                    b.Property<int>("soupkitchen_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("soupkitchen_id"));

                    b.Property<int>("fk_character_id")
                        .HasColumnType("integer");

                    b.Property<int>("soupkitchen_cooldown_days")
                        .HasColumnType("integer");

                    b.Property<int>("soupkitchen_cooldown_hours")
                        .HasColumnType("integer");

                    b.Property<int>("soupkitchen_cooldown_minutes")
                        .HasColumnType("integer");

                    b.Property<int>("soupkitchen_cooldown_seconds")
                        .HasColumnType("integer");

                    b.HasKey("soupkitchen_id");

                    b.HasIndex("fk_character_id")
                        .IsUnique();

                    b.ToTable("t_soupkitchen");
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Treasury", b =>
                {
                    b.Property<int>("treasury_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("treasury_id"));

                    b.Property<int>("fk_kingdom_id")
                        .HasColumnType("integer");

                    b.Property<BigInteger>("treasury_coin")
                        .HasColumnType("numeric");

                    b.Property<double>("treasury_gainrate")
                        .HasColumnType("double precision");

                    b.Property<double>("treasury_multiplier")
                        .HasColumnType("double precision");

                    b.HasKey("treasury_id");

                    b.HasIndex("fk_kingdom_id")
                        .IsUnique();

                    b.ToTable("t_treasury");
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_User", b =>
                {
                    b.Property<int>("user_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("user_id"));

                    b.Property<string>("user_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("user_id");

                    b.ToTable("t_user");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Armoury", b =>
                {
                    b.HasOne("Database.Postgres.DbSchema.t_Kingdom", "kingdom")
                        .WithOne("armoury")
                        .HasForeignKey("Database.Postgres.DbSchema.t_Armoury", "fk_kingdom_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("kingdom");
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Character", b =>
                {
                    b.HasOne("Database.Postgres.DbSchema.t_Kingdom", "kingdom")
                        .WithMany("characters")
                        .HasForeignKey("fk_kingdom_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("kingdom");
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Kingdom", b =>
                {
                    b.HasOne("Database.Postgres.DbSchema.t_User", "user")
                        .WithOne("kingdom")
                        .HasForeignKey("Database.Postgres.DbSchema.t_Kingdom", "fk_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("user");
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Session", b =>
                {
                    b.HasOne("Database.Postgres.DbSchema.t_User", "user")
                        .WithMany("sessions")
                        .HasForeignKey("fk_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("user");
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Soupkitchen", b =>
                {
                    b.HasOne("Database.Postgres.DbSchema.t_Character", "character")
                        .WithOne("soupkitchen")
                        .HasForeignKey("Database.Postgres.DbSchema.t_Soupkitchen", "fk_character_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("character");
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Treasury", b =>
                {
                    b.HasOne("Database.Postgres.DbSchema.t_Kingdom", "kingdom")
                        .WithOne("treasury")
                        .HasForeignKey("Database.Postgres.DbSchema.t_Treasury", "fk_kingdom_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("kingdom");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Character", b =>
                {
                    b.Navigation("soupkitchen")
                        .IsRequired();
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_Kingdom", b =>
                {
                    b.Navigation("armoury")
                        .IsRequired();

                    b.Navigation("characters");

                    b.Navigation("treasury")
                        .IsRequired();
                });

            modelBuilder.Entity("Database.Postgres.DbSchema.t_User", b =>
                {
                    b.Navigation("kingdom")
                        .IsRequired();

                    b.Navigation("sessions");
                });
#pragma warning restore 612, 618
        }
    }
}
