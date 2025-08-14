#!/bin/bash

# Script de déploiement de l'interface web SFTP Copy Tool
# Auteur: CopyToNas
# Date: $(date +"%Y-%m-%d")

set -e  # Arrêter le script en cas d'erreur

# Variables de configuration
APP_NAME="sftpcopy-web"
APP_USER="sftpcopy"
APP_GROUP="sftpcopy"
APP_HOME="/opt/${APP_NAME}"
SERVICE_NAME="${APP_NAME}"
DOTNET_VERSION="9.0"
PORT="5000"
ENVIRONMENT="Production"

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Fonction pour afficher des messages colorés
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Vérifier les prérequis
check_prerequisites() {
    log_info "Vérification des prérequis..."

    # Vérifier si on est root
    if [[ $EUID -ne 0 ]]; then
        log_error "Ce script doit être exécuté en tant que root (avec sudo)"
        exit 1
    fi

    # Vérifier la distribution Linux
    if ! command -v systemctl &> /dev/null; then
        log_error "systemctl n'est pas disponible. Ce script nécessite systemd"
        exit 1
    fi

    log_success "Prérequis vérifiés"
}

# Installer .NET Runtime si nécessaire
install_dotnet() {
    log_info "Vérification de .NET ${DOTNET_VERSION}..."

    if command -v dotnet &> /dev/null; then
        CURRENT_VERSION=$(dotnet --version | cut -d. -f1,2)
        if [[ "$CURRENT_VERSION" == "$DOTNET_VERSION" ]]; then
            log_success ".NET ${DOTNET_VERSION} est déjà installé"
            return 0
        fi
    fi

    log_info "Installation de .NET ${DOTNET_VERSION}..."

    # Détecter la distribution
    if [[ -f /etc/debian_version ]]; then
        # Debian/Ubuntu
        wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
        dpkg -i packages-microsoft-prod.deb
        rm packages-microsoft-prod.deb

        apt-get update
        apt-get install -y aspnetcore-runtime-${DOTNET_VERSION}

    elif [[ -f /etc/redhat-release ]]; then
        # CentOS/RHEL/Fedora
        rpm --import https://packages.microsoft.com/keys/microsoft.asc
        wget -O /etc/yum.repos.d/microsoft-prod.repo https://packages.microsoft.com/config/rhel/8/prod.repo

        if command -v dnf &> /dev/null; then
            dnf install -y aspnetcore-runtime-${DOTNET_VERSION}
        else
            yum install -y aspnetcore-runtime-${DOTNET_VERSION}
        fi

    else
        log_error "Distribution Linux non supportée automatiquement"
        log_info "Veuillez installer .NET ${DOTNET_VERSION} Runtime manuellement"
        log_info "https://learn.microsoft.com/dotnet/core/install/linux"
        exit 1
    fi

    log_success ".NET ${DOTNET_VERSION} installé"
}

# Créer l'utilisateur système
create_user() {
    log_info "Création de l'utilisateur système ${APP_USER}..."

    # Créer le groupe s'il n'existe pas
    if ! getent group "$APP_GROUP" > /dev/null 2>&1; then
        groupadd --system "$APP_GROUP"
        log_success "Groupe ${APP_GROUP} créé"
    else
        log_warning "Le groupe ${APP_GROUP} existe déjà"
    fi

    # Créer l'utilisateur s'il n'existe pas
    if id "$APP_USER" &>/dev/null; then
        log_warning "L'utilisateur ${APP_USER} existe déjà"
    else
        useradd --system --no-create-home --shell /bin/false --gid "$APP_GROUP" "$APP_USER"
        log_success "Utilisateur ${APP_USER} créé"
    fi
}

# Créer les répertoires
create_directories() {
    log_info "Création des répertoires..."

    mkdir -p ${APP_HOME}
    mkdir -p ${APP_HOME}/logs
    mkdir -p /etc/systemd/system

    # Permissions
    chown -R ${APP_USER}:${APP_GROUP} ${APP_HOME}
    chmod 755 ${APP_HOME}
    chmod 755 ${APP_HOME}/logs

    log_success "Répertoires créés"
}

# Déployer l'application
deploy_app() {
    log_info "Déploiement de l'application..."

    # Vérifier si le répertoire de publication existe
    if [[ ! -d "../web/bin/Release/net9.0/publish" ]]; then
        log_warning "Répertoire de publication non trouvé, compilation en cours..."
        cd ../web
        dotnet publish -c Release -o bin/Release/net9.0/publish --self-contained false
        cd ../scripts
    fi

    # Arrêter le service s'il est en cours d'exécution
    if systemctl is-active --quiet ${SERVICE_NAME}; then
        log_info "Arrêt du service ${SERVICE_NAME}..."
        systemctl stop ${SERVICE_NAME}
    fi

    # Copier les fichiers
    log_info "Copie des fichiers d'application..."
    cp -r ../web/bin/Release/net9.0/publish/* ${APP_HOME}/

    # Permissions
    chown -R ${APP_USER}:${APP_GROUP} ${APP_HOME}
    chmod +x ${APP_HOME}/SftpCopyTool.Web

    log_success "Application déployée"
}

# Créer le fichier de configuration
create_config() {
    log_info "Création du fichier de configuration..."

    cat > ${APP_HOME}/appsettings.Production.json << EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "Console": {
      "FormatterName": "simple",
      "FormatterOptions": {
        "SingleLine": true,
        "TimestampFormat": "HH:mm:ss "
      }
    }
  },
  "AllowedHosts": "*",
  "Urls": "http://localhost:${PORT}",
  "Environment": "${ENVIRONMENT}"
}
EOF

    chown ${APP_USER}:${APP_GROUP} ${APP_HOME}/appsettings.Production.json
    chmod 644 ${APP_HOME}/appsettings.Production.json

    log_success "Fichier de configuration créé"
}

# Créer le service systemd
create_service() {
    log_info "Création du service systemd..."

    cat > /etc/systemd/system/${SERVICE_NAME}.service << EOF
[Unit]
Description=SFTP Copy Tool Web Interface
After=network.target

[Service]
Type=notify
ExecStart=${APP_HOME}/SftpCopyTool.Web
Restart=always
RestartSec=5
TimeoutStopSec=20
SyslogIdentifier=${SERVICE_NAME}
User=${APP_USER}
Group=${APP_GROUP}
WorkingDirectory=${APP_HOME}
Environment=ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}
Environment=ASPNETCORE_URLS=http://localhost:${PORT}
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=DOTNET_USE_POLLING_FILE_WATCHER=true

# Limites de sécurité
PrivateTmp=true
ProtectSystem=strict
ReadWritePaths=${APP_HOME}/logs
NoNewPrivileges=true
PrivateDevices=true
ProtectHome=true
ProtectKernelTunables=true
ProtectKernelModules=true
ProtectControlGroups=true

[Install]
WantedBy=multi-user.target
EOF

    # Recharger systemd et activer le service
    systemctl daemon-reload
    systemctl enable ${SERVICE_NAME}

    log_success "Service systemd créé et activé"
}

# Configurer le pare-feu si nécessaire
configure_firewall() {
    log_info "Configuration du pare-feu..."

    # Pour ufw (Ubuntu)
    if command -v ufw &> /dev/null; then
        ufw allow ${PORT}/tcp comment "SFTP Copy Tool Web"
        log_info "Port ${PORT} autorisé dans ufw"
    fi

    # Pour firewalld (CentOS/RHEL)
    if command -v firewall-cmd &> /dev/null; then
        firewall-cmd --permanent --add-port=${PORT}/tcp
        firewall-cmd --reload
        log_info "Port ${PORT} autorisé dans firewalld"
    fi

    log_warning "Vérifiez que votre pare-feu autorise le port ${PORT}"
}

# Démarrer le service
start_service() {
    log_info "Démarrage du service ${SERVICE_NAME}..."

    systemctl start ${SERVICE_NAME}
    sleep 3

    if systemctl is-active --quiet ${SERVICE_NAME}; then
        log_success "Service ${SERVICE_NAME} démarré avec succès"
        systemctl status ${SERVICE_NAME} --no-pager -l
    else
        log_error "Échec du démarrage du service"
        log_info "Logs du service:"
        journalctl -u ${SERVICE_NAME} --no-pager -l -n 20
        exit 1
    fi
}

# Afficher les informations finales
show_info() {
    log_success "=== INSTALLATION TERMINÉE ==="
    echo ""
    log_info "Application installée dans: ${APP_HOME}"
    log_info "Service systemd: ${SERVICE_NAME}"
    log_info "Port d'écoute: ${PORT}"
    log_info "Utilisateur: ${APP_USER}"
    echo ""
    log_info "Commandes utiles:"
    echo "  Statut:     sudo systemctl status ${SERVICE_NAME}"
    echo "  Redémarrer: sudo systemctl restart ${SERVICE_NAME}"
    echo "  Arrêter:    sudo systemctl stop ${SERVICE_NAME}"
    echo "  Logs:       sudo journalctl -u ${SERVICE_NAME} -f"
    echo ""
    log_warning "N'oubliez pas de configurer votre reverse proxy YARP pour pointer vers http://localhost:${PORT}"
    echo ""
}

# Fonction principale
main() {
    log_info "=== DÉBUT DU DÉPLOIEMENT SFTP COPY TOOL WEB ==="

    check_prerequisites
    install_dotnet
    create_user
    create_directories
    deploy_app
    create_config
    create_service
    configure_firewall
    start_service
    show_info

    log_success "Déploiement terminé avec succès!"
}

# Exécution du script principal
main "$@"
