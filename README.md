## 🔧 Configuration

The application is configured using the `appsettings.json` file.

### `appsettings.json and appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "BookingContext": "<your-postgresql-connection-string>"
  },
  "Groq":{
     "ApiKey": "<your-api-key>"
  },
  "CorsOrigin": "https://your-frontend-domain.com"
}
```
---
### Configuration Keys

| Key              | Description                                                                 |
|------------------|-----------------------------------------------------------------------------|
| `BookingContext` | PostgreSQL connection string used by EF Core to connect to the database.    |
| `CorsOrigin`     | The origin URL of the frontend allowed to access this API (CORS policy).    |
| `Groq`           | Groq api key used for generating responses.                                 |

---

## Running the Booking API with Docker

### 🛠️ Build docker compose

```bash
docker-compose up --build
```

