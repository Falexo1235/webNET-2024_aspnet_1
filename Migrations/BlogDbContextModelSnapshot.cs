﻿// <auto-generated />
using System;
using BlogApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BlogApi.Migrations
{
    [DbContext(typeof(BlogDbContext))]
    partial class BlogDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BlogApi.Models.AddrObj", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("id"));

                    b.Property<long?>("changeid")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("enddate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("isactive")
                        .HasColumnType("integer");

                    b.Property<int?>("isactual")
                        .HasColumnType("integer");

                    b.Property<string>("level")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long?>("nextid")
                        .HasColumnType("bigint");

                    b.Property<Guid>("objectguid")
                        .HasColumnType("uuid");

                    b.Property<long>("objectid")
                        .HasColumnType("bigint");

                    b.Property<int?>("opertypeid")
                        .HasColumnType("integer");

                    b.Property<long?>("previd")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("startdate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("typename")
                        .HasColumnType("text");

                    b.Property<DateTime?>("updatedate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("id");

                    b.ToTable("as_addr_obj", (string)null);
                });

            modelBuilder.Entity("BlogApi.Models.AdmHierarchy", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("id"));

                    b.Property<string>("areacode")
                        .HasColumnType("text");

                    b.Property<long?>("changeid")
                        .HasColumnType("bigint");

                    b.Property<string>("citycode")
                        .HasColumnType("text");

                    b.Property<DateTime?>("enddate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("isactive")
                        .HasColumnType("integer");

                    b.Property<long?>("nextid")
                        .HasColumnType("bigint");

                    b.Property<long>("objectid")
                        .HasColumnType("bigint");

                    b.Property<long>("parentobjid")
                        .HasColumnType("bigint");

                    b.Property<string>("path")
                        .HasColumnType("text");

                    b.Property<string>("placecode")
                        .HasColumnType("text");

                    b.Property<string>("plancode")
                        .HasColumnType("text");

                    b.Property<long?>("previd")
                        .HasColumnType("bigint");

                    b.Property<string>("regioncode")
                        .HasColumnType("text");

                    b.Property<DateTime?>("startdate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("streetcode")
                        .HasColumnType("text");

                    b.Property<DateTime?>("updatedate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("id");

                    b.ToTable("as_adm_hierarchy", (string)null);
                });

            modelBuilder.Entity("BlogApi.Models.Comment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<string>("Author")
                        .HasColumnType("text");

                    b.Property<Guid>("AuthorId")
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("DeleteDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("PostId")
                        .HasColumnType("uuid");

                    b.Property<int>("SubComments")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("BlogApi.Models.Community", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<bool>("IsClosed")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("SubscribersCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Communities");
                });

            modelBuilder.Entity("BlogApi.Models.CommunityUser", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CommunityId")
                        .HasColumnType("uuid");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("UserId", "CommunityId");

                    b.HasIndex("CommunityId");

                    b.ToTable("CommunityUsers");
                });

            modelBuilder.Entity("BlogApi.Models.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AdminId")
                        .HasColumnType("integer");

                    b.Property<Guid>("AdminId1")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsPrivate")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AdminId1");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("BlogApi.Models.House", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("id"));

                    b.Property<string>("addnum1")
                        .HasColumnType("text");

                    b.Property<string>("addnum2")
                        .HasColumnType("text");

                    b.Property<int?>("addtype1")
                        .HasColumnType("integer");

                    b.Property<int?>("addtype2")
                        .HasColumnType("integer");

                    b.Property<long?>("changeid")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("enddate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("housenum")
                        .HasColumnType("text");

                    b.Property<int?>("housetype")
                        .HasColumnType("integer");

                    b.Property<int?>("isactive")
                        .HasColumnType("integer");

                    b.Property<int?>("isactual")
                        .HasColumnType("integer");

                    b.Property<long?>("nextid")
                        .HasColumnType("bigint");

                    b.Property<Guid>("objectguid")
                        .HasColumnType("uuid");

                    b.Property<long>("objectid")
                        .HasColumnType("bigint");

                    b.Property<int?>("opertypeid")
                        .HasColumnType("integer");

                    b.Property<long?>("previd")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("startdate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("updatedate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("id");

                    b.ToTable("as_houses", (string)null);
                });

            modelBuilder.Entity("BlogApi.Models.Like", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("PostId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.ToTable("Likes");
                });

            modelBuilder.Entity("BlogApi.Models.ObjectLevel", b =>
                {
                    b.Property<int>("level")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("level"));

                    b.Property<DateTime>("enddate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("isactive")
                        .HasColumnType("integer");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("startdate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("updatedate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("level");

                    b.ToTable("as_object_levels", (string)null);
                });

            modelBuilder.Entity("BlogApi.Models.Post", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<Guid?>("AddressId")
                        .HasColumnType("uuid");

                    b.Property<string>("Author")
                        .HasColumnType("text");

                    b.Property<Guid>("AuthorId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CommunityId")
                        .HasColumnType("uuid");

                    b.Property<string>("CommunityName")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Image")
                        .HasColumnType("text");

                    b.Property<int?>("ReadingTime")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.HasIndex("CommunityId");

                    b.ToTable("Posts");
                });

            modelBuilder.Entity("BlogApi.Models.PostTag", b =>
                {
                    b.Property<Guid>("PostId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TagId")
                        .HasColumnType("uuid");

                    b.HasKey("PostId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("PostTags");
                });

            modelBuilder.Entity("BlogApi.Models.RevokedToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("ExpirationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("RevokedTokens");
                });

            modelBuilder.Entity("BlogApi.Models.Tag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("PostId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("BlogApi.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<DateTime?>("BirthDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(70)
                        .HasColumnType("character varying(70)");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("BlogApi.Models.Comment", b =>
                {
                    b.HasOne("BlogApi.Models.Post", null)
                        .WithMany("Comments")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BlogApi.Models.CommunityUser", b =>
                {
                    b.HasOne("BlogApi.Models.Community", null)
                        .WithMany("CommunityUsers")
                        .HasForeignKey("CommunityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BlogApi.Models.Group", b =>
                {
                    b.HasOne("BlogApi.Models.User", "Admin")
                        .WithMany()
                        .HasForeignKey("AdminId1")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Admin");
                });

            modelBuilder.Entity("BlogApi.Models.Like", b =>
                {
                    b.HasOne("BlogApi.Models.Post", null)
                        .WithMany("Likes")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BlogApi.Models.Post", b =>
                {
                    b.HasOne("BlogApi.Models.Community", null)
                        .WithMany("Posts")
                        .HasForeignKey("CommunityId");
                });

            modelBuilder.Entity("BlogApi.Models.PostTag", b =>
                {
                    b.HasOne("BlogApi.Models.Post", "Post")
                        .WithMany("PostTags")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlogApi.Models.Tag", "Tag")
                        .WithMany("PostTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("BlogApi.Models.Tag", b =>
                {
                    b.HasOne("BlogApi.Models.Post", null)
                        .WithMany("Tags")
                        .HasForeignKey("PostId");
                });

            modelBuilder.Entity("BlogApi.Models.Community", b =>
                {
                    b.Navigation("CommunityUsers");

                    b.Navigation("Posts");
                });

            modelBuilder.Entity("BlogApi.Models.Post", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("Likes");

                    b.Navigation("PostTags");

                    b.Navigation("Tags");
                });

            modelBuilder.Entity("BlogApi.Models.Tag", b =>
                {
                    b.Navigation("PostTags");
                });
#pragma warning restore 612, 618
        }
    }
}
