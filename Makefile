# Makefile pour SFTP Copy Tool Web Interface
# Usage: make [target]

.PHONY: help build publish deploy update monitor clean

# Variables
PROJECT_DIR = web
SCRIPTS_DIR = scripts
OUTPUT_DIR = $(PROJECT_DIR)/bin/Release/net9.0/publish
DOTNET = dotnet

# Couleurs pour les messages
COLOR_RESET = \033[0m
COLOR_BLUE = \033[34m
COLOR_GREEN = \033[32m
COLOR_YELLOW = \033[33m

# Cible par défaut
help:
	@echo "$(COLOR_BLUE)SFTP Copy Tool Web - Commandes disponibles:$(COLOR_RESET)"
	@echo ""
	@echo "  $(COLOR_GREEN)build$(COLOR_RESET)      - Compiler l'application en mode Release"
	@echo "  $(COLOR_GREEN)publish$(COLOR_RESET)    - Publier l'application (prête pour le déploiement)"
	@echo "  $(COLOR_GREEN)deploy$(COLOR_RESET)     - Déployer sur le serveur (nécessite sudo sur Linux)"
	@echo "  $(COLOR_GREEN)update$(COLOR_RESET)     - Mettre à jour l'application déployée"
	@echo "  $(COLOR_GREEN)monitor$(COLOR_RESET)    - Surveiller l'application déployée"
	@echo "  $(COLOR_GREEN)validate$(COLOR_RESET)   - Valider l'installation"
	@echo "  $(COLOR_GREEN)clean$(COLOR_RESET)      - Nettoyer les fichiers de build"
	@echo "  $(COLOR_GREEN)scripts$(COLOR_RESET)    - Rendre les scripts bash exécutables"
	@echo ""
	@echo "$(COLOR_YELLOW)Exemples:$(COLOR_RESET)"
	@echo "  make build"
	@echo "  make publish"
	@echo "  sudo make deploy"

# Compiler l'application
build:
	@echo "$(COLOR_BLUE)🔨 Compilation de l'application...$(COLOR_RESET)"
	cd $(PROJECT_DIR) && $(DOTNET) build -c Release
	@echo "$(COLOR_GREEN)✅ Compilation terminée$(COLOR_RESET)"

# Publier l'application
publish: build
	@echo "$(COLOR_BLUE)📦 Publication de l'application...$(COLOR_RESET)"
	cd $(PROJECT_DIR) && $(DOTNET) publish -c Release -o bin/Release/net9.0/publish --self-contained false
	@echo "$(COLOR_GREEN)✅ Publication terminée dans $(OUTPUT_DIR)$(COLOR_RESET)"

# Déployer l'application (Linux uniquement)
deploy: scripts
	@echo "$(COLOR_BLUE)🚀 Déploiement de l'application...$(COLOR_RESET)"
	@if [ "$$(uname)" = "Linux" ]; then \
		cd $(SCRIPTS_DIR) && ./deploy-web.sh; \
	else \
		echo "$(COLOR_YELLOW)⚠️  Le déploiement n'est disponible que sur Linux$(COLOR_RESET)"; \
		echo "Utilisez WSL ou transférez les fichiers sur votre serveur Linux"; \
	fi

# Mettre à jour l'application déployée (Linux uniquement)
update: scripts
	@echo "$(COLOR_BLUE)🔄 Mise à jour de l'application...$(COLOR_RESET)"
	@if [ "$$(uname)" = "Linux" ]; then \
		cd $(SCRIPTS_DIR) && ./update-web.sh; \
	else \
		echo "$(COLOR_YELLOW)⚠️  La mise à jour n'est disponible que sur Linux$(COLOR_RESET)"; \
	fi

# Surveiller l'application (Linux uniquement)
monitor: scripts
	@echo "$(COLOR_BLUE)📊 Surveillance de l'application...$(COLOR_RESET)"
	@if [ "$$(uname)" = "Linux" ]; then \
		cd $(SCRIPTS_DIR) && ./monitor-web.sh status; \
	else \
		echo "$(COLOR_YELLOW)⚠️  La surveillance n'est disponible que sur Linux$(COLOR_RESET)"; \
	fi

# Valider l'installation (Linux uniquement)
validate: scripts
	@echo "$(COLOR_BLUE)✅ Validation de l'installation...$(COLOR_RESET)"
	@if [ "$$(uname)" = "Linux" ]; then \
		cd $(SCRIPTS_DIR) && ./validate-installation.sh; \
	else \
		echo "$(COLOR_YELLOW)⚠️  La validation n'est disponible que sur Linux$(COLOR_RESET)"; \
	fi

# Nettoyer les fichiers de build
clean:
	@echo "$(COLOR_BLUE)🧹 Nettoyage des fichiers de build...$(COLOR_RESET)"
	cd $(PROJECT_DIR) && $(DOTNET) clean
	rm -rf $(PROJECT_DIR)/bin/Release
	rm -rf $(PROJECT_DIR)/obj
	@echo "$(COLOR_GREEN)✅ Nettoyage terminé$(COLOR_RESET)"

# Rendre les scripts bash exécutables (Linux/macOS uniquement)
scripts:
	@echo "$(COLOR_BLUE)🔧 Configuration des scripts...$(COLOR_RESET)"
	@if [ "$$(uname)" != "Windows_NT" ] && [ -z "$$OS" ]; then \
		chmod +x $(SCRIPTS_DIR)/*.sh; \
		echo "$(COLOR_GREEN)✅ Scripts rendus exécutables$(COLOR_RESET)"; \
	else \
		echo "$(COLOR_YELLOW)⚠️  Les permissions seront définies lors du déploiement sur Linux$(COLOR_RESET)"; \
	fi

# Commandes de développement local
dev:
	@echo "$(COLOR_BLUE)🖥️  Démarrage en mode développement...$(COLOR_RESET)"
	cd $(PROJECT_DIR) && $(DOTNET) run

dev-watch:
	@echo "$(COLOR_BLUE)👀 Démarrage avec rechargement automatique...$(COLOR_RESET)"
	cd $(PROJECT_DIR) && $(DOTNET) watch run

# Tests
test:
	@echo "$(COLOR_BLUE)🧪 Exécution des tests...$(COLOR_RESET)"
	$(DOTNET) test

# Informations sur le projet
info:
	@echo "$(COLOR_BLUE)ℹ️  Informations sur le projet:$(COLOR_RESET)"
	@echo ""
	@echo "  Projet: SFTP Copy Tool Web Interface"
	@echo "  Framework: .NET 9.0"
	@echo "  Type: Blazor Server"
	@echo "  Répertoire: $(PROJECT_DIR)"
	@echo "  Scripts: $(SCRIPTS_DIR)"
	@echo ""
	@echo "$(COLOR_GREEN)Fichiers importants:$(COLOR_RESET)"
	@echo "  - $(PROJECT_DIR)/Program.cs"
	@echo "  - $(PROJECT_DIR)/Pages/Index.razor"
	@echo "  - $(PROJECT_DIR)/Services/"
	@echo "  - $(SCRIPTS_DIR)/deploy-web.sh"
	@echo "  - docs/WEB_INSTALLATION.md"
