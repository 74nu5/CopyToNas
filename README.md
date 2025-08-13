# SFTP Copy Tool

Outil en ligne de commande pour copier des fichiers et dossiers depuis un serveur SFTP vers un système local Linux.

## Fonctionnalités

- 🚀 Copie de fichiers individuels depuis un serveur SFTP
- 📁 Copie récursive de dossiers et de leurs sous-éléments
- 🔒 Support des connexions SFTP sécurisées
- 📋 Logging détaillé des opérations
- ⚡ Gestion d'erreurs robuste
- 🐧 Optimisé pour Linux avec support systemd
- 📦 Installation automatisée

## Prérequis

- .NET 8.0 ou supérieur
- Accès à un serveur SFTP
- Linux (testé sur Ubuntu, CentOS, openSUSE)

## 🚀 Démarrage rapide

### Méthode recommandée (avec publication)

```bash
# 1. Cloner et compiler
git clone <repository-url>
cd SftpCopyTool
chmod +x build.sh
./build.sh --trim --clean

# 2. Déployer
chmod +x deploy.sh
sudo ./deploy.sh

# 3. Configurer
sudo nano /etc/sftp-copy/config.env

# 4. Utiliser
sftp-copy-tool --help
# ou
sudo systemctl start sftp-copy.service
```

### Méthode de développement

```bash
git clone <repository-url>
cd SftpCopyTool
dotnet run -- --help
```

## Installation rapide (Linux)

```bash
# Cloner le projet
git clone <repository-url>
cd SftpCopyTool

# Installation automatique (nécessite sudo)
chmod +x install.sh
sudo ./install.sh
```

## Installation manuelle

```bash
# Cloner le projet
git clone <repository-url>
cd SftpCopyTool

# Installer .NET 8.0 si nécessaire
# Ubuntu/Debian : sudo apt-get install dotnet-runtime-8.0 dotnet-sdk-8.0
# CentOS/RHEL : sudo yum install dotnet-runtime-8.0 dotnet-sdk-8.0

# Restaurer les dépendances
dotnet restore

# Compiler le projet
dotnet build -c Release

# Publier l'application (recommandé pour la production)
dotnet publish -c Release -o ./publish --self-contained true -r linux-x64
```

## Configuration

### Variables d'environnement (recommandé)

```bash
# Copiez et modifiez le fichier de configuration
cp config.env /etc/sftp-copy/config.env
sudo nano /etc/sftp-copy/config.env

# Chargez la configuration
source /etc/sftp-copy/config.env
```

### Configuration directe par arguments

Voir les exemples d'utilisation ci-dessous.

## Publication de l'application

### Avantages de `dotnet publish`

La commande `dotnet publish` créé une version optimisée et déployable de l'application :

- ✅ **Performance améliorée** : Code IL compilé et optimisé
- ✅ **Déploiement simplifié** : Un seul exécutable autonome
- ✅ **Pas besoin de .NET SDK** : Runtime inclus (mode self-contained)
- ✅ **Taille réduite** : Suppression des dépendances inutilisées
- ✅ **Sécurité** : Moins de surface d'attaque

### Options de publication

```bash
# Publication basique (nécessite .NET Runtime sur la machine cible)
dotnet publish -c Release -o ./publish

# Publication autonome (inclut le runtime .NET)
dotnet publish -c Release -o ./publish --self-contained true -r linux-x64

# Publication optimisée avec suppression des dépendances inutilisées
dotnet publish -c Release -o ./publish --self-contained true -r linux-x64 -p:PublishTrimmed=true

# Publication pour différentes architectures
dotnet publish -c Release -o ./publish-arm64 --self-contained true -r linux-arm64  # ARM64
dotnet publish -c Release -o ./publish-musl --self-contained true -r linux-musl-x64  # Alpine Linux
```

### Déploiement de l'application publiée

```bash
# Après publication, copier l'exécutable
sudo cp ./publish/SftpCopyTool /usr/local/bin/sftp-copy-tool
sudo chmod +x /usr/local/bin/sftp-copy-tool

# Utilisation globale
sftp-copy-tool --host sftp.example.com --username user --password pass --remote-path /remote/file --local-path /local/
```

## Utilisation

### Avec l'application publiée (recommandé)

Après avoir utilisé `dotnet publish`, vous pouvez exécuter l'application directement :

```bash
# Depuis le dossier publish
./publish/SftpCopyTool --host <serveur> --username <utilisateur> --password <mot_de_passe> --remote-path <chemin_distant> --local-path <chemin_local>

# Ou après avoir copié l'exécutable dans /usr/local/bin
sudo cp ./publish/SftpCopyTool /usr/local/bin/sftp-copy-tool
sftp-copy-tool --host <serveur> --username <utilisateur> --password <mot_de_passe> --remote-path <chemin_distant> --local-path <chemin_local>
```

### Avec dotnet run (développement)

```bash
dotnet run -- --host <serveur> --username <utilisateur> --password <mot_de_passe> --remote-path <chemin_distant> --local-path <chemin_local>
```

### Options disponibles

- `--host` : Adresse du serveur SFTP (obligatoire)
- `--port` : Port du serveur SFTP (défaut: 22)
- `--username` : Nom d'utilisateur SFTP (obligatoire)  
- `--password` : Mot de passe SFTP (obligatoire)
- `--remote-path` : Chemin absolu sur le serveur distant (obligatoire)
- `--local-path` : Dossier local de destination (obligatoire)
- `--recursive` : Active la copie récursive pour les dossiers

### Exemples

#### Copier un fichier unique (avec application publiée)

```bash
./publish/SftpCopyTool --host sftp.example.com --username myuser --password mypass --remote-path /home/user/document.pdf --local-path /tmp/downloads/
```

#### Copier un dossier complet de façon récursive (avec application publiée)

```bash
./publish/SftpCopyTool --host sftp.example.com --username myuser --password mypass --remote-path /home/user/documents --local-path /tmp/downloads/ --recursive
```

#### Copier vers un fichier spécifique (avec application publiée)

```bash
./publish/SftpCopyTool --host sftp.example.com --username myuser --password mypass --remote-path /home/user/data.txt --local-path /tmp/my-data.txt
```

#### Exemples avec dotnet run (développement)

```bash
# Fichier unique
dotnet run -- --host sftp.example.com --username myuser --password mypass --remote-path /home/user/document.pdf --local-path /tmp/downloads/

# Dossier récursif
dotnet run -- --host sftp.example.com --username myuser --password mypass --remote-path /home/user/documents --local-path /tmp/downloads/ --recursive

# Fichier spécifique
dotnet run -- --host sftp.example.com --username myuser --password mypass --remote-path /home/user/data.txt --local-path /tmp/my-data.txt
```

#### Utilisation avec configuration (après installation)

```bash
# Exécution simple
sftp-copy

# Ou avec systemd
sudo systemctl start sftp-copy.service
```

## Service systemd

Après installation, vous pouvez utiliser systemd pour automatiser les copies :

```bash
# Activer le service au démarrage
sudo systemctl enable sftp-copy.service

# Démarrer le service
sudo systemctl start sftp-copy.service

# Vérifier le statut
sudo systemctl status sftp-copy.service

# Voir les logs
sudo journalctl -u sftp-copy.service -f
```

## Scripts utilitaires

- `install.sh` : Installation automatique sur Linux
- `uninstall.sh` : Désinstallation complète
- `run-example.sh` : Exemple de script d'exécution
- `config.env` : Modèle de configuration
- `build.sh` : Script de compilation et publication
- `deploy.sh` : Script de déploiement automatisé

## Sécurité

⚠️ **Important** : Pour un usage en production, considérez :

- ✅ Utilisation de variables d'environnement (recommandé)
- ✅ Permissions restrictives sur les fichiers de configuration (`chmod 600`)
- ⚠️ Authentification par clé SSH (non encore supportée)
- ⚠️ Stockage sécurisé des mots de passe (envisagez un gestionnaire de secrets)

### Sécurisation de la configuration

```bash
# Restreindre les permissions du fichier de configuration
sudo chmod 600 /etc/sftp-copy/config.env
sudo chown root:root /etc/sftp-copy/config.env
```

## Structure du projet

```
SftpCopyTool/
├── Program.cs                  # Point d'entrée et configuration CLI
├── SftpService.cs              # Service principal pour les opérations SFTP
├── SftpCopyTool.csproj         # Configuration du projet
├── README.md                   # Documentation
├── LICENSE                     # Licence MIT
├── .gitignore                  # Fichiers à ignorer par Git
├── Scripts d'installation/déploiement:
│   ├── install.sh              # Script d'installation Linux automatique
│   ├── uninstall.sh            # Script de désinstallation
│   ├── build.sh                # Script de compilation et publication
│   └── deploy.sh               # Script de déploiement automatisé
├── Configuration:
│   ├── config.env              # Modèle de configuration
│   └── sftp-copy.service.example # Exemple de service systemd
└── Exemples:
    └── run-example.sh          # Exemple d'exécution
```

## Dépendances

- **SSH.NET** (2024.0.0) : Bibliothèque pour les connexions SSH/SFTP
- **Microsoft.Extensions.Logging** (8.0.0) : Framework de logging
- **Microsoft.Extensions.Logging.Console** (8.0.0) : Logger console  
- **System.CommandLine** (2.0.0-beta4) : Parsing des arguments CLI

## Gestion des erreurs

L'outil gère les erreurs suivantes :
- ❌ Échec de connexion au serveur SFTP
- ❌ Fichiers ou dossiers inexistants sur le serveur distant  
- ❌ Problèmes de permissions
- ❌ Erreurs d'écriture sur le système local
- ❌ Paramètres de connexion invalides

## Désinstallation

```bash
# Désinstallation automatique
sudo ./uninstall.sh

# Ou manuellement
sudo systemctl stop sftp-copy.service
sudo systemctl disable sftp-copy.service
sudo rm -f /etc/systemd/system/sftp-copy.service
sudo rm -rf /opt/sftpcopy
sudo rm -f /usr/local/bin/sftp-copy
# Optionnel : sudo rm -rf /etc/sftp-copy
```

## Contributions

Les contributions sont les bienvenues ! N'hésitez pas à :
- 🐛 Signaler des bugs
- 💡 Proposer des améliorations  
- 🔧 Soumettre des pull requests
- 📖 Améliorer la documentation

## Licence

Ce projet est sous licence MIT. Voir le fichier LICENSE pour plus de détails.

---

**Fait avec ❤️ pour simplifier les transferts SFTP sous Linux**
