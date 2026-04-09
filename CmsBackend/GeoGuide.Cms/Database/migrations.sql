CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE pois (
    id uuid NOT NULL,
    name character varying(200) NOT NULL,
    description text NOT NULL,
    latitude double precision NOT NULL,
    longitude double precision NOT NULL,
    trigger_radius_meters integer NOT NULL,
    priority integer NOT NULL,
    category_key character varying(100) NOT NULL,
    category_label character varying(200) NOT NULL,
    image_url text NOT NULL,
    map_url text NOT NULL,
    audio_url text NOT NULL,
    tts_script text NOT NULL,
    language_code character varying(20) NOT NULL,
    is_active boolean NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    CONSTRAINT "PK_pois" PRIMARY KEY (id)
);

CREATE TABLE playback_logs (
    id uuid NOT NULL,
    poi_id uuid NOT NULL,
    played_at timestamp with time zone NOT NULL,
    trigger_type character varying(20) NOT NULL,
    duration_seconds integer NOT NULL,
    device_id character varying(200) NOT NULL,
    CONSTRAINT "PK_playback_logs" PRIMARY KEY (id),
    CONSTRAINT "FK_playback_logs_pois_poi_id" FOREIGN KEY (poi_id) REFERENCES pois (id) ON DELETE CASCADE
);

INSERT INTO pois (id, audio_url, category_key, category_label, description, image_url, is_active, language_code, latitude, longitude, map_url, name, priority, trigger_radius_meters, tts_script, updated_at)
VALUES ('458a1a0e-b524-4e95-92f4-684e80ea7b97', 'https://example.com/audio/notre-dame-cathedral.mp3', 'attraction', 'Tham quan', 'Nha tho Duc Ba Sai Gon voi kien truc Phap tieu bieu.', 'https://images.unsplash.com/photo-1583417267826-aebc4d1542e1', TRUE, 'vi-VN', 10.7798, 106.699, 'https://maps.google.com/?q=10.7798,106.6990', 'Notre-Dame Cathedral', 2, 90, 'Nha tho Duc Ba la mot diem nhan kien truc va lich su ngay giua trung tam thanh pho.', TIMESTAMPTZ '2026-04-09T10:00:00+00:00');
INSERT INTO pois (id, audio_url, category_key, category_label, description, image_url, is_active, language_code, latitude, longitude, map_url, name, priority, trigger_radius_meters, tts_script, updated_at)
VALUES ('5807657e-240d-42cb-b6e8-b46fd35652ce', 'https://example.com/audio/tao-dan-park.mp3', 'park', 'Cong vien', 'Cong vien xanh phu hop cho di bo va thu gian.', 'https://images.unsplash.com/photo-1506744038136-46273834b3fb', TRUE, 'vi-VN', 10.777799999999999, 106.6927, 'https://maps.google.com/?q=10.7778,106.6927', 'Tao Dan Park', 3, 100, 'Cong vien Tao Dan la khoang xanh hien hoi, noi nguoi dan dia phuong thuong tap the duc vao sang som.', TIMESTAMPTZ '2026-04-09T10:00:00+00:00');
INSERT INTO pois (id, audio_url, category_key, category_label, description, image_url, is_active, language_code, latitude, longitude, map_url, name, priority, trigger_radius_meters, tts_script, updated_at)
VALUES ('9f0bbf75-a9fc-4a94-93a1-7c5ef0fc6a01', 'https://example.com/audio/ben-thanh-market.mp3', 'attraction', 'Tham quan', 'Khu cho noi tieng o trung tam TP.HCM.', 'https://images.unsplash.com/photo-1555921015-5532091f6026', TRUE, 'vi-VN', 10.772, 106.6983, 'https://maps.google.com/?q=10.7720,106.6983', 'Ben Thanh Market', 1, 80, 'Day la cho Ben Thanh, mot bieu tuong van hoa va du lich cua thanh pho.', TIMESTAMPTZ '2026-04-09T10:00:00+00:00');
INSERT INTO pois (id, audio_url, category_key, category_label, description, image_url, is_active, language_code, latitude, longitude, map_url, name, priority, trigger_radius_meters, tts_script, updated_at)
VALUES ('a76542f8-e34f-45ee-96c9-0e3961c4ca30', 'https://example.com/audio/hcm-city-museum.mp3', 'museum', 'Bao tang', 'Bao tang gioi thieu lich su va van hoa thanh pho.', 'https://images.unsplash.com/photo-1518998053901-5348d3961a04', TRUE, 'vi-VN', 10.7765, 106.70099999999999, 'https://maps.google.com/?q=10.7765,106.7010', 'Ho Chi Minh City Museum', 4, 75, 'Bao tang Thanh pho Ho Chi Minh luu giu nhieu tu lieu va hien vat quan trong.', TIMESTAMPTZ '2026-04-09T10:00:00+00:00');

CREATE INDEX "IX_playback_logs_poi_id" ON playback_logs (poi_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260409073359_InitialCreate', '10.0.5');

COMMIT;

