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
  "CorsOrigin": "https://your-frontend-domain.com"
}
```
---
### Configuration Keys

| Key              | Description                                                                 |
|------------------|-----------------------------------------------------------------------------|
| `BookingContext` | PostgreSQL connection string used by EF Core to connect to the database.   |
| `CorsOrigin`     | The origin URL of the frontend allowed to access this API (CORS policy).    |

---

## Running the Booking API with Docker

### 🛠️ Build the Docker Image

```bash
docker build -t booking-api .
```
---
### ▶️ Run the Docker Container
```bash
docker run -d \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e CorsOrigin=https://your-frontend.com \
  -e ConnectionStrings__BookingContext="Host=host;Database=db;Username=user;Password=pass" \
  -p 5000:80 \
  --name booking-api \
  booking-api
```
