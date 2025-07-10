# Update Notification System Setup

## Configuration Files

### Local Configuration
File: `launcher-config.json` (in launcher directory)

```json
{
  "version": "1.0.0",
  "websiteUrl": "https://yoursite.com",
  "updateCheckUrl": "https://yoursite.com/mu-version.json"
}
```

### Remote Version File
File: `mu-version.json` (on your web server)

```json
{
  "version": "1.0.1",
  "patchUrl": "https://yoursite.com/downloads/patch-1.0.1.zip"
}
```

## Setup Steps

1. Edit `launcher-config.json` with your URLs
2. Upload `mu-version.json` to your web server
3. When users run launcher, they'll see update notification if remote version is different

## Update Process

1. User sees "NEW PATCH - MUST UPDATE" option
2. User presses `0`
3. Browser opens `patchUrl` for download

## Testing

Test your remote endpoint:
```bash
curl "https://yoursite.com/mu-version.json"
```

Should return valid JSON with `version` and `patchUrl` fields.