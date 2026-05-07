-- Run this script in your PostgreSQL database.
-- This version is aligned with the current CMS code in CmsBackend/GeoGuide.Cms.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS pois (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT NOT NULL,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,
    trigger_radius_meters INTEGER NOT NULL,
    priority INTEGER NOT NULL,
    category_key VARCHAR(100) NOT NULL,
    category_label VARCHAR(200) NOT NULL,
    image_url TEXT NOT NULL,
    map_url TEXT NOT NULL,
    audio_url TEXT NOT NULL,
    tts_script TEXT NOT NULL,
    language_code VARCHAR(20) NOT NULL,
    is_active BOOLEAN NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS playback_logs (
    id UUID PRIMARY KEY,
    poi_id UUID NOT NULL REFERENCES pois(id) ON DELETE CASCADE,
    played_at TIMESTAMPTZ NOT NULL,
    trigger_type VARCHAR(20) NOT NULL,
    duration_seconds INTEGER NOT NULL,
    device_id VARCHAR(200) NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_pois_active_priority ON pois (is_active, priority);
CREATE INDEX IF NOT EXISTS idx_playback_logs_poi_id ON playback_logs (poi_id);

INSERT INTO pois (
    id,
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
    is_active,
    updated_at
)
VALUES
    (
        '9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01',
        'Ben Thanh Market',
        'Khu cho noi tieng o trung tam TP.HCM.',
        10.7720,
        106.6983,
        80,
        1,
        'attraction',
        'Tham quan',
        'https://images.unsplash.com/photo-1555921015-5532091f6026',
        'https://maps.google.com/?q=10.7720,106.6983',
        'https://example.com/audio/ben-thanh-market.mp3',
        'Day la cho Ben Thanh, mot bieu tuong van hoa va du lich cua thanh pho.',
        'vi-VN',
        TRUE,
        '2026-04-09T10:00:00Z'
    ),
    (
        '458a1a0e-b524-4e95-92f4-684e80ea7b97',
        'Notre-Dame Cathedral',
        'Nha tho Duc Ba Sai Gon voi kien truc Phap tieu bieu.',
        10.7798,
        106.6990,
        90,
        2,
        'attraction',
        'Tham quan',
        'https://images.unsplash.com/photo-1583417267826-aebc4d1542e1',
        'https://maps.google.com/?q=10.7798,106.6990',
        'https://example.com/audio/notre-dame-cathedral.mp3',
        'Nha tho Duc Ba la mot diem nhan kien truc va lich su ngay giua trung tam thanh pho.',
        'vi-VN',
        TRUE,
        '2026-04-09T10:00:00Z'
    ),
    (
        '5807657e-240d-42cb-b6e8-b46fd35652ce',
        'Tao Dan Park',
        'Cong vien xanh phu hop cho di bo va thu gian.',
        10.7778,
        106.6927,
        100,
        3,
        'park',
        'Cong vien',
        'https://images.unsplash.com/photo-1506744038136-46273834b3fb',
        'https://maps.google.com/?q=10.7778,106.6927',
        'https://example.com/audio/tao-dan-park.mp3',
        'Cong vien Tao Dan la khoang xanh hien hoi, noi nguoi dan dia phuong thuong tap the duc vao sang som.',
        'vi-VN',
        TRUE,
        '2026-04-09T10:00:00Z'
    ),
    (
        'a76542f8-e34f-45ee-96c9-0e3961c4ca30',
        'Ho Chi Minh City Museum',
        'Bao tang gioi thieu lich su va van hoa thanh pho.',
        10.7765,
        106.7010,
        75,
        4,
        'museum',
        'Bao tang',
        'https://images.unsplash.com/photo-1518998053901-5348d3961a04',
        'https://maps.google.com/?q=10.7765,106.7010',
        'https://example.com/audio/hcm-city-museum.mp3',
        'Bao tang Thanh pho Ho Chi Minh luu giu nhieu tu lieu va hien vat quan trong.',
        'vi-VN',
        TRUE,
        '2026-04-09T10:00:00Z'
    )
ON CONFLICT (id) DO NOTHING;
