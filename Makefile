# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help build restore clean test run publish docker docker-build docker-run docker-stop lint format analyze benchmark update-deps

# Default target
help:
	@echo "SQLite CRUD Generator - Available targets:"
	@echo ""
	@echo "Build & Development:"
	@echo "  make build          - Build the project (Release)"
	@echo "  make restore        - Restore NuGet packages"
	@echo "  make clean          - Clean build artifacts"
	@echo "  make rebuild        - Clean and rebuild"
	@echo "  make run            - Build and run application"
	@echo ""
	@echo "Testing & Quality:"
	@echo "  make test           - Run unit tests"
	@echo "  make lint           - Run code analysis"
	@echo "  make format         - Format code with StyleCop"
	@echo "  make analyze        - Run static analysis"
	@echo ""
	@echo "Deployment:"
	@echo "  make publish        - Publish Release build"
	@echo "  make docker-build   - Build Docker image"
	@echo "  make docker-run     - Run Docker container"
	@echo "  make docker-stop    - Stop Docker container"
	@echo "  make docker-compose - Start services with Docker Compose"
	@echo ""
	@echo "Maintenance:"
	@echo "  make update-deps    - Update NuGet dependencies"
	@echo "  make benchmark      - Run performance benchmarks"
	@echo "  make info           - Show project information"

# Project variables
SOLUTION := dotnet-sqlite-crud-generator.sln
PROJECT := src/DotNet.SQLite.CrudGenerator/DotNet.SQLite.CrudGenerator.csproj
BUILD_CONFIG := Release
OUTPUT_DIR := ./publish
ARTIFACT_DIR := ./artifacts

# Build targets
build: restore
	dotnet build $(SOLUTION) -c $(BUILD_CONFIG)
	@echo "✓ Build completed successfully"

restore:
	dotnet restore $(SOLUTION)
	@echo "✓ Dependencies restored"

clean:
	dotnet clean $(SOLUTION)
	rm -rf $(OUTPUT_DIR) $(ARTIFACT_DIR) bin/ obj/ **/*.dll **/*.exe **/*.pdb
	@echo "✓ Clean completed"

rebuild: clean build

# Development targets
run: build
	dotnet run --project $(PROJECT) --configuration $(BUILD_CONFIG)

dev:
	dotnet watch run --project $(PROJECT)

# Testing targets
test:
	dotnet test $(SOLUTION) -c $(BUILD_CONFIG) --verbosity normal

test-verbose:
	dotnet test $(SOLUTION) -c $(BUILD_CONFIG) --verbosity detailed

test-coverage:
	dotnet test $(SOLUTION) -c $(BUILD_CONFIG) \
		--collect:"XPlat Code Coverage" \
		--logger trx

coverage-report:
	@echo "Code coverage report generated in TestResults/ directory"

# Code quality targets
lint:
	dotnet build $(SOLUTION) -c $(BUILD_CONFIG) /p:TreatWarningsAsErrors=true
	@echo "✓ Linting completed - no warnings or errors"

format:
	dotnet format $(SOLUTION) --verify-no-changes --verbosity diagnostic
	@echo "✓ Code formatting verified"

format-fix:
	dotnet format $(SOLUTION)
	@echo "✓ Code formatted"

analyze:
	dotnet build $(SOLUTION) -c $(BUILD_CONFIG) \
		/p:EnforceCodeStyleInBuild=true \
		/p:AnalysisLevel=latest
	@echo "✓ Static analysis completed"

# Deployment targets
publish: restore
	dotnet publish $(PROJECT) -c $(BUILD_CONFIG) -o $(OUTPUT_DIR)
	@echo "✓ Application published to $(OUTPUT_DIR)"

publish-self-contained: restore
	dotnet publish $(PROJECT) -c $(BUILD_CONFIG) \
		--self-contained \
		--runtime linux-x64 \
		-o $(OUTPUT_DIR)-linux-x64
	dotnet publish $(PROJECT) -c $(BUILD_CONFIG) \
		--self-contained \
		--runtime win-x64 \
		-o $(OUTPUT_DIR)-win-x64
	@echo "✓ Self-contained packages created"

# Docker targets
docker-build:
	docker build -t dotnet-crud-generator:latest -t dotnet-crud-generator:1.2.0 .
	@echo "✓ Docker image built: dotnet-crud-generator:latest"

docker-run:
	docker run -d \
		--name crudgen-app \
		-e DATABASE_PATH=/data/app.db \
		-v crudgen-data:/data \
		-p 5000:5000 \
		dotnet-crud-generator:latest
	@echo "✓ Docker container started: crudgen-app"

docker-stop:
	docker stop crudgen-app 2>/dev/null || true
	docker rm crudgen-app 2>/dev/null || true
	@echo "✓ Docker container stopped and removed"

docker-compose-up:
	docker-compose up -d
	@echo "✓ Docker Compose services started"
	docker-compose logs -f

docker-compose-down:
	docker-compose down
	@echo "✓ Docker Compose services stopped"

docker-compose-logs:
	docker-compose logs -f

# Performance & Maintenance targets
benchmark:
	@echo "Running performance benchmarks..."
	dotnet run -c Release --project $(PROJECT) -- benchmark
	@echo "✓ Benchmarks completed"

update-deps:
	dotnet outdated
	@echo ""
	@echo "To update packages, run: dotnet upgrade"

info:
	@echo "Project Information:"
	@echo "  Solution: $(SOLUTION)"
	@echo "  Project: $(PROJECT)"
	@echo "  Build Config: $(BUILD_CONFIG)"
	@echo "  Output Dir: $(OUTPUT_DIR)"
	@echo ""
	@dotnet --version
	@dotnet --list-sdks

# Continuous Integration targets
ci-build: restore
	dotnet build $(SOLUTION) -c $(BUILD_CONFIG) --no-restore
	@echo "✓ CI build completed"

ci-test:
	dotnet test $(SOLUTION) -c $(BUILD_CONFIG) \
		--no-build \
		--verbosity normal \
		--logger "trx;LogFileName=test-results.trx"
	@echo "✓ CI tests completed"

ci-analyze:
	dotnet build $(SOLUTION) -c $(BUILD_CONFIG) \
		/p:TreatWarningsAsErrors=true \
		/p:EnforceCodeStyleInBuild=true
	@echo "✓ CI analysis completed"

# Git targets
git-clean-local:
	git clean -fdx -e .vs -e bin -e obj
	@echo "✓ Local changes cleaned"

git-status:
	git status --short

# Database targets
db-backup:
	@mkdir -p backups
	@cp crudgenerator.db backups/app_$$(date +%Y%m%d_%H%M%S).db 2>/dev/null || echo "No database file found"
	@echo "✓ Database backed up"

db-reset:
	rm -f crudgenerator.db
	@echo "✓ Database reset"

# Help targets
version:
	@dotnet --version

list-targets:
	@grep "^[a-z].*:" Makefile | cut -d: -f1 | sort

# Error handling
.DEFAULT_GOAL := help
