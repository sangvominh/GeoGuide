# System ERD (Phase 1 Baseline)

## Purpose

Tài liệu này là nguồn mô tả dữ liệu cho Giai đoạn 1, khớp với:

- `src/GeoGuide.Cms/Data/ApplicationDbContext.cs`
- EF migrations hiện tại trong `src/GeoGuide.Cms/Migrations`

## ERD

```mermaid
erDiagram
    POI_TENANT ||--o{ POI : "owns"
    POI ||--o{ POI_AUDIO : "has contents"
    TOUR ||--o{ TOUR_POI_MAPPING : "contains"
    POI ||--o{ TOUR_POI_MAPPING : "mapped in"
    POI ||--o{ PLAYBACK_LOG : "produces"

    POI_TENANT {
        uuid id PK
        string name
        string slug UNIQUE
        bool is_active
        datetime updated_at
    }

    POI {
        uuid id PK
        datetime created_at
        datetime updated_at
        bool is_deleted
        uuid tenant_id FK
        int approval_status
        string name
        string description
        float latitude
        float longitude
        int trigger_radius_meters
        int cooldown_minutes
        int priority
        string category_key
        string category_label
        string image_url
        string map_url
        bool is_active
    }

    POI_AUDIO {
        uuid id PK
        uuid poi_id FK
        string language_code
        int content_type
        string audio_url NULL
        string tts_content NULL
        datetime created_at
        datetime updated_at
        bool is_deleted
    }

    TOUR {
        uuid id PK
        string name
        string description
        string thumbnail_url
        bool is_active
        bool is_deleted
        datetime created_at
        datetime updated_at
    }

    TOUR_POI_MAPPING {
        uuid tour_id PK, FK
        uuid poi_id PK, FK
        int order_index
    }

    PLAYBACK_LOG {
        uuid id PK
        uuid poi_id FK
        datetime played_at
        string trigger_type
        int duration_seconds
        string device_id
    }
```

## Core Constraints

- `pois.latitude` trong khoảng `[-90, 90]`.
- `pois.longitude` trong khoảng `[-180, 180]`.
- `pois.trigger_radius_meters > 0`.
- `pois.cooldown_minutes >= 0`.
- `pois.priority >= 0`.
- `poi_audios` bắt buộc hợp lệ theo `content_type`:
  - `1` -> phải có `audio_url`.
  - `2` -> phải có `tts_content`.
- `playback_logs.trigger_type` chỉ nhận `gps | qr | manual`.
- `playback_logs.duration_seconds >= 0`.

## Notes

- Đây là schema Phase 1 hướng “nền tảng dữ liệu + CMS”, không còn mô hình POI gộp `audio_url/tts_script` như bản MVP cũ.
- API vẫn trả camelCase; DB dùng snake_case.
