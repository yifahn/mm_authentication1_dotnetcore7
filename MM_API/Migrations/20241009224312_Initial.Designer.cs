﻿// <auto-generated />
using System;
using System.Numerics;
using MM_API;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MM_API.Migrations
{
    [DbContext(typeof(MM_DbContext))]
    [Migration("20241009224312_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MM_API.Database.Postgres.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<int>("CustomUserId")
                        .HasColumnType("integer");

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

                    b.HasIndex("CustomUserId");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "5de48f8c-0a70-4e21-9cc0-798ff818fdc3",
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "81c97c6d-f7ef-4497-9806-b7e87a90c077",
                            CustomUserId = -999,
                            Email = "yifahnadmin@gmail.com",
                            EmailConfirmed = false,
                            LockoutEnabled = false,
                            NormalizedEmail = "YIFAHNADMIN@GMAIL.COM",
                            NormalizedUserName = "YIFAHNADMIN",
                            PasswordHash = "AQAAAAIAAYagAAAAEFR0S3EOLx7v+nu0WTMG5IHsdazRWXOIf95kKSKWioBhK6HLN4H/wy8Zn3s1je1fHQ==",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "fc0c4971-8a12-4c2b-8f79-6fc86d2aced2",
                            TwoFactorEnabled = false,
                            UserName = "yifahnadmin"
                        });
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_Armoury", b =>
                {
                    b.Property<int>("armoury_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("armoury_id"));

                    b.Property<string>("armoury_armour")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("armoury_jewellery")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("armoury_weapons")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("fk_user_id")
                        .HasColumnType("integer");

                    b.HasKey("armoury_id");

                    b.HasIndex("fk_user_id");

                    b.ToTable("t_armoury");
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_Character", b =>
                {
                    b.Property<int>("character_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("character_id"));

                    b.Property<string>("character_armour")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("character_attributes")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("character_jewellery")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("character_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("character_state")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("character_weapons")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<int>("fk_user_id")
                        .HasColumnType("integer");

                    b.HasKey("character_id");

                    b.HasIndex("fk_user_id");

                    b.ToTable("t_character");
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_Kingdom", b =>
                {
                    b.Property<int>("kingdom_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("kingdom_id"));

                    b.Property<int>("fk_user_id")
                        .HasColumnType("integer");

                    b.Property<string>("kingdom_map")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("kingdom_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("kingdom_id");

                    b.HasIndex("fk_user_id");

                    b.ToTable("t_kingdom");
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_Session", b =>
                {
                    b.Property<int>("session_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("session_id"));

                    b.Property<int>("fk_user_id")
                        .HasColumnType("integer");

                    b.Property<string>("refreshtoken")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset>("session_loggedin")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("session_loggedout")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("session_id");

                    b.HasIndex("fk_user_id");

                    b.ToTable("t_session");
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_Soupkitchen", b =>
                {
                    b.Property<int>("soupkitchen_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("soupkitchen_id"));

                    b.Property<int>("fk_user_id")
                        .HasColumnType("integer");

                    b.HasKey("soupkitchen_id");

                    b.HasIndex("fk_user_id");

                    b.ToTable("t_soupkitchen");
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_Treasury", b =>
                {
                    b.Property<int>("treasury_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("treasury_id"));

                    b.Property<int>("fk_user_id")
                        .HasColumnType("integer");

                    b.Property<BigInteger>("treasury_coin")
                        .HasColumnType("numeric");

                    b.Property<double>("treasury_gainrate")
                        .HasColumnType("double precision");

                    b.Property<double>("treasury_multiplier")
                        .HasColumnType("double precision");

                    b.HasKey("treasury_id");

                    b.HasIndex("fk_user_id");

                    b.ToTable("t_treasury");
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_User", b =>
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

                    b.HasData(
                        new
                        {
                            user_id = -999,
                            user_name = "yifahnadmin"
                        });
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

                    b.HasData(
                        new
                        {
                            Id = "70ff5865-335d-4d60-9851-d91499c5505c",
                            Name = "Admin",
                            NormalizedName = "ADMIN"
                        },
                        new
                        {
                            Id = "520d6e0e-235c-47c2-a1d8-5078f7b3fa43",
                            Name = "User",
                            NormalizedName = "USER"
                        });
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

                    b.HasData(
                        new
                        {
                            UserId = "5de48f8c-0a70-4e21-9cc0-798ff818fdc3",
                            RoleId = "70ff5865-335d-4d60-9851-d91499c5505c"
                        });
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

            modelBuilder.Entity("MM_API.Database.Postgres.ApplicationUser", b =>
                {
                    b.HasOne("MM_API.Database.Postgres.DbSchema.t_User", "User")
                        .WithMany()
                        .HasForeignKey("CustomUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_Armoury", b =>
                {
                    b.HasOne("MM_API.Database.Postgres.DbSchema.t_User", "User")
                        .WithMany()
                        .HasForeignKey("fk_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_Character", b =>
                {
                    b.HasOne("MM_API.Database.Postgres.DbSchema.t_User", "User")
                        .WithMany()
                        .HasForeignKey("fk_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_Kingdom", b =>
                {
                    b.HasOne("MM_API.Database.Postgres.DbSchema.t_User", "User")
                        .WithMany()
                        .HasForeignKey("fk_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_Session", b =>
                {
                    b.HasOne("MM_API.Database.Postgres.DbSchema.t_User", "User")
                        .WithMany()
                        .HasForeignKey("fk_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_Soupkitchen", b =>
                {
                    b.HasOne("MM_API.Database.Postgres.DbSchema.t_User", "User")
                        .WithMany()
                        .HasForeignKey("fk_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MM_API.Database.Postgres.DbSchema.t_Treasury", b =>
                {
                    b.HasOne("MM_API.Database.Postgres.DbSchema.t_User", "User")
                        .WithMany()
                        .HasForeignKey("fk_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
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
                    b.HasOne("MM_API.Database.Postgres.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("MM_API.Database.Postgres.ApplicationUser", null)
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

                    b.HasOne("MM_API.Database.Postgres.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("MM_API.Database.Postgres.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
