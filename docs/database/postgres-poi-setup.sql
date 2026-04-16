-- GeoGuide Phase 1 schema bootstrap (PostgreSQL)
-- Aligned with src/GeoGuide.Cms EF Core model and migrations.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS poi_tenants (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    updated_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS pois (
    id UUID PRIMARY KEY,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    tenant_id UUID NULL REFERENCES poi_tenants(id) ON DELETE SET NULL,
    approval_status INTEGER NOT NULL DEFAULT 0,
    name VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,
    trigger_radius_meters INTEGER NOT NULL,
    cooldown_minutes INTEGER NOT NULL,
    priority INTEGER NOT NULL,
    category_key VARCHAR(100) NOT NULL,
    category_label VARCHAR(200) NOT NULL,
    image_url TEXT NOT NULL,
    map_url TEXT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT ck_pois_latitude_range CHECK (latitude >= -90 AND latitude <= 90),
    CONSTRAINT ck_pois_longitude_range CHECK (longitude >= -180 AND longitude <= 180),
    CONSTRAINT ck_pois_trigger_radius_positive CHECK (trigger_radius_meters > 0),
    CONSTRAINT ck_pois_cooldown_minutes_non_negative CHECK (cooldown_minutes >= 0),
    CONSTRAINT ck_pois_priority_non_negative CHECK (priority >= 0)
);

CREATE TABLE IF NOT EXISTS poi_audios (
    id UUID PRIMARY KEY,
    poi_id UUID NOT NULL REFERENCES pois(id) ON DELETE CASCADE,
    language_code VARCHAR(10) NOT NULL,
    content_type INTEGER NOT NULL,
    audio_url TEXT NULL,
    tts_content TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT ck_poi_audios_content_payload CHECK (
        (content_type = 1 AND audio_url IS NOT NULL)
        OR (content_type = 2 AND tts_content IS NOT NULL)
    )
);

CREATE TABLE IF NOT EXISTS tours (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    thumbnail_url TEXT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS tour_poi_mappings (
    tour_id UUID NOT NULL REFERENCES tours(id) ON DELETE CASCADE,
    poi_id UUID NOT NULL REFERENCES pois(id) ON DELETE CASCADE,
    order_index INTEGER NOT NULL,
    PRIMARY KEY (tour_id, poi_id)
);

CREATE TABLE IF NOT EXISTS playback_logs (
    id UUID PRIMARY KEY,
    poi_id UUID NOT NULL REFERENCES pois(id) ON DELETE CASCADE,
    played_at TIMESTAMPTZ NOT NULL,
    trigger_type VARCHAR(20) NOT NULL,
    duration_seconds INTEGER NOT NULL,
    device_id VARCHAR(200) NOT NULL,
    CONSTRAINT ck_playback_logs_trigger_type CHECK (trigger_type IN ('gps', 'qr', 'manual')),
    CONSTRAINT ck_playback_logs_duration_non_negative CHECK (duration_seconds >= 0)
);

CREATE INDEX IF NOT EXISTS ix_pois_latitude_longitude ON pois (latitude, longitude);
CREATE INDEX IF NOT EXISTS ix_poi_audios_poi_id_language_code_content_type_is_deleted
    ON poi_audios (poi_id, language_code, content_type, is_deleted);
CREATE INDEX IF NOT EXISTS ix_tour_poi_mappings_poi_id ON tour_poi_mappings (poi_id);
CREATE INDEX IF NOT EXISTS ix_playback_logs_poi_id ON playback_logs (poi_id);
CREATE INDEX IF NOT EXISTS ix_playback_logs_played_at ON playback_logs (played_at);

-- Minimal seed for local demo bootstrap.
INSERT INTO poi_tenants (id, name, slug, is_active, updated_at)
VALUES ('6eb3f509-c1a5-4d8d-9bb4-e307db6d0a75', 'Demo POI Tenant', 'demo-poi-tenant', TRUE, NOW())
ON CONFLICT (id) DO NOTHING;

INSERT INTO pois (
    id, created_at, updated_at, is_deleted, tenant_id, approval_status, name, description,
    latitude, longitude, trigger_radius_meters, cooldown_minutes, priority, category_key,
    category_label, image_url, map_url, is_active
)
VALUES (
    '9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01', NOW(), NOW(), FALSE,
    '6eb3f509-c1a5-4d8d-9bb4-e307db6d0a75', 1, 'Bến Thành Market',
    'Khu chợ nổi tiếng ở trung tâm TP.HCM.',
    10.7720, 106.6983, 80, 15, 1, 'attraction', 'Tham quan',
    'https://images.unsplash.com/photo-1555921015-5532091f6026',
    'https://maps.google.com/?q=10.7720,106.6983',
    TRUE
)
ON CONFLICT (id) DO NOTHING;

INSERT INTO poi_audios (
    id, poi_id, language_code, content_type, audio_url, tts_content, created_at, updated_at, is_deleted
)
VALUES (
    '0f7fb649-d0ba-4bb7-9c05-d5db40cbdf01',
    '9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01',
    'vi',
    1,
    'https://example.com/audio/ben-thanh-market.mp3',
    NULL,
    NOW(),
    NOW(),
    FALSE
)
ON CONFLICT (id) DO NOTHING;
