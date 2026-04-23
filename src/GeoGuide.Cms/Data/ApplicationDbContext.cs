using GeoGuide.Cms.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GeoGuide.Cms.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Poi> Pois => Set<Poi>();
    public DbSet<PoiAudio> PoiAudios => Set<PoiAudio>();
    public DbSet<PlaybackLog> PlaybackLogs => Set<PlaybackLog>();
    public DbSet<DeviceSessionJoin> DeviceSessionJoins => Set<DeviceSessionJoin>();
    public DbSet<PoiTenant> PoiTenants => Set<PoiTenant>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<TourPoiMapping> TourPoiMappings => Set<TourPoiMapping>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("cms_users");
            entity.Property(p => p.DisplayName).HasColumnName("display_name").HasMaxLength(200);
            entity.Property(p => p.TenantId).HasColumnName("tenant_id");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>(entity =>
        {
            entity.ToTable("cms_roles");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("cms_user_roles");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("cms_user_claims");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("cms_user_logins");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("cms_user_tokens");
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("cms_role_claims");
        });

        modelBuilder.Entity<PoiTenant>(entity =>
        {
            entity.ToTable("poi_tenants");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id).HasColumnName("id");
            entity.Property(t => t.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(t => t.Slug).HasColumnName("slug").HasMaxLength(100);
            entity.Property(t => t.IsActive).HasColumnName("is_active");
            entity.Property(t => t.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(t => t.Slug).IsUnique();
        });

        modelBuilder.Entity<Poi>(entity =>
        {
            entity.ToTable("pois");
            entity.HasKey(p => p.Id);
            entity.HasQueryFilter(p => !p.IsDeleted);

            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.CreatedAt).HasColumnName("created_at");
            entity.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            entity.Property(p => p.IsDeleted).HasColumnName("is_deleted");
            entity.Property(p => p.TenantId).HasColumnName("tenant_id");
            entity.Property(p => p.ApprovalStatus).HasColumnName("approval_status");
            entity.Property(p => p.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(p => p.Description).HasColumnName("description");
            entity.Property(p => p.Latitude).HasColumnName("latitude");
            entity.Property(p => p.Longitude).HasColumnName("longitude");
            entity.Property(p => p.TriggerRadiusMeters).HasColumnName("trigger_radius_meters");
            entity.Property(p => p.CooldownMinutes).HasColumnName("cooldown_minutes");
            entity.Property(p => p.Priority).HasColumnName("priority");
            entity.Property(p => p.CategoryKey).HasColumnName("category_key").HasMaxLength(100);
            entity.Property(p => p.CategoryLabel).HasColumnName("category_label").HasMaxLength(200);
            entity.Property(p => p.ImageUrl).HasColumnName("image_url");
            entity.Property(p => p.MapUrl).HasColumnName("map_url");
            entity.Property(p => p.IsActive).HasColumnName("is_active");

            entity.HasIndex(p => new { p.Latitude, p.Longitude });

            entity.HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PoiAudio>(entity =>
        {
            entity.ToTable("poi_audios");
            entity.HasKey(p => p.Id);
            entity.HasQueryFilter(p => !p.IsDeleted);

            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.PoiId).HasColumnName("poi_id");
            entity.Property(p => p.LanguageCode).HasColumnName("language_code").HasMaxLength(10);
            entity.Property(p => p.ContentType).HasColumnName("content_type");
            entity.Property(p => p.AudioUrl).HasColumnName("audio_url");
            entity.Property(p => p.TtsContent).HasColumnName("tts_content");
            entity.Property(p => p.CreatedAt).HasColumnName("created_at");
            entity.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            entity.Property(p => p.IsDeleted).HasColumnName("is_deleted");

            entity.HasOne(p => p.Poi)
                .WithMany(p => p.Audios)
                .HasForeignKey(p => p.PoiId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlaybackLog>(entity =>
        {
            entity.ToTable("playback_logs");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.PoiId).HasColumnName("poi_id");
            entity.Property(p => p.PlayedAt).HasColumnName("played_at");
            entity.Property(p => p.TriggerType).HasColumnName("trigger_type").HasMaxLength(20);
            entity.Property(p => p.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(p => p.DeviceId).HasColumnName("device_id").HasMaxLength(200);
            entity.Property(p => p.SessionToken).HasColumnName("session_token").HasMaxLength(120);
            entity.Property(p => p.ClientType).HasColumnName("client_type").HasMaxLength(20);

            entity.HasIndex(p => p.SessionToken);
            entity.HasIndex(p => new { p.SessionToken, p.DeviceId });

            entity.HasOne(p => p.Poi)
                .WithMany()
                .HasForeignKey(p => p.PoiId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeviceSessionJoin>(entity =>
        {
            entity.ToTable("device_session_joins");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.SessionToken).HasColumnName("session_token").HasMaxLength(120);
            entity.Property(p => p.DeviceId).HasColumnName("device_id").HasMaxLength(200);
            entity.Property(p => p.ClientType).HasColumnName("client_type").HasMaxLength(20);
            entity.Property(p => p.AccessMode).HasColumnName("access_mode").HasMaxLength(20);
            entity.Property(p => p.JoinedAt).HasColumnName("joined_at");
            entity.Property(p => p.LastSeenAt).HasColumnName("last_seen_at");

            entity.HasIndex(p => p.SessionToken);
            entity.HasIndex(p => new { p.SessionToken, p.DeviceId }).IsUnique();
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.ToTable("tours");
            entity.HasKey(t => t.Id);
            entity.HasQueryFilter(t => !t.IsDeleted);

            entity.Property(t => t.Id).HasColumnName("id");
            entity.Property(t => t.Name).HasColumnName("name").HasMaxLength(255);
            entity.Property(t => t.Description).HasColumnName("description");
            entity.Property(t => t.ThumbnailUrl).HasColumnName("thumbnail_url");
            entity.Property(t => t.IsActive).HasColumnName("is_active");
            entity.Property(t => t.IsDeleted).HasColumnName("is_deleted");
            entity.Property(t => t.CreatedAt).HasColumnName("created_at");
            entity.Property(t => t.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<TourPoiMapping>(entity =>
        {
            entity.ToTable("tour_poi_mappings");
            entity.HasKey(t => new { t.TourId, t.PoiId });
            entity.HasQueryFilter(t => !t.Tour!.IsDeleted && !t.Poi!.IsDeleted);

            entity.Property(t => t.TourId).HasColumnName("tour_id");
            entity.Property(t => t.PoiId).HasColumnName("poi_id");
            entity.Property(t => t.OrderIndex).HasColumnName("order_index");

            entity.HasOne(t => t.Tour)
                .WithMany(t => t.PoiMappings)
                .HasForeignKey(t => t.TourId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Poi)
                .WithMany(p => p.TourMappings)
                .HasForeignKey(t => t.PoiId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PoiTenant>().HasData(PoiSeedData.Tenants);
        modelBuilder.Entity<Poi>().HasData(PoiSeedData.All);
        modelBuilder.Entity<PoiAudio>().HasData(PhaseOneSeedData.PoiAudios);
        modelBuilder.Entity<Tour>().HasData(PhaseOneSeedData.Tours);
        modelBuilder.Entity<TourPoiMapping>().HasData(PhaseOneSeedData.TourPoiMappings);
    }
}
