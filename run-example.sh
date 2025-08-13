#!/bin/bash

# Script exemple pour utiliser SftpCopyTool sous Linux
# Modifier les variables ci-dessous selon vos besoins

# Configuration du serveur SFTP
SFTP_HOST="votre-serveur-sftp.com"
SFTP_PORT=22
SFTP_USERNAME="votre-utilisateur"
SFTP_PASSWORD="votre-mot-de-passe"

# Chemins
REMOTE_PATH="/chemin/distant/sur/serveur"
LOCAL_PATH="/chemin/local/destination"

# Options
RECURSIVE="--recursive"  # Supprimer cette ligne pour copier seulement un fichier

# Exécution de l'outil
dotnet run -- \
    --host "$SFTP_HOST" \
    --port "$SFTP_PORT" \
    --username "$SFTP_USERNAME" \
    --password "$SFTP_PASSWORD" \
    --remote-path "$REMOTE_PATH" \
    --local-path "$LOCAL_PATH" \
    $RECURSIVE

echo "Copie terminée !"
