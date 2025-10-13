#!/bin/bash

# Volur Development Setup Script
# This script sets up the development environment for Volur

set -e

echo "🚀 Setting up Volur development environment..."

# Check prerequisites
check_command() {
    if ! command -v $1 &> /dev/null; then
        echo "❌ $1 is not installed. Please install it first."
        exit 1
    fi
}

echo "📋 Checking prerequisites..."
check_command dotnet
check_command node
check_command docker

echo "✅ All prerequisites are installed"

# Backend setup
echo ""
echo "🔧 Setting up backend..."
cd src/Volur.Api

echo "  - Initializing user secrets..."
dotnet user-secrets init

echo "  - Please enter your EODHD API token:"
read -r API_TOKEN
dotnet user-secrets set "Eodhd:ApiToken" "$API_TOKEN"

echo "  - Restoring NuGet packages..."
cd ../..
dotnet restore

echo "✅ Backend setup complete"

# Frontend setup
echo ""
echo "🎨 Setting up frontend..."
cd web/volur-web

echo "  - Installing npm packages..."
npm install

echo "✅ Frontend setup complete"

# MongoDB setup
echo ""
echo "🗄️  Starting MongoDB with Docker..."
cd ../..

if [ ! "$(docker ps -q -f name=volur-mongo)" ]; then
    if [ "$(docker ps -aq -f status=exited -f name=volur-mongo)" ]; then
        echo "  - Starting existing MongoDB container..."
        docker start volur-mongo
    else
        echo "  - Creating new MongoDB container..."
        docker run -d \
            --name volur-mongo \
            -p 27017:27017 \
            mongo:7.0
    fi
fi

echo "✅ MongoDB is running"

# Summary
echo ""
echo "🎉 Development environment setup complete!"
echo ""
echo "To start developing:"
echo ""
echo "1. Start the API:"
echo "   cd src/Volur.Api"
echo "   dotnet run"
echo "   (API will be at http://localhost:5000)"
echo ""
echo "2. Start the frontend (in a new terminal):"
echo "   cd web/volur-web"
echo "   npm run dev"
echo "   (Web UI will be at http://localhost:5173)"
echo ""
echo "3. Access Swagger documentation:"
echo "   http://localhost:5000/swagger"
echo ""
echo "Happy coding! 🚀"

