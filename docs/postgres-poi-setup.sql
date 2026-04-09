-- Run this script in your PostgreSQL database.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS poi (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL,
    description TEXT,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,
    trigger_radius_meters INTEGER NOT NULL DEFAULT 80,
    priority INTEGER NOT NULL DEFAULT 1,
    category_key TEXT NOT NULL DEFAULT 'other',
    category_label TEXT NOT NULL DEFAULT 'Dia diem khac',
    image_url TEXT,
    map_url TEXT,
    audio_url TEXT,
    tts_script TEXT,
    language_code TEXT NOT NULL DEFAULT 'vi-VN',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS playback_log (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    poi_id UUID NOT NULL REFERENCES poi(id) ON DELETE CASCADE,
    played_at TIMESTAMPTZ NOT NULL,
    trigger_type TEXT NOT NULL CHECK (trigger_type IN ('gps', 'qr', 'manual')),
    duration_seconds INTEGER NOT NULL DEFAULT 0,
    device_id TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_poi_active ON poi (is_active);
CREATE INDEX IF NOT EXISTS idx_poi_lat_lon ON poi (latitude, longitude);
CREATE INDEX IF NOT EXISTS idx_playback_log_poi_id ON playback_log (poi_id);
CREATE INDEX IF NOT EXISTS idx_playback_log_played_at ON playback_log (played_at DESC);

INSERT INTO poi (
    name,
    description,
    latitude,
    longitude,
    trigger_radius_meters,
    priority,
    category_key,
    category_label,
    image_url,
    map_url,
    audio_url,
    tts_script,
    language_code,
    is_active
)
VALUES
    (
        'Ben Thanh Market',
        'Khu cho noi tieng o trung tam TP.HCM',
        10.7720,
        106.6983,
        80,
        1,
        'attraction',
        'Tham quan',
        'https://example.com/images/ben-thanh.jpg',
        'https://maps.google.com/?q=10.7720,106.6983',
        'https://example.com/audio/ben-thanh.mp3',
        'Cho Ben Thanh la mot bieu tuong du lich quen thuoc cua thanh pho.',
        'vi-VN',
        TRUE
    ),
    (
        'Notre-Dame Cathedral',
        'Nha tho Duc Ba Sai Gon',
        10.7798,
        106.6990,
        80,
        1,
        'attraction',
        'Tham quan',
        'https://example.com/images/notre-dame.jpg',
        'https://maps.google.com/?q=10.7798,106.6990',
        'https://example.com/audio/notre-dame.mp3',
        'Nha tho Duc Ba la dia diem kien truc noi bat tai trung tam thanh pho.',
        'vi-VN',
        TRUE
    ),
    (
        'Tao Dan Park',
        'Cong vien xanh de di bo va thu gian',
        10.7778,
        106.6927,
        60,
        2,
        'park',
        'Cong vien',
        'https://example.com/images/tao-dan.jpg',
        'https://maps.google.com/?q=10.7778,106.6927',
        'https://example.com/audio/tao-dan.mp3',
        'Cong vien Tao Dan phu hop cho du khach nghi chan va tan huong khong gian xanh.',
        'vi-VN',
        TRUE
    ),
    (
        'Ho Chi Minh City Museum',
        'Bao tang lich su va van hoa',
        10.7765,
        106.7010,
        70,
        1,
        'attraction',
        'Tham quan',
        'https://example.com/images/city-museum.jpg',
        'https://maps.google.com/?q=10.7765,106.7010',
        'https://example.com/audio/city-museum.mp3',
        'Bao tang Thanh pho Ho Chi Minh trung bay cac dau moc lich su va van hoa quan trong.',
        'vi-VN',
        TRUE
    )
ON CONFLICT DO NOTHING;
