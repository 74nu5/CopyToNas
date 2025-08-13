#!/bin/bash

# Script d'installation pour SftpCopyTool sur Linux
# Exécutez avec : chmod +x install.sh && sudo ./install.sh

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

log_info "Installation de SftpCopyTool..."

# Définir les chemins
INSTALL_DIR="/opt/sftpcopy"
SERVICE_FILE="/etc/systemd/system/sftp-copy.service"
CONFIG_DIR="/etc/sftp-copy"

# Créer les dossiers
log_info "Création des dossiers..."
mkdir -p "$INSTALL_DIR"
mkdir -p "$CONFIG_DIR"

# Vérifier si .NET est installé
if ! command -v dotnet &> /dev/null; then
    log_warn ".NET n'est pas installé. Installation en cours..."
    
    # Installation pour Ubuntu/Debian
    if command -v apt-get &> /dev/null; then
        apt-get update
        apt-get install -y dotnet-runtime-8.0 dotnet-sdk-8.0
    
    # Installation pour CentOS/RHEL/Fedora
    elif command -v yum &> /dev/null; then
        yum install -y dotnet-runtime-8.0 dotnet-sdk-8.0
    
    # Installation pour openSUSE
    elif command -v zypper &> /dev/null; then
        zypper install -y dotnet-runtime-8.0 dotnet-sdk-8.0
    
    else
        log_error "Gestionnaire de paquets non supporté. Veuillez installer .NET manuellement."
        exit 1
    fi
fi

# Copier les fichiers du projet
log_info "Copie des fichiers du projet..."
cp -r ./* "$INSTALL_DIR/"

# Compiler le projet
log_info "Compilation du projet..."
cd "$INSTALL_DIR"
dotnet build -c Release

# Copier le fichier de configuration
log_info "Configuration du service..."
cp config.env "$CONFIG_DIR/"
chmod 600 "$CONFIG_DIR/config.env"

# Installer le service systemd
if [[ -f "sftp-copy.service" ]]; then
    cp sftp-copy.service "$SERVICE_FILE"
    
    # Mettre à jour les chemins dans le service
    sed -i "s|WorkingDirectory=.*|WorkingDirectory=$INSTALL_DIR|g" "$SERVICE_FILE"
    sed -i "s|ExecStart=.*|ExecStart=$INSTALL_DIR/sftp-copy.sh|g" "$SERVICE_FILE"
    
    systemctl daemon-reload
    log_info "Service systemd installé : sftp-copy.service"
fi

# Créer un script d'exécution
cat > "$INSTALL_DIR/sftp-copy.sh" << 'EOF'
#!/bin/bash
source /etc/sftp-copy/config.env
cd /opt/sftpcopy
exec dotnet run -c Release -- \
    --host "$SFTP_HOST" \
    --port "$SFTP_PORT" \
    --username "$SFTP_USERNAME" \
    --password "$SFTP_PASSWORD" \
    --remote-path "$REMOTE_PATH" \
    --local-path "$LOCAL_PATH" \
    $RECURSIVE_MODE
EOF

chmod +x "$INSTALL_DIR/sftp-copy.sh"

# Créer un lien symbolique dans /usr/local/bin
ln -sf "$INSTALL_DIR/sftp-copy.sh" /usr/local/bin/sftp-copy

log_info "Installation terminée !"
echo
echo "Configuration :"
log_info "1. Éditez le fichier de configuration : $CONFIG_DIR/config.env"
log_info "2. Pour exécuter manuellement : sftp-copy"
log_info "3. Pour activer le service : systemctl enable sftp-copy.service"
log_info "4. Pour démarrer le service : systemctl start sftp-copy.service"
log_info "5. Pour voir les logs : journalctl -u sftp-copy.service"
echo
log_warn "N'oubliez pas de configurer les paramètres SFTP dans $CONFIG_DIR/config.env"
