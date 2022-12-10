﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Palm.Infrastructure;

#nullable disable

namespace Palm.Migrations.SessionData
{
    [DbContext(typeof(SessionDataContext))]
    partial class SessionDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Palm.Models.Sessions.Answer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsCorrect")
                        .HasColumnType("boolean");

                    b.Property<Guid?>("QuestionId")
                        .HasColumnType("uuid");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.ToTable("Answer");
                });

            modelBuilder.Entity("Palm.Models.Sessions.Question", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("Palm.Models.Sessions.QuestionAnswer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AnswerId")
                        .HasColumnType("integer");

                    b.Property<string>("QuestionId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("TakeId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("TakeId");

                    b.ToTable("QuestionAnswer");
                });

            modelBuilder.Entity("Palm.Models.Sessions.Session", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("GroupInfoId")
                        .HasColumnType("integer");

                    b.Property<Guid>("HostId")
                        .HasColumnType("uuid");

                    b.Property<List<string>>("Questions")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("ShortId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<List<string>>("Students")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GroupInfoId");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("Palm.Models.Sessions.SessionGroupInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("GroupName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsTeacherConnected")
                        .HasColumnType("boolean");

                    b.Property<string>("TeacherId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("SessionGroupInfo");
                });

            modelBuilder.Entity("Palm.Models.Sessions.Take", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ConnectionId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("SessionId")
                        .HasColumnType("uuid");

                    b.Property<string>("StudentId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<TimeOnly>("TimeCompleted")
                        .HasColumnType("time without time zone");

                    b.Property<TimeOnly>("TimeStart")
                        .HasColumnType("time without time zone");

                    b.HasKey("Id");

                    b.HasIndex("SessionId");

                    b.ToTable("Take");
                });

            modelBuilder.Entity("Palm.Models.Sessions.Answer", b =>
                {
                    b.HasOne("Palm.Models.Sessions.Question", null)
                        .WithMany("Answers")
                        .HasForeignKey("QuestionId");
                });

            modelBuilder.Entity("Palm.Models.Sessions.QuestionAnswer", b =>
                {
                    b.HasOne("Palm.Models.Sessions.Take", null)
                        .WithMany("QuestionAnswers")
                        .HasForeignKey("TakeId");
                });

            modelBuilder.Entity("Palm.Models.Sessions.Session", b =>
                {
                    b.HasOne("Palm.Models.Sessions.SessionGroupInfo", "GroupInfo")
                        .WithMany()
                        .HasForeignKey("GroupInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GroupInfo");
                });

            modelBuilder.Entity("Palm.Models.Sessions.Take", b =>
                {
                    b.HasOne("Palm.Models.Sessions.Session", null)
                        .WithMany("Takes")
                        .HasForeignKey("SessionId");
                });

            modelBuilder.Entity("Palm.Models.Sessions.Question", b =>
                {
                    b.Navigation("Answers");
                });

            modelBuilder.Entity("Palm.Models.Sessions.Session", b =>
                {
                    b.Navigation("Takes");
                });

            modelBuilder.Entity("Palm.Models.Sessions.Take", b =>
                {
                    b.Navigation("QuestionAnswers");
                });
#pragma warning restore 612, 618
        }
    }
}