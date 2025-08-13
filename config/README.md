# Configuration

Ce dossier contient les fichiers de configuration pour SftpCopyTool.

## Fichiers

- **`config.env`** : Modèle de fichier de configuration avec variables d'environnement
- **`sftp-copy.service`** : Service systemd pour automatisation
- **`sftp-copy.service.example`** : Exemple de service systemd avec sécurité renforcée

## Configuration des variables d'environnement

Le fichier `config.env` contient les variables suivantes :

```bash
SFTP_HOST="votre-serveur-sftp.com"
SFTP_PORT=22
SFTP_USERNAME="votre-utilisateur"
SFTP_PASSWORD="votre-mot-de-passe"
REMOTE_PATH="/chemin/distant/sur/serveur"
LOCAL_PATH="/chemin/local/destination"
RECURSIVE_MODE="--recursive"  # ou laisser vide pour désactiver
```

## Installation en production

```bash
# Copier la configuration
sudo cp config/config.env /etc/sftp-copy/config.env

# Sécuriser le fichier
sudo chmod 600 /etc/sftp-copy/config.env
sudo chown root:root /etc/sftp-copy/config.env

# Éditer les paramètres
sudo nano /etc/sftp-copy/config.env
```

## Service systemd

Pour utiliser le service systemd :

```bash
# Copier le service
sudo cp config/sftp-copy.service /etc/systemd/system/

# Recharger systemd
sudo systemctl daemon-reload

# Activer et démarrer
sudo systemctl enable sftp-copy.service
sudo systemctl start sftp-copy.service
```
