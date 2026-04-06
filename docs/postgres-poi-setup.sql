-- Run this script in your PostgreSQL database.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS poi (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL,
    category_key TEXT NOT NULL DEFAULT 'other',
    category_label TEXT NOT NULL DEFAULT 'Dia diem khac',
    description TEXT,
    latitude DOUBLE PRECISION NOT NULL,
    longitude DOUBLE PRECISION NOT NULL,
    has_audio BOOLEAN NOT NULL DEFAULT TRUE,
    is_registered BOOLEAN NOT NULL DEFAULT TRUE,
    registered_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_poi_registered ON poi (is_registered);
CREATE INDEX IF NOT EXISTS idx_poi_lat_lon ON poi (latitude, longitude);

INSERT INTO poi (name, category_key, category_label, description, latitude, longitude, has_audio, is_registered)
VALUES
    ('Ben Thanh Market', 'attraction', 'Tham quan', 'Khu cho noi tieng o trung tam TP.HCM', 10.7720, 106.6983, TRUE, TRUE),
    ('Notre-Dame Cathedral', 'attraction', 'Tham quan', 'Nha tho Duc Ba Sai Gon', 10.7798, 106.6990, TRUE, TRUE),
    ('Tao Dan Park', 'park', 'Cong vien', 'Cong vien xanh de di bo va thu gian', 10.7778, 106.6927, TRUE, TRUE),
    ('Ho Chi Minh City Museum', 'attraction', 'Tham quan', 'Bao tang lich su va van hoa', 10.7765, 106.7010, TRUE, TRUE)
ON CONFLICT DO NOTHING;
