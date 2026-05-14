# MVP Demo Script

## Demo Goal

Show one integrated flow where CMS manages a POI, mobile consumes it, narration is triggered, and playback logging reaches the backend.

## What changed by role

- **User App**: Mobile UI with debug panel, map interface, sync status, offline cache fallback, and manual narration playback.
- **Seller/Owner**: Seller portal to register, submit POIs, check status of submissions, and use AI Advisor to enhance POI descriptions.
- **Admin**: Control center dashboard, review and approve/reject POI submissions, view system status.
- **Backend/API**: Authentication, role-based access control (RBAC), POI CRUD, fake AI Advisor endpoint (local demo fallback), and Reference Workflow Console for Localization/Audio/Map API simulation.
- **Demo limitations**: GPS trigger is simulated or manual. AI Advisor uses a deterministic local fallback if no Gemini key is provided. Reference Workflow Consoles simulate third-party service responses.

## UI-Complete vs API/Placeholder

- **UI-Complete**: Admin Control Center, Seller Portal, Admin Review Submission, Mobile Debug Panel, AI Advisor UI form, Reference Workflow Console UI.
- **API/Placeholder**: AI Advisor real processing (has local fallback), Audio/TTS generation (simulated in console), Offline Map tile packaging (simulated in console), Localization translation (simulated in console).

## Demo Setup

- Confirm backend API is reachable by the mobile app.
- Confirm the mobile app has location permission if GPS trigger will be demonstrated.

## Demo Sequence

### 1. Login Admin

Say:
> First, let's log in to the CMS as an administrator to see the full control center.

Do:
1. Open the CMS login page `/Identity/Account/Login`.
2. Log in with the SystemAdmin credentials.

### 2. Open Dashboard/Control Center

Say:
> This is the Admin Control Center, which serves as the reference architecture dashboard.

Do:
1. Navigate to the Admin Control Center (`/`).
2. Briefly walk through the hero section, module status cards, and links to portals.

### 3. Register Seller

Say:
> Now, we will simulate a local business owner registering on the platform to add their Point of Interest.

Do:
1. Click on "Seller / Owner Portal".
2. Register a new user or log in as an existing Seller.

### 4. Seller Submits POI

Say:
> The seller can submit a new POI and use the AI Advisor to improve their content.

Do:
1. Go to "Submit New POI" in the Seller Portal.
2. Fill out basic details.
3. Use the **AI Advisor** to enhance the POI description. Point out the metadata and local demo fallback status.
4. Submit the POI. Show that its status is "Pending".

### 5. Admin Approves

Say:
> The submitted POI needs approval from a system administrator before it goes live.

Do:
1. Log out and log back in as SystemAdmin (or use a secondary browser window).
2. Go to the **Admin Review** portal.
3. Review the pending POI submission and click "Approve".

### 6. Mobile Sync / Offline Narrative

Say:
> On the mobile side, the app will sync the newly approved POI so users can see it on the map and hear the narration.

Do:
1. Open the Mobile App (MAUI).
2. Open the **Debug/Demo Status Panel**.
3. Tap "Sync POIs" to fetch the new POI from the backend.
4. Close the panel, find the POI on the map.
5. Tap the POI and manually trigger the playback. Explain that GPS triggers work similarly when moving into the radius.

### 7. AI Advisor / Audio / Localization / Map Console

Say:
> Finally, let's look at the Reference Workflow Console. These screens demonstrate how backend systems handle offline map tiles, audio processing, and translations.

Do:
1. In the CMS Admin dashboard, click on "Reference Workflow Console".
2. Walk through the mock interfaces for Localization, Audio/TTS, and Offline Maps.
3. Explain that these are UI-complete placeholder interfaces meant to represent integration with third-party providers.

## Demo Exit Criteria

- A new POI is created via the Seller portal and improved via AI Advisor.
- The Admin successfully approves the POI.
- The Mobile app syncs the new POI and plays back narration.
- Workflow Consoles are shown to illustrate background processes.
