#!/bin/bash

# Script de démarrage rapide - Installation complète SFTP Copy Tool Web
# Usage: curl -sSL https://raw.githubusercontent.com/74nu5/CopyToNas/main/scripts/quick-install.sh | sudo bash

set -e

# Configuration
REPO_URL="https://github.com/74nu5/CopyToNas.git"
INSTALL_DIR="/tmp/sftpcopy-install"
LOG_FILE="/var/log/sftpcopy-install.log"

# Couleurs
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1" | tee -a "$LOG_FILE"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1" | tee -a "$LOG_FILE"
}

# Fonction de nettoyage en cas d'erreur
cleanup() {
    if [[ -d "$INSTALL_DIR" ]]; then
        rm -rf "$INSTALL_DIR"
    fi
}

trap cleanup EXIT

main() {
    log_info "=== INSTALLATION RAPIDE SFTP COPY TOOL WEB ==="
    log_info "Début de l'installation: $(date)"

    # Vérifier les prérequis
    if [[ $EUID -ne 0 ]]; then
        log_error "Ce script doit être exécuté en tant que root (avec sudo)"
        exit 1
    fi

    # Installer git si nécessaire
    if ! command -v git &> /dev/null; then
        log_info "Installation de git..."
        if command -v apt-get &> /dev/null; then
            apt-get update && apt-get install -y git
        elif command -v yum &> /dev/null; then
            yum install -y git
        elif command -v dnf &> /dev/null; then
            dnf install -y git
        else
            log_error "Impossible d'installer git automatiquement"
            exit 1
        fi
    fi

    # Cloner le repository
    log_info "Clonage du repository..."
    rm -rf "$INSTALL_DIR"
    git clone "$REPO_URL" "$INSTALL_DIR"

    # Naviguer vers le répertoire des scripts
    cd "$INSTALL_DIR/scripts"

    # Rendre les scripts exécutables
    chmod +x *.sh

    # Exécuter l'installation
    log_info "Exécution du script d'installation principal..."
    ./deploy-web.sh

    # Validation
    log_info "Validation de l'installation..."
    ./validate-installation.sh

    # Nettoyage
    cd /
    rm -rf "$INSTALL_DIR"

    log_success "Installation terminée avec succès!"
    log_info "Log complet disponible dans: $LOG_FILE"
}

main "$@"
