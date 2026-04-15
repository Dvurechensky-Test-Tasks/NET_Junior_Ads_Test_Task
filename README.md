<p align="center">✨Dvurechensky✨</p>

<h1 align="center"> AdPlatformService - Junior .NET Test Task 📄 </h1>

<div align="center" style="margin: 20px 0; padding: 10px; background: #1c1917; border-radius: 10px;">
  <strong>🌐 Language: </strong>
  
  <a href="./README.ru.md" style="color: #F5F752; margin: 0 10px;">
    🇷🇺 Russian
  </a>
  | 
  <span style="color: #0891b2; margin: 0 10px;">
    ✅ 🇺🇸 English (current)
  </span>
</div>

---

**AdPlatformService** is a high-performance in-memory web service for storing and searching advertising platforms by location.

📄 Task documentation is also available in PDF: [TASK.NET.pdf](docs/TASK.NET.pdf)  
📄 Test input file: [test_input_data.txt](docs/test_input_data.txt)

<img src="https://github.com/jrohitofficial/jrohitofficial/blob/master/2nd%20arrow.gif?raw=true">

# ⭐ Table of Contents

- [⭐ Table of Contents](#-table-of-contents)
  - [🚀 Run](#-run)
    - [Start](#start)
    - [Debug](#debug)
    - [Release](#release)
  - [API](#api)
  - [✨ Features](#-features)
  - [🔹 Core Functionality](#-core-functionality)
    - [1. Loading platforms from file](#1-loading-platforms-from-file)
      - [Key Features](#key-features)
      - [Example file format](#example-file-format)
    - [2. Search by location](#2-search-by-location)
  - [🔹 Technical Approach](#-technical-approach)
    - [Immutable Collections](#immutable-collections)
    - [Hierarchical Index](#hierarchical-index)
    - [Logging](#logging)
    - [Fault Tolerance](#fault-tolerance)
    - [Set-based Search](#set-based-search)
  - [🔹 Advantages](#-advantages)
  - [🔹 Testing](#-testing)
  - [🛠 CI/CD](#-cicd)
  - [🔹 Docker](#-docker)
    - [Generate certificate](#generate-certificate)
    - [Run container](#run-container)
    - [Access](#access)
    - [Pull image](#pull-image)
    - [Run](#run)

<img src="https://github.com/jrohitofficial/jrohitofficial/blob/master/2nd%20arrow.gif?raw=true">

---

## 🚀 Run

### Start

```bash
dotnet run --project AdRegionService
```

### Debug

> 👉 API: [http://localhost:5411](http://localhost:5411)
> 👉 Swagger UI: [http://localhost:5411/swagger](http://localhost:5411/swagger)

### Release

> 👉 API: [http://localhost:5411](http://localhost:5411)
> 👉 Swagger UI: [http://localhost:5411/swagger](http://localhost:5411/swagger)

---

## API

<details>
<summary>Show API details</summary>

```http
POST /api/load
Content-Type: multipart/form-data

file=@platforms.txt
```

```json
{
	"message": "Data loaded",
	"loaded": 123,
	"skipped": 5
}
```

```http
GET /api/search?location=/ru/svrd
```

```json
[
	{ "name": "Cool Ads", "locations": ["/ru/svrd"] },
	{
		"name": "Revdinsky Worker",
		"locations": ["/ru/svrd/revda", "/ru/svrd/pervik"]
	}
]
```

</details>

---

## ✨ Features

- 📂 Load advertising platforms from file (`Stream`)
- ⚡ In-memory storage using **immutable collections**
- 📊 Load statistics (`loaded / skipped`)
- 🔍 Hierarchical location search:
  - `/loc1` matches all platforms under this level
  - `/loc1/loc2` also includes `/loc1`

- 🛡 Error handling:
  - `OperationCanceledException`
  - `OutOfMemoryException`
  - General errors are logged, state remains consistent

---

## 🔹 Core Functionality

### 1. Loading platforms from file

Method:

```csharp
LoadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
```

#### Key Features

- ✅ Accepts `Stream` (supports files and network sources)
- ✅ Ignores invalid lines:
  - empty lines
  - missing `:`
  - empty platform name
  - no valid locations

- ✅ Location indexing
  Stored in:

```csharp
ImmutableDictionary<string, ImmutableHashSet<AdPlatform>>
```

- ✅ Load statistics (`LoadStats`)
- ✅ Progress logging every 100,000 lines
- ✅ Error resilience:
  - keeps previous state on failure
  - logs all exceptions

#### Example file format

```txt
Yandex.Direct:/ru
Revdinsky Worker:/ru/svrd/revda,/ru/svrd/pervik
Ural Moscow Newspaper:/ru/msk,/ru/permobl,/ru/chelobl
Cool Ads:/ru/svrd
```

---

### 2. Search by location

- ✅ Method: `Search(string location)`
- ✅ Uses indexed lookup (no full scan)
- ✅ Supports hierarchical matching
- ✅ Returns `IEnumerable<AdPlatform>`
- ✅ Thread-safe
- ⚡ Efficient up to:
  - 1–2 million entries
  - 100–500 MB data

> For >10M entries → consider external storage (PostgreSQL, full-text search, etc.)

---

## 🔹 Technical Approach

### Immutable Collections

- `ImmutableArray`
- `ImmutableDictionary`
- `ImmutableHashSet`

➡️ Thread-safe without locks

---

### Hierarchical Index

Search `/a/b/c` → checks:

- `/a`
- `/a/b`
- `/a/b/c`

---

### Logging

- `ILogger`
- Progress logging
- Error tracking
- Ready for integration (Seq, Kibana, etc.)

---

### Fault Tolerance

- Invalid data is skipped
- State is preserved on failure
- Search never throws exceptions

---

### Set-based Search

- Uses `HashSet<AdPlatform>`
- Removes duplicates automatically
- Returns lightweight enumerable

---

## 🔹 Advantages

- ⚡ High performance (1–2M entries)
- 🛡 Robust error handling
- 🔗 Easy REST integration
- 🧵 Thread-safe architecture

---

## 🔹 Testing

- xUnit tests
- Covers:
  - loading
  - search
  - edge cases

- Run:

```bash
dotnet test
```

---

## 🛠 CI/CD

- Docker build on push to `main`
- Publish to GitHub Container Registry
- `dotnet build` + `dotnet test`

---

## 🔹 Docker

### Generate certificate

```bash
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout key.pem -out cert.pem -subj "/CN=localhost"
```

### Run container

```bash
docker-compose up --build
```

### Access

- [http://localhost:5411/swagger/index.html](http://localhost:5411/swagger/index.html)
- [https://localhost:5412/swagger/index.html](https://localhost:5412/swagger/index.html)

---

### Pull image

```bash
docker pull ghcr.io/dvurechensky/net_junior_ads_test_task/adservice:latest
```

### Run

```bash
docker run -it --rm -p 5411:5411 ghcr.io/dvurechensky/net_junior_ads_test_task/adservice:latest
```

---

<p align="center">✨Dvurechensky✨</p>
