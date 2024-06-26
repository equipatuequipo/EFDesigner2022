﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sandbox_EFCore_Test;

#nullable disable

namespace Sandbox_EFCore_Test.Migrations
{
    [DbContext(typeof(EFModel1))]
    [Migration("20230330175006_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("dbo")
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Sandbox_EFCore_Test.BaseType", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.HasKey("Id");

                    b.ToTable("BaseTypes", "dbo");

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("Sandbox_EFCore_Test.Entity1", b =>
                {
                    b.HasBaseType("Sandbox_EFCore_Test.BaseType");

                    b.Property<string>("Property1")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("Entity1", "dbo");
                });

            modelBuilder.Entity("Sandbox_EFCore_Test.Entity2", b =>
                {
                    b.HasBaseType("Sandbox_EFCore_Test.BaseType");

                    b.Property<string>("Property2")
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("Entity2", "dbo");
                });

            modelBuilder.Entity("Sandbox_EFCore_Test.Entity1", b =>
                {
                    b.HasOne("Sandbox_EFCore_Test.BaseType", null)
                        .WithOne()
                        .HasForeignKey("Sandbox_EFCore_Test.Entity1", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Sandbox_EFCore_Test.Entity2", b =>
                {
                    b.HasOne("Sandbox_EFCore_Test.BaseType", null)
                        .WithOne()
                        .HasForeignKey("Sandbox_EFCore_Test.Entity2", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
