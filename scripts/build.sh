#!/bin/bash

# Script de compilation et publication pour SftpCopyTool
# Usage: ./build.sh [options]

set -e

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration par défaut
BUILD_CONFIG="Release"
PUBLISH_DIR="./publish"
RUNTIME="linux-x64"
SELF_CONTAINED="true"
TRIM_UNUSED="false"

# Fonction d'aide
show_help() {
    echo -e "${BLUE}Script de compilation et publication pour SftpCopyTool${NC}"
    echo
    echo "Usage: $0 [options]"
    echo
    echo "Options:"
    echo "  -c, --config CONFIG       Configuration de build (Debug|Release) [défaut: Release]"
    echo "  -o, --output DIR          Répertoire de sortie [défaut: ./publish]"
    echo "  -r, --runtime RUNTIME     Runtime cible [défaut: linux-x64]"
    echo "  --no-self-contained       Désactive le mode self-contained"
    echo "  --trim                    Active la suppression des dépendances inutilisées"
    echo "  --clean                   Nettoie avant de compiler"
    echo "  -h, --help               Affiche cette aide"
    echo
    echo "Runtimes supportés:"
    echo "  linux-x64        Linux 64-bit (Intel/AMD)"
    echo "  linux-arm64      Linux ARM64"
    echo "  linux-musl-x64   Alpine Linux 64-bit"
    echo
    echo "Exemples:"
    echo "  $0                                    # Build standard"
    echo "  $0 --trim --clean                    # Build optimisé avec nettoyage"
    echo "  $0 -r linux-arm64 -o ./publish-arm64 # Build pour ARM64"
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

# Parse des arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--config)
            BUILD_CONFIG="$2"
            shift 2
            ;;
        -o|--output)
            PUBLISH_DIR="$2"
            shift 2
            ;;
        -r|--runtime)
            RUNTIME="$2"
            shift 2
            ;;
        --no-self-contained)
            SELF_CONTAINED="false"
            shift
            ;;
        --trim)
            TRIM_UNUSED="true"
            shift
            ;;
        --clean)
            CLEAN_FIRST="true"
            shift
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

# Vérifier que dotnet est installé
if ! command -v dotnet &> /dev/null; then
    log_error ".NET SDK n'est pas installé ou n'est pas dans le PATH"
    exit 1
fi

log_info "Configuration de build:"
log_info "  Configuration: $BUILD_CONFIG"
log_info "  Répertoire de sortie: $PUBLISH_DIR"
log_info "  Runtime: $RUNTIME"
log_info "  Self-contained: $SELF_CONTAINED"
log_info "  Trim unused: $TRIM_UNUSED"
echo

# Nettoyage si demandé
if [[ "$CLEAN_FIRST" == "true" ]]; then
    log_step "Nettoyage des artefacts de build..."
    dotnet clean ../src/ -c "$BUILD_CONFIG"
    rm -rf "$PUBLISH_DIR"
fi

# Restauration des dépendances
log_step "Restauration des dépendances..."
dotnet restore ../src/

# Compilation
log_step "Compilation du projet..."
dotnet build ../src/ -c "$BUILD_CONFIG" --no-restore

# Construction des arguments de publication
PUBLISH_ARGS="../src/ -c $BUILD_CONFIG -o $PUBLISH_DIR --no-build"

if [[ "$SELF_CONTAINED" == "true" ]]; then
    PUBLISH_ARGS="$PUBLISH_ARGS --self-contained true -r $RUNTIME"
else
    PUBLISH_ARGS="$PUBLISH_ARGS --self-contained false"
fi

if [[ "$TRIM_UNUSED" == "true" ]]; then
    PUBLISH_ARGS="$PUBLISH_ARGS -p:PublishTrimmed=true"
fi

# Publication
log_step "Publication de l'application..."
dotnet publish $PUBLISH_ARGS

# Vérification du résultat
if [[ -f "$PUBLISH_DIR/SftpCopyTool" ]]; then
    log_info "✅ Publication réussie !"
    
    # Rendre l'exécutable... exécutable
    chmod +x "$PUBLISH_DIR/SftpCopyTool"
    
    # Afficher les informations sur le fichier généré
    FILE_SIZE=$(du -h "$PUBLISH_DIR/SftpCopyTool" | cut -f1)
    log_info "📦 Taille de l'exécutable: $FILE_SIZE"
    log_info "📁 Emplacement: $PUBLISH_DIR/SftpCopyTool"
    
    echo
    log_info "Pour tester l'application:"
    echo "  $PUBLISH_DIR/SftpCopyTool --help"
    echo
    log_info "Pour installer globalement:"
    echo "  sudo cp $PUBLISH_DIR/SftpCopyTool /usr/local/bin/sftp-copy-tool"
    echo "  sudo chmod +x /usr/local/bin/sftp-copy-tool"
    
else
    log_error "❌ La publication a échoué - exécutable non trouvé"
    exit 1
fi
