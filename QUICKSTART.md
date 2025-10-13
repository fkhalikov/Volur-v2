# Quick Start Guide

Get Volur up and running in 5 minutes!

## Prerequisites

- Docker and Docker Compose (recommended)
- OR .NET 8 SDK + Node.js 20+ + MongoDB

## Option 1: Docker (Easiest)

### 1. Get your EODHD API token
Sign up for a free account at [https://eodhd.com/](https://eodhd.com/) to get your API token.

### 2. Configure environment
```bash
cd deploy
cp .env.example .env
```

Edit `.env` and add your token:
```bash
EODHD_API_TOKEN=your_actual_token_here
```

### 3. Start everything
```bash
docker-compose up -d
```

### 4. Access the application
- **Web UI**: http://localhost:3000
- **API**: http://localhost:5000
- **API Docs**: http://localhost:5000/swagger

### 5. Stop everything
```bash
docker-compose down
```

## Option 2: Local Development

### 1. Start MongoDB
```bash
# Using Docker
docker run -d -p 27017:27017 --name volur-mongo mongo:7.0

# OR install MongoDB locally and start it
mongod --dbpath /path/to/data
```

### 2. Configure and run the API
```bash
cd src/Volur.Api

# Set up user secrets
dotnet user-secrets init
dotnet user-secrets set "Eodhd:ApiToken" "YOUR_TOKEN_HERE"

# Run the API
dotnet run
```

The API will be available at http://localhost:5000

### 3. Run the frontend
```bash
cd web/volur-web

# Install dependencies
npm install

# Start dev server
npm run dev
```

The web UI will be available at http://localhost:5173

## Verify Installation

### Test the API
```bash
# Health check
curl http://localhost:5000/api/health

# Get exchanges
curl http://localhost:5000/api/exchanges

# Get symbols for US exchange
curl "http://localhost:5000/api/exchanges/US/symbols?page=1&pageSize=10"
```

### Test the Web UI
1. Open http://localhost:3000 (Docker) or http://localhost:5173 (local)
2. You should see a list of exchanges
3. Click "View Symbols →" on any exchange
4. Browse and search symbols

## What's Next?

- **Explore the API**: Visit http://localhost:5000/swagger for interactive API documentation
- **Read the docs**: Check out [README.md](README.md) for full documentation
- **Development guide**: See [DEVELOPMENT.md](DEVELOPMENT.md) for development details

## Troubleshooting

### "Connection refused" errors
- Make sure MongoDB is running: `docker ps` or check your local MongoDB service
- Check if ports 5000 (API) and 3000/5173 (Web) are available

### "Invalid API token" errors
- Verify your EODHD API token is correct
- Check the token is properly set in `.env` or user secrets

### Frontend can't connect to API
- Check CORS settings allow your origin
- Verify the API is running on port 5000
- Check browser console for detailed errors

### Docker build fails
- Ensure Docker Desktop is running
- Try `docker-compose build --no-cache`
- Check Docker has enough resources allocated

## Getting Help

- Check the [README.md](README.md) for detailed documentation
- Review [DEVELOPMENT.md](DEVELOPMENT.md) for development setup
- Open an issue on GitHub for bugs or questions

