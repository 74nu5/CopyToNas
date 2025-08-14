#!/bin/bash

# Script de monitoring et maintenance pour SFTP Copy Tool Web
# Auteur: CopyToNas
# Usage: ./monitor-web.sh [status|logs|restart|health|cleanup]

APP_NAME="sftpcopy-web"
APP_HOME="/opt/${APP_NAME}"
SERVICE_NAME="${APP_NAME}"
LOG_FILE="${APP_HOME}/logs/monitor.log"

# Couleurs
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
    echo "$(date '+%Y-%m-%d %H:%M:%S') [INFO] $1" >> ${LOG_FILE}
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
    echo "$(date '+%Y-%m-%d %H:%M:%S') [SUCCESS] $1" >> ${LOG_FILE}
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
    echo "$(date '+%Y-%m-%d %H:%M:%S') [WARNING] $1" >> ${LOG_FILE}
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
    echo "$(date '+%Y-%m-%d %H:%M:%S') [ERROR] $1" >> ${LOG_FILE}
}

# Créer le répertoire de logs si nécessaire
mkdir -p "${APP_HOME}/logs" 2>/dev/null || true

# Vérifier le statut du service
check_status() {
    log_info "=== STATUT DU SERVICE ==="

    if systemctl is-active --quiet ${SERVICE_NAME}; then
        log_success "Service ${SERVICE_NAME} est en cours d'exécution"

        # Vérifier la connectivité HTTP
        if curl -s -o /dev/null -w "%{http_code}" http://localhost:5000 | grep -q "200\|302"; then
            log_success "Application web répond correctement"
        else
            log_warning "Application web ne répond pas sur le port 5000"
        fi

        # Informations sur le processus
        PID=$(systemctl show --property MainPID --value ${SERVICE_NAME})
        if [[ "$PID" != "0" ]]; then
            MEMORY=$(ps -o rss= -p $PID | awk '{print $1/1024 " MB"}' 2>/dev/null || echo "N/A")
            CPU=$(ps -o %cpu= -p $PID 2>/dev/null || echo "N/A")
            log_info "PID: $PID, Mémoire: $MEMORY, CPU: $CPU%"
        fi

    else
        log_error "Service ${SERVICE_NAME} n'est pas en cours d'exécution"
        return 1
    fi

    # Statut systemd détaillé
    systemctl status ${SERVICE_NAME} --no-pager -l
}

# Afficher les logs
show_logs() {
    log_info "=== LOGS DU SERVICE ==="

    if [[ -n "${2:-}" ]]; then
        # Nombre de lignes spécifié
        journalctl -u ${SERVICE_NAME} --no-pager -l -n "$2"
    else
        # Par défaut, les 50 dernières lignes
        journalctl -u ${SERVICE_NAME} --no-pager -l -n 50
    fi
}

# Suivre les logs en temps réel
follow_logs() {
    log_info "Suivi des logs en temps réel (Ctrl+C pour arrêter)..."
    journalctl -u ${SERVICE_NAME} -f
}

# Redémarrer le service
restart_service() {
    log_info "Redémarrage du service ${SERVICE_NAME}..."

    systemctl restart ${SERVICE_NAME}
    sleep 3

    if systemctl is-active --quiet ${SERVICE_NAME}; then
        log_success "Service redémarré avec succès"
    else
        log_error "Échec du redémarrage du service"
        return 1
    fi
}

# Test de santé complet
health_check() {
    log_info "=== VÉRIFICATION DE SANTÉ ==="

    # Vérifier le service
    if ! systemctl is-active --quiet ${SERVICE_NAME}; then
        log_error "Service non actif"
        return 1
    fi

    # Vérifier la connectivité HTTP
    HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000 || echo "000")
    case $HTTP_STATUS in
        200|302)
            log_success "HTTP OK ($HTTP_STATUS)"
            ;;
        000)
            log_error "Impossible de se connecter à l'application"
            return 1
            ;;
        *)
            log_warning "HTTP Status inattendu: $HTTP_STATUS"
            ;;
    esac

    # Vérifier l'espace disque
    DISK_USAGE=$(df ${APP_HOME} | awk 'NR==2 {print $5}' | sed 's/%//')
    if [[ $DISK_USAGE -gt 90 ]]; then
        log_error "Espace disque critique: ${DISK_USAGE}%"
    elif [[ $DISK_USAGE -gt 80 ]]; then
        log_warning "Espace disque élevé: ${DISK_USAGE}%"
    else
        log_success "Espace disque OK: ${DISK_USAGE}%"
    fi

    # Vérifier la mémoire
    if command -v free &> /dev/null; then
        MEMORY_USAGE=$(free | awk 'NR==2{printf "%.0f", $3*100/$2}')
        if [[ $MEMORY_USAGE -gt 90 ]]; then
            log_error "Utilisation mémoire critique: ${MEMORY_USAGE}%"
        elif [[ $MEMORY_USAGE -gt 80 ]]; then
            log_warning "Utilisation mémoire élevée: ${MEMORY_USAGE}%"
        else
            log_success "Utilisation mémoire OK: ${MEMORY_USAGE}%"
        fi
    fi

    # Vérifier les logs d'erreur récents
    ERROR_COUNT=$(journalctl -u ${SERVICE_NAME} --since "1 hour ago" | grep -i error | wc -l)
    if [[ $ERROR_COUNT -gt 0 ]]; then
        log_warning "${ERROR_COUNT} erreur(s) dans la dernière heure"
    else
        log_success "Aucune erreur dans la dernière heure"
    fi

    log_success "Vérification de santé terminée"
}

# Nettoyer les fichiers temporaires et logs anciens
cleanup() {
    log_info "=== NETTOYAGE ==="

    # Nettoyer les logs anciens (plus de 30 jours)
    if [[ -d "${APP_HOME}/logs" ]]; then
        find "${APP_HOME}/logs" -name "*.log" -type f -mtime +30 -delete 2>/dev/null || true
        log_info "Logs anciens supprimés"
    fi

    # Nettoyer les journalctl (garder seulement 7 jours)
    journalctl --vacuum-time=7d

    # Nettoyer les sauvegardes anciennes
    BACKUP_DIR="/opt/${APP_NAME}-backups"
    if [[ -d "${BACKUP_DIR}" ]]; then
        find "${BACKUP_DIR}" -name "backup-*.tar.gz" -type f -mtime +30 -delete 2>/dev/null || true
        log_info "Sauvegardes anciennes supprimées"
    fi

    log_success "Nettoyage terminé"
}

# Afficher les métriques de performance
show_metrics() {
    log_info "=== MÉTRIQUES DE PERFORMANCE ==="

    # Temps de fonctionnement du service
    UPTIME=$(systemctl show --property ActiveEnterTimestamp --value ${SERVICE_NAME})
    if [[ -n "$UPTIME" ]]; then
        log_info "Démarré le: $UPTIME"
    fi

    # Statistiques du processus
    PID=$(systemctl show --property MainPID --value ${SERVICE_NAME})
    if [[ "$PID" != "0" ]]; then
        log_info "Informations du processus (PID: $PID):"
        ps -p $PID -o pid,ppid,cmd,%mem,%cpu,etime 2>/dev/null || log_warning "Impossible de récupérer les stats du processus"
    fi

    # Statistiques réseau (connexions sur le port 5000)
    if command -v netstat &> /dev/null; then
        CONNECTIONS=$(netstat -an | grep :5000 | wc -l)
        log_info "Connexions actives sur le port 5000: $CONNECTIONS"
    fi

    # Statistiques des logs
    LOG_LINES=$(journalctl -u ${SERVICE_NAME} --since "today" | wc -l)
    log_info "Lignes de logs aujourd'hui: $LOG_LINES"
}

# Fonction d'aide
show_help() {
    echo "Usage: $0 [COMMAND] [OPTIONS]"
    echo ""
    echo "COMMANDS:"
    echo "  status          Afficher le statut du service"
    echo "  logs [N]        Afficher les N dernières lignes de logs (défaut: 50)"
    echo "  follow          Suivre les logs en temps réel"
    echo "  restart         Redémarrer le service"
    echo "  health          Effectuer un test de santé complet"
    echo "  cleanup         Nettoyer les fichiers temporaires et logs anciens"
    echo "  metrics         Afficher les métriques de performance"
    echo "  help            Afficher cette aide"
    echo ""
    echo "Exemples:"
    echo "  $0 status"
    echo "  $0 logs 100"
    echo "  $0 health"
}

# Traitement des arguments
case "${1:-status}" in
    "status")
        check_status
        ;;
    "logs")
        show_logs "$@"
        ;;
    "follow")
        follow_logs
        ;;
    "restart")
        restart_service
        ;;
    "health")
        health_check
        ;;
    "cleanup")
        cleanup
        ;;
    "metrics")
        show_metrics
        ;;
    "help"|"-h"|"--help")
        show_help
        ;;
    *)
        log_error "Commande inconnue: $1"
        show_help
        exit 1
        ;;
esac
