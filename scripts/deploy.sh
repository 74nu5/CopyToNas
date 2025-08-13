#!/bin/bash

# Script de déploiement automatisé pour SftpCopyTool
# Usage: ./deploy.sh [options]

set -e

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration par défaut
INSTALL_DIR="/opt/sftpcopy"
BIN_NAME="sftp-copy-tool"
BIN_DIR="/usr/local/bin"
CONFIG_DIR="/etc/sftp-copy"
SERVICE_NAME="sftp-copy"
PUBLISH_DIR="./publish"
CREATE_SERVICE="true"

# Fonction d'aide
show_help() {
    echo -e "${BLUE}Script de déploiement pour SftpCopyTool${NC}"
    echo
    echo "Usage: $0 [options]"
    echo
    echo "Options:"
    echo "  --install-dir DIR         Répertoire d'installation [défaut: $INSTALL_DIR]"
    echo "  --bin-name NAME           Nom de l'exécutable global [défaut: $BIN_NAME]"
    echo "  --bin-dir DIR             Répertoire des binaires [défaut: $BIN_DIR]"
    echo "  --config-dir DIR          Répertoire de configuration [défaut: $CONFIG_DIR]"
    echo "  --publish-dir DIR         Répertoire source de publication [défaut: $PUBLISH_DIR]"
    echo "  --no-service              Ne pas créer le service systemd"
    echo "  --uninstall               Désinstaller au lieu d'installer"
    echo "  -h, --help               Affiche cette aide"
    echo
    echo "Exemples:"
    echo "  $0                        # Déploiement standard"
    echo "  $0 --no-service           # Déploiement sans service systemd"
    echo "  $0 --uninstall           # Désinstallation"
}

# Fonction de logging
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_step() {
    echo -e "${BLUE}[STEP]${NC} $1"
}

# Fonction de désinstallation
uninstall() {
    log_step "Désinstallation de SftpCopyTool..."
    
    # Arrêter et désactiver le service
    if systemctl is-active --quiet "$SERVICE_NAME.service" 2>/dev/null; then
        log_info "Arrêt du service $SERVICE_NAME..."
        sudo systemctl stop "$SERVICE_NAME.service"
    fi
    
    if systemctl is-enabled --quiet "$SERVICE_NAME.service" 2>/dev/null; then
        log_info "Désactivation du service $SERVICE_NAME..."
        sudo systemctl disable "$SERVICE_NAME.service"
    fi
    
    # Supprimer le fichier de service
    if [[ -f "/etc/systemd/system/$SERVICE_NAME.service" ]]; then
        log_info "Suppression du service systemd..."
        sudo rm -f "/etc/systemd/system/$SERVICE_NAME.service"
        sudo systemctl daemon-reload
    fi
    
    # Supprimer l'exécutable global
    if [[ -f "$BIN_DIR/$BIN_NAME" ]]; then
        log_info "Suppression de l'exécutable global..."
        sudo rm -f "$BIN_DIR/$BIN_NAME"
    fi
    
    # Supprimer le répertoire d'installation
    if [[ -d "$INSTALL_DIR" ]]; then
        log_info "Suppression du répertoire d'installation..."
        sudo rm -rf "$INSTALL_DIR"
    fi
    
    # Demander pour la configuration
    if [[ -d "$CONFIG_DIR" ]]; then
        echo
        read -p "Supprimer la configuration dans $CONFIG_DIR ? [y/N] " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            sudo rm -rf "$CONFIG_DIR"
            log_info "Configuration supprimée"
        else
            log_info "Configuration conservée"
        fi
    fi
    
    log_info "✅ Désinstallation terminée !"
    exit 0
}

# Parse des arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --install-dir)
            INSTALL_DIR="$2"
            shift 2
            ;;
        --bin-name)
            BIN_NAME="$2"
            shift 2
            ;;
        --bin-dir)
            BIN_DIR="$2"
            shift 2
            ;;
        --config-dir)
            CONFIG_DIR="$2"
            shift 2
            ;;
        --publish-dir)
            PUBLISH_DIR="$2"
            shift 2
            ;;
        --no-service)
            CREATE_SERVICE="false"
            shift
            ;;
        --uninstall)
            uninstall
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            log_error "Option inconnue: $1"
            show_help
            exit 1
            ;;
    esac
done

# Vérifier les privilèges root
if [[ $EUID -ne 0 ]]; then
    log_error "Ce script doit être exécuté avec sudo"
    exit 1
fi

# Vérifier que l'application a été publiée
if [[ ! -f "$PUBLISH_DIR/SftpCopyTool" ]]; then
    log_error "Application non trouvée dans $PUBLISH_DIR/"
    log_error "Exécutez d'abord: ./build.sh"
    exit 1
fi

log_info "Configuration de déploiement:"
log_info "  Répertoire d'installation: $INSTALL_DIR"
log_info "  Nom de l'exécutable: $BIN_NAME"
log_info "  Répertoire des binaires: $BIN_DIR"
log_info "  Répertoire de configuration: $CONFIG_DIR"
log_info "  Créer le service systemd: $CREATE_SERVICE"
echo

# Créer les répertoires
log_step "Création des répertoires..."
mkdir -p "$INSTALL_DIR"
mkdir -p "$CONFIG_DIR"

# Copier l'application
log_step "Installation de l'application..."
cp -r "$PUBLISH_DIR"/* "$INSTALL_DIR/"
chmod +x "$INSTALL_DIR/SftpCopyTool"

# Créer l'exécutable global
log_step "Création de l'exécutable global..."
ln -sf "$INSTALL_DIR/SftpCopyTool" "$BIN_DIR/$BIN_NAME"

# Copier la configuration si elle n'existe pas
if [[ ! -f "$CONFIG_DIR/config.env" ]] && [[ -f "../config/config.env" ]]; then
    log_step "Installation de la configuration par défaut..."
    cp ../config/config.env "$CONFIG_DIR/"
    chmod 600 "$CONFIG_DIR/config.env"
    chown root:root "$CONFIG_DIR/config.env"
fi

# Créer le service systemd si demandé
if [[ "$CREATE_SERVICE" == "true" ]]; then
    log_step "Création du service systemd..."
    
    cat > "/etc/systemd/system/$SERVICE_NAME.service" << EOF
[Unit]
Description=SFTP Copy Tool
After=network.target

[Service]
Type=oneshot
User=root
WorkingDirectory=$INSTALL_DIR
EnvironmentFile=$CONFIG_DIR/config.env
ExecStart=$INSTALL_DIR/SftpCopyTool --host \${SFTP_HOST} --port \${SFTP_PORT} --username \${SFTP_USERNAME} --password \${SFTP_PASSWORD} --remote-path \${REMOTE_PATH} --local-path \${LOCAL_PATH} \${RECURSIVE_MODE}
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
EOF
    
    systemctl daemon-reload
    log_info "Service $SERVICE_NAME.service créé"
fi

# Afficher les informations finales
log_info "✅ Déploiement réussi !"
echo
log_info "Utilisation:"
log_info "  Exécution directe: $BIN_NAME --help"
log_info "  Configuration: $CONFIG_DIR/config.env"

if [[ "$CREATE_SERVICE" == "true" ]]; then
    echo
    log_info "Service systemd:"
    log_info "  Activer: systemctl enable $SERVICE_NAME.service"
    log_info "  Démarrer: systemctl start $SERVICE_NAME.service"
    log_info "  Statut: systemctl status $SERVICE_NAME.service"
    log_info "  Logs: journalctl -u $SERVICE_NAME.service -f"
fi

echo
log_warn "N'oubliez pas de configurer $CONFIG_DIR/config.env avec vos paramètres SFTP !"
