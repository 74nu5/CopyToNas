#!/bin/bash

# Script de mise à jour de l'interface web SFTP Copy Tool
# Auteur: CopyToNas
# Date: $(date +"%Y-%m-%d")

set -e

# Variables
APP_NAME="sftpcopy-web"
APP_HOME="/opt/${APP_NAME}"
SERVICE_NAME="${APP_NAME}"
BACKUP_DIR="/opt/${APP_NAME}-backups"

# Couleurs
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

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
    if [[ $EUID -ne 0 ]]; then
        log_error "Ce script doit être exécuté en tant que root (avec sudo)"
        exit 1
    fi

    if [[ ! -d ${APP_HOME} ]]; then
        log_error "Application non trouvée dans ${APP_HOME}"
        log_info "Veuillez d'abord exécuter deploy-web.sh"
        exit 1
    fi
}

# Créer une sauvegarde
create_backup() {
    log_info "Création d'une sauvegarde..."

    mkdir -p ${BACKUP_DIR}
    BACKUP_FILE="${BACKUP_DIR}/backup-$(date +%Y%m%d-%H%M%S).tar.gz"

    tar -czf ${BACKUP_FILE} -C ${APP_HOME} . 2>/dev/null || {
        log_error "Échec de la création de la sauvegarde"
        exit 1
    }

    log_success "Sauvegarde créée: ${BACKUP_FILE}"

    # Garder seulement les 5 dernières sauvegardes
    find ${BACKUP_DIR} -name "backup-*.tar.gz" -type f -mtime +7 -delete 2>/dev/null || true
}

# Compiler la nouvelle version
build_app() {
    log_info "Compilation de la nouvelle version..."

    cd ../web

    # Nettoyer les anciens builds
    rm -rf bin/Release/net9.0/publish 2>/dev/null || true

    # Compiler et publier
    dotnet publish -c Release -o bin/Release/net9.0/publish --self-contained false

    if [[ ! -f "bin/Release/net9.0/publish/SftpCopyTool.Web" ]]; then
        log_error "Échec de la compilation"
        exit 1
    fi

    cd ../scripts
    log_success "Compilation terminée"
}

# Mettre à jour l'application
update_app() {
    log_info "Mise à jour de l'application..."

    # Arrêter le service
    log_info "Arrêt du service..."
    systemctl stop ${SERVICE_NAME}

    # Copier les nouveaux fichiers
    log_info "Copie des nouveaux fichiers..."
    cp -r ../web/bin/Release/net9.0/publish/* ${APP_HOME}/

    # Restaurer les permissions
    chown -R sftpcopy:sftpcopy ${APP_HOME}
    chmod +x ${APP_HOME}/SftpCopyTool.Web

    # Redémarrer le service
    log_info "Redémarrage du service..."
    systemctl start ${SERVICE_NAME}

    sleep 3

    if systemctl is-active --quiet ${SERVICE_NAME}; then
        log_success "Service redémarré avec succès"
    else
        log_error "Échec du redémarrage du service"
        log_info "Tentative de restauration de la sauvegarde..."
        restore_backup
        exit 1
    fi
}

# Restaurer la sauvegarde en cas de problème
restore_backup() {
    log_warning "Restauration de la dernière sauvegarde..."

    LATEST_BACKUP=$(find ${BACKUP_DIR} -name "backup-*.tar.gz" -type f -printf '%T@ %p\n' 2>/dev/null | sort -n | tail -1 | cut -d' ' -f2-)

    if [[ -n "${LATEST_BACKUP}" ]]; then
        systemctl stop ${SERVICE_NAME}
        rm -rf ${APP_HOME}/*
        tar -xzf ${LATEST_BACKUP} -C ${APP_HOME}
        chown -R sftpcopy:sftpcopy ${APP_HOME}
        chmod +x ${APP_HOME}/SftpCopyTool.Web
        systemctl start ${SERVICE_NAME}
        log_warning "Sauvegarde restaurée: ${LATEST_BACKUP}"
    else
        log_error "Aucune sauvegarde trouvée"
    fi
}

# Afficher le statut
show_status() {
    log_info "=== STATUT DU SERVICE ==="
    systemctl status ${SERVICE_NAME} --no-pager -l
    echo ""
    log_info "Dernières lignes des logs:"
    journalctl -u ${SERVICE_NAME} --no-pager -l -n 10
}

# Fonction principale
main() {
    log_info "=== MISE À JOUR SFTP COPY TOOL WEB ==="

    check_prerequisites
    create_backup
    build_app
    update_app
    show_status

    log_success "Mise à jour terminée avec succès!"
}

# Gestion des arguments
case "${1:-}" in
    "backup")
        check_prerequisites
        create_backup
        ;;
    "restore")
        check_prerequisites
        restore_backup
        ;;
    "status")
        show_status
        ;;
    *)
        main "$@"
        ;;
esac
