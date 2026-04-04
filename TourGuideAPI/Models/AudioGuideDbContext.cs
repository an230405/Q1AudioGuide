using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TourGuideAPI.Models;

public partial class AudioGuideDbContext : DbContext
{
    public AudioGuideDbContext()
    {
    }

    public AudioGuideDbContext(DbContextOptions<AudioGuideDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppSetting> AppSettings { get; set; }

    public virtual DbSet<Audio> Audios { get; set; }

    public virtual DbSet<AudioLog> AudioLogs { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Poi> Pois { get; set; }

    public virtual DbSet<QrCode> QrCodes { get; set; }

    public virtual DbSet<Translation> Translations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserTracking> UserTrackings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=audio_guide_db;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__app_sett__3213E83F8EA86E6A");

            entity.ToTable("app_settings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SettingKey)
                .HasMaxLength(100)
                .HasColumnName("setting_key");
            entity.Property(e => e.SettingValue)
                .HasMaxLength(200)
                .HasColumnName("setting_value");
        });

        modelBuilder.Entity<Audio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__audio__3213E83F52972208");

            entity.ToTable("audio");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AudioUrl)
                .HasMaxLength(255)
                .HasColumnName("audio_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.PoiId).HasColumnName("poi_id");

            entity.HasOne(d => d.Language).WithMany(p => p.Audios)
                .HasForeignKey(d => d.LanguageId)
                .HasConstraintName("FK__audio__language___45F365D3");

            entity.HasOne(d => d.Poi).WithMany(p => p.Audios)
                .HasForeignKey(d => d.PoiId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__audio__poi_id__44FF419A");
        });

        modelBuilder.Entity<AudioLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__audio_lo__3213E83FDF10AEDF");

            entity.ToTable("audio_logs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(100)
                .HasColumnName("device_id");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.PlayTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("play_time");
            entity.Property(e => e.PoiId).HasColumnName("poi_id");

            entity.HasOne(d => d.Language).WithMany(p => p.AudioLogs)
                .HasForeignKey(d => d.LanguageId)
                .HasConstraintName("FK__audio_log__langu__5AEE82B9");

            entity.HasOne(d => d.Poi).WithMany(p => p.AudioLogs)
                .HasForeignKey(d => d.PoiId)
                .HasConstraintName("FK__audio_log__poi_i__59FA5E80");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__images__3213E83FE080BAF7");

            entity.ToTable("images");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Caption)
                .HasMaxLength(200)
                .HasColumnName("caption");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.PoiId).HasColumnName("poi_id");

            entity.HasOne(d => d.Poi).WithMany(p => p.Images)
                .HasForeignKey(d => d.PoiId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__images__poi_id__49C3F6B7");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__language__3213E83F926E2713");

            entity.ToTable("languages");

            entity.HasIndex(e => e.Code, "UQ__language__357D4CF95C7A9C2D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Poi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__poi__3213E83F9D08109A");

            entity.ToTable("poi");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Radius)
                .HasDefaultValue(30)
                .HasColumnName("radius");
        });

        modelBuilder.Entity<QrCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__qr_codes__3213E83F714C59F9");

            entity.ToTable("qr_codes");

            entity.HasIndex(e => e.QrValue, "UQ__qr_codes__7F85B087B0E56EDC").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PoiId).HasColumnName("poi_id");
            entity.Property(e => e.QrValue)
                .HasMaxLength(200)
                .HasColumnName("qr_value");

            entity.HasOne(d => d.Poi).WithMany(p => p.QrCodes)
                .HasForeignKey(d => d.PoiId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__qr_codes__poi_id__4E88ABD4");
        });

        modelBuilder.Entity<Translation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__translat__3213E83F16BF5964");

            entity.ToTable("translations");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.PoiId).HasColumnName("poi_id");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Language).WithMany(p => p.Translations)
                .HasForeignKey(d => d.LanguageId)
                .HasConstraintName("FK__translati__langu__412EB0B6");

            entity.HasOne(d => d.Poi).WithMany(p => p.Translations)
                .HasForeignKey(d => d.PoiId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__translati__poi_i__403A8C7D");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F32004ADD");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "UQ__users__F3DBC572AF6631B2").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserTracking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_tra__3213E83F95807071");

            entity.ToTable("user_tracking");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(100)
                .HasColumnName("device_id");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.PoiId).HasColumnName("poi_id");
            entity.Property(e => e.VisitTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("visit_time");

            entity.HasOne(d => d.Poi).WithMany(p => p.UserTrackings)
                .HasForeignKey(d => d.PoiId)
                .HasConstraintName("FK__user_trac__poi_i__5629CD9C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
