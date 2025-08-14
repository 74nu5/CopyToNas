#!/bin/bash

# Script de validation post-installation pour SFTP Copy Tool Web
# Auteur: CopyToNas
# Usage: ./validate-installation.sh

set -e

APP_NAME="sftpcopy-web"
APP_HOME="/opt/${APP_NAME}"
SERVICE_NAME="${APP_NAME}"

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
    echo -e "${GREEN}[✓]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

log_error() {
    echo -e "${RED}[✗]${NC} $1"
}

# Variables de résultat
TESTS_PASSED=0
TESTS_FAILED=0
WARNINGS=0

test_result() {
    if [ $1 -eq 0 ]; then
        log_success "$2"
        ((TESTS_PASSED++))
    else
        log_error "$2"
        ((TESTS_FAILED++))
    fi
}

test_warning() {
    log_warning "$1"
    ((WARNINGS++))
}

# Vérifications
log_info "=== VALIDATION DE L'INSTALLATION SFTP COPY TOOL WEB ==="
echo ""

# Test 1: .NET Runtime
log_info "1. Vérification de .NET Runtime..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version 2>/dev/null | cut -d. -f1,2)
    if [[ "$DOTNET_VERSION" == "9.0" ]]; then
        test_result 0 ".NET 9.0 Runtime installé ($DOTNET_VERSION)"
    else
        test_result 1 ".NET version incorrecte: $DOTNET_VERSION (attendu: 9.0)"
    fi
else
    test_result 1 ".NET Runtime non trouvé"
fi

# Test 2: Utilisateur système
log_info "2. Vérification de l'utilisateur système..."
if id "sftpcopy" &>/dev/null; then
    test_result 0 "Utilisateur 'sftpcopy' existe"
else
    test_result 1 "Utilisateur 'sftpcopy' n'existe pas"
fi

# Test 3: Répertoire d'installation
log_info "3. Vérification des répertoires..."
if [[ -d "$APP_HOME" ]]; then
    test_result 0 "Répertoire d'installation existe: $APP_HOME"

    # Vérifier l'exécutable principal
    if [[ -f "$APP_HOME/SftpCopyTool.Web" ]]; then
        test_result 0 "Exécutable principal trouvé"

        # Vérifier les permissions
        if [[ -x "$APP_HOME/SftpCopyTool.Web" ]]; then
            test_result 0 "Permissions d'exécution correctes"
        else
            test_result 1 "Permissions d'exécution manquantes"
        fi
    else
        test_result 1 "Exécutable principal manquant"
    fi

    # Vérifier le répertoire de logs
    if [[ -d "$APP_HOME/logs" ]]; then
        test_result 0 "Répertoire de logs existe"
    else
        test_result 1 "Répertoire de logs manquant"
    fi
else
    test_result 1 "Répertoire d'installation manquant: $APP_HOME"
fi

# Test 4: Service systemd
log_info "4. Vérification du service systemd..."
if [[ -f "/etc/systemd/system/$SERVICE_NAME.service" ]]; then
    test_result 0 "Fichier de service systemd existe"

    # Vérifier que le service est activé
    if systemctl is-enabled --quiet $SERVICE_NAME; then
        test_result 0 "Service systemd activé"
    else
        test_result 1 "Service systemd non activé"
    fi

    # Vérifier que le service est en cours d'exécution
    if systemctl is-active --quiet $SERVICE_NAME; then
        test_result 0 "Service systemd en cours d'exécution"
    else
        test_result 1 "Service systemd non démarré"
    fi
else
    test_result 1 "Fichier de service systemd manquant"
fi

# Test 5: Connectivité HTTP
log_info "5. Test de connectivité HTTP..."
if command -v curl &> /dev/null; then
    HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000 2>/dev/null || echo "000")
    case $HTTP_STATUS in
        200|302)
            test_result 0 "Application web répond (HTTP $HTTP_STATUS)"
            ;;
        000)
            test_result 1 "Impossible de se connecter à l'application"
            ;;
        *)
            test_warning "HTTP Status inattendu: $HTTP_STATUS"
            ;;
    esac
else
    test_warning "curl non installé, impossible de tester HTTP"
fi

# Test 6: Health Check
log_info "6. Test du Health Check..."
if command -v curl &> /dev/null; then
    HEALTH_RESPONSE=$(curl -s http://localhost:5000/health 2>/dev/null || echo "ERROR")
    if [[ "$HEALTH_RESPONSE" == "Healthy" ]]; then
        test_result 0 "Health check OK"
    else
        test_result 1 "Health check échoué: $HEALTH_RESPONSE"
    fi
else
    test_warning "curl non installé, impossible de tester le health check"
fi

# Test 7: Vérification des ports
log_info "7. Vérification des ports..."
if command -v netstat &> /dev/null; then
    if netstat -tlnp 2>/dev/null | grep -q ":5000 "; then
        test_result 0 "Port 5000 en écoute"
    else
        test_result 1 "Port 5000 n'est pas en écoute"
    fi
elif command -v ss &> /dev/null; then
    if ss -tlnp 2>/dev/null | grep -q ":5000 "; then
        test_result 0 "Port 5000 en écoute"
    else
        test_result 1 "Port 5000 n'est pas en écoute"
    fi
else
    test_warning "netstat/ss non disponibles, impossible de vérifier les ports"
fi

# Test 8: Configuration
log_info "8. Vérification de la configuration..."
CONFIG_FILE="$APP_HOME/appsettings.Production.json"
if [[ -f "$CONFIG_FILE" ]]; then
    test_result 0 "Fichier de configuration Production existe"

    # Vérifier que le fichier est valide JSON
    if command -v jq &> /dev/null; then
        if jq empty "$CONFIG_FILE" 2>/dev/null; then
            test_result 0 "Configuration JSON valide"
        else
            test_result 1 "Configuration JSON invalide"
        fi
    else
        test_warning "jq non installé, impossible de valider le JSON"
    fi
else
    test_result 1 "Fichier de configuration Production manquant"
fi

# Test 9: Logs
log_info "9. Vérification des logs..."
if journalctl -u $SERVICE_NAME --since "1 minute ago" | grep -q "Now listening on"; then
    test_result 0 "Logs de démarrage présents"
else
    test_warning "Logs de démarrage non trouvés dans la dernière minute"
fi

# Test 10: Permissions de sécurité
log_info "10. Vérification des permissions..."
APP_OWNER=$(stat -c %U:%G "$APP_HOME" 2>/dev/null || echo "unknown")
if [[ "$APP_OWNER" == "sftpcopy:sftpcopy" ]]; then
    test_result 0 "Propriétaire du répertoire correct: $APP_OWNER"
else
    test_result 1 "Propriétaire du répertoire incorrect: $APP_OWNER (attendu: sftpcopy:sftpcopy)"
fi

# Résumé
echo ""
log_info "=== RÉSUMÉ DE LA VALIDATION ==="
echo ""

if [[ $TESTS_FAILED -eq 0 ]]; then
    log_success "✅ Tous les tests sont passés ($TESTS_PASSED/$((TESTS_PASSED + TESTS_FAILED)))"
    if [[ $WARNINGS -gt 0 ]]; then
        log_warning "⚠️  $WARNINGS avertissement(s)"
    fi
    echo ""
    log_success "🎉 Installation validée avec succès!"
    echo ""
    log_info "Accès à l'application:"
    log_info "  - Local: http://localhost:5000"
    log_info "  - Health Check: http://localhost:5000/health"
    echo ""
    log_info "Configuration du reverse proxy YARP nécessaire pour l'accès externe"
    log_info "Consultez le fichier: config/yarp-sftpcopy-web.json.template"
    echo ""
    log_info "Scripts de maintenance disponibles:"
    log_info "  - ./monitor-web.sh status"
    log_info "  - ./update-web.sh"
    echo ""
    exit 0
else
    log_error "❌ $TESTS_FAILED test(s) échoué(s) sur $((TESTS_PASSED + TESTS_FAILED))"
    if [[ $WARNINGS -gt 0 ]]; then
        log_warning "⚠️  $WARNINGS avertissement(s)"
    fi
    echo ""
    log_error "🔧 L'installation nécessite des corrections"
    echo ""
    log_info "Pour diagnostiquer les problèmes:"
    log_info "  sudo systemctl status $SERVICE_NAME"
    log_info "  sudo journalctl -u $SERVICE_NAME -n 50"
    log_info "  ./monitor-web.sh health"
    echo ""
    exit 1
fi
