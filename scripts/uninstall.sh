#!/bin/bash

# Script de désinstallation pour SftpCopyTool
# Exécutez avec : chmod +x uninstall.sh && sudo ./uninstall.sh

set -e

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Fonction d'affichage de messages
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Vérifier si on est root
if [[ $EUID -ne 0 ]]; then
   log_error "Ce script doit être exécuté en tant que root (utilisez sudo)"
   exit 1
fi

log_info "Désinstallation de SftpCopyTool..."

# Définir les chemins
INSTALL_DIR="/opt/sftpcopy"
SERVICE_FILE="/etc/systemd/system/sftp-copy.service"
CONFIG_DIR="/etc/sftp-copy"
SYMLINK="/usr/local/bin/sftp-copy"

# Arrêter et désactiver le service
if systemctl is-active --quiet sftp-copy.service; then
    log_info "Arrêt du service sftp-copy..."
    systemctl stop sftp-copy.service
fi

if systemctl is-enabled --quiet sftp-copy.service; then
    log_info "Désactivation du service sftp-copy..."
    systemctl disable sftp-copy.service
fi

# Supprimer le fichier de service
if [[ -f "$SERVICE_FILE" ]]; then
    log_info "Suppression du service systemd..."
    rm -f "$SERVICE_FILE"
    systemctl daemon-reload
fi

# Supprimer le lien symbolique
if [[ -L "$SYMLINK" ]]; then
    log_info "Suppression du lien symbolique..."
    rm -f "$SYMLINK"
fi

# Supprimer les dossiers d'installation
if [[ -d "$INSTALL_DIR" ]]; then
    log_info "Suppression du dossier d'installation..."
    rm -rf "$INSTALL_DIR"
fi

# Demander avant de supprimer la configuration
if [[ -d "$CONFIG_DIR" ]]; then
    echo
    read -p "Voulez-vous supprimer la configuration dans $CONFIG_DIR ? [y/N] " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        log_info "Suppression de la configuration..."
        rm -rf "$CONFIG_DIR"
    else
        log_info "Configuration conservée dans $CONFIG_DIR"
    fi
fi

log_info "Désinstallation terminée !"
log_warn "Si vous avez installé .NET uniquement pour SftpCopyTool, vous pouvez le désinstaller manuellement."
