# SF## Fonctionnalités

- 🚀 Copie de fichiers individuels depuis un serveur SFTP
- 📁 Copie récursive de dossiers et de leurs sous-éléments
- 🔒 Support des connexions SFTP sécurisées
- ⏱️ **Suivi de progression en temps réel** avec vitesse de téléchargement
- 📋 **Logging avancé avec Serilog** - Niveaux configurables et rotation des fichiers
- 😀 Logging détaillé avec emojis pour une meilleure lisibilité
- ⚡ Gestion d'erreurs robuste
- 🐧 Optimisé pour Linux avec support systemd
- 📦 Installation automatiséeol

Outil en ligne de commande pour copier des fichiers et dossiers depuis un serveur SFTP vers un système local Linux.

## Fonctionnalités

- 🚀 Copie de fichiers individuels depuis un serveur SFTP
- 📁 Copie récursive de dossiers et de leurs sous-éléments
- 🔒 Support des connexions SFTP sécurisées
- � **Suivi de progression en temps réel** avec vitesse de téléchargement
- �📋 Logging détaillé avec emojis pour une meilleure lisibilité
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
chmod +x scripts/build.sh
./scripts/build.sh --trim --clean

# 2. Déployer
chmod +x scripts/deploy.sh
sudo ./scripts/deploy.sh

# 3. Configurer
sudo nano /etc/sftp-copy/config.env

# 4. Utiliser
sftp-copy-tool --help
# ou avec logging personnalisé
sftp-copy-tool --host sftp.example.com --username user --password pass \
  --remote-path /remote --local-path /local --log-level Debug
# ou
sudo systemctl start sftp-copy.service
```

### Méthode de développement

```bash
git clone <repository-url>
cd SftpCopyTool
dotnet run --project src/ -- --help
```

## Installation rapide (Linux)

```bash
# Cloner le projet
git clone <repository-url>
cd SftpCopyTool

# Installation automatique (nécessite sudo)
chmod +x scripts/install.sh
sudo ./scripts/install.sh
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
dotnet restore src/

# Compiler le projet
dotnet build src/ -c Release

# Publier l'application (recommandé pour la production)
dotnet publish src/ -c Release -o ./publish --self-contained true -r linux-x64
```

## Configuration

### Variables d'environnement (recommandé)

```bash
# Copiez et modifiez le fichier de configuration
cp config/config.env /etc/sftp-copy/config.env
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
dotnet publish src/ -c Release -o ./publish

# Publication autonome (inclut le runtime .NET)
dotnet publish src/ -c Release -o ./publish --self-contained true -r linux-x64

# Publication optimisée avec suppression des dépendances inutilisées
dotnet publish src/ -c Release -o ./publish --self-contained true -r linux-x64 -p:PublishTrimmed=true

# Publication pour différentes architectures
dotnet publish src/ -c Release -o ./publish-arm64 --self-contained true -r linux-arm64  # ARM64
dotnet publish src/ -c Release -o ./publish-musl --self-contained true -r linux-musl-x64  # Alpine Linux
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
dotnet run --project src/ -- --host <serveur> --username <utilisateur> --password <mot_de_passe> --remote-path <chemin_distant> --local-path <chemin_local>
```

### Options disponibles

**Options de connexion SFTP :**
- `--host` : Adresse du serveur SFTP (obligatoire)
- `--port` : Port du serveur SFTP (défaut: 22)
- `--username` : Nom d'utilisateur SFTP (obligatoire)  
- `--password` : Mot de passe SFTP (obligatoire)
- `--remote-path` : Chemin absolu sur le serveur distant (obligatoire)
- `--local-path` : Dossier local de destination (obligatoire)
- `--recursive` : Active la copie récursive pour les dossiers

**Options de logging avancé (Serilog) :**
- `--log-level` : Niveau de logging (défaut: Information)
  - `Verbose` : Logs très détaillés pour le débogage
  - `Debug` : Informations de débogage
  - `Information` : Logs informatifs normaux (recommandé)
  - `Warning` : Avertissements uniquement
  - `Error` : Erreurs uniquement
  - `Fatal` : Erreurs critiques uniquement
- `--log-file` : Activer l'écriture des logs dans un fichier (défaut: true)

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

#### Exemples avec options de logging avancées

```bash
# Copie avec logging minimal (erreurs uniquement, pas de fichier de log)
./publish/SftpCopyTool --host sftp.example.com --username myuser --password mypass \
  --remote-path /home/user/large-file.zip --local-path /tmp/downloads/ \
  --log-level Error --log-file false

# Copie avec logging détaillé pour diagnostic
./publish/SftpCopyTool --host sftp.example.com --username myuser --password mypass \
  --remote-path /home/user/documents --local-path /tmp/downloads/ --recursive \
  --log-level Debug --log-file true

# Copie de production avec logs standards (configuration par défaut)
./publish/SftpCopyTool --host sftp.example.com --username myuser --password mypass \
  --remote-path /home/user/backup --local-path /local/backup/ --recursive
```

#### Exemples avec dotnet run (développement)

```bash
# Fichier unique
dotnet run --project src/ -- --host sftp.example.com --username myuser --password mypass --remote-path /home/user/document.pdf --local-path /tmp/downloads/

# Dossier récursif avec logging de débogage
dotnet run --project src/ -- --host sftp.example.com --username myuser --password mypass --remote-path /home/user/documents --local-path /tmp/downloads/ --recursive --log-level Debug

# Fichier spécifique avec logs minimaux
dotnet run --project src/ -- --host sftp.example.com --username myuser --password mypass --remote-path /home/user/data.txt --local-path /tmp/my-data.txt --log-level Warning --log-file false
```

#### Utilisation avec configuration (après installation)

```bash
# Exécution simple
sftp-copy

# Ou avec systemd
sudo systemctl start sftp-copy.service
```

## 📋 Logs avec Emojis

L'application utilise des emojis pour améliorer la lisibilité des logs :

- 🔌 **Connexion/Déconnexion** - État des connexions SFTP
- ✅ **Succès** - Opérations réussies
- ❌ **Erreurs** - Problèmes rencontrés
- 📂 **Dossiers** - Traitement des répertoires
- 📄 **Fichiers** - Copie de fichiers individuels
- ⬇️ **Téléchargement** - Transferts en cours avec taille
- 📊 **Progression** - Avancement et vitesse en temps réel
- 🎉 **Completion** - Fin des opérations avec succès

Exemple de sortie avec progression :
```
🔌 Connexion au serveur SFTP exemple.com:22
✅ Connexion établie avec succès
📄 Copie du fichier '/remote/large-file.zip' vers '/local/downloads/'
⬇️ Téléchargement : /remote/large-file.zip -> /local/downloads/large-file.zip (25.5MB octets)
📊 Progression : 25.0% (6.4MB/25.5MB) - 3.2MB/s
📊 Progression : 50.0% (12.8MB/25.5MB) - 2.9MB/s
📊 Progression : 75.0% (19.1MB/25.5MB) - 3.1MB/s
📊 Progression : 100.0% (25.5MB/25.5MB) - 3.0MB/s
✅ Fichier copié : 26738688 octets
🎉 Copie terminée avec succès
🔌 Déconnexion du serveur SFTP
```

**Guides détaillés :**
- 📋 [Guide des emojis](docs/EMOJI_LOGS.md)
- 📊 [Suivi de progression](docs/DOWNLOAD_PROGRESS.md)
- 📊 [Système de logging Serilog](docs/SERILOG_LOGGING.md) - **Nouveau !**

## 📊 Logging Avancé avec Serilog

L'application utilise **Serilog** pour un système de logging professionnel et configurable avec les fonctionnalités suivantes :

### 🎛️ Configuration des Logs

#### Niveaux de Log Disponibles

| Niveau | Description | Cas d'usage |
|--------|-------------|-------------|
| `Verbose` | Logs très détaillés | Débogage approfondi, diagnostic |
| `Debug` | Informations de débogage | Développement, investigation |
| `Information` | Logs informatifs normaux | **Production (défaut)** |
| `Warning` | Avertissements uniquement | Monitoring d'alertes |
| `Error` | Erreurs uniquement | Monitoring d'erreurs |
| `Fatal` | Erreurs critiques uniquement | Alertes critiques |

#### Options de Sortie des Logs

- **Console** : Toujours activée avec format compact `[HH:mm:ss LVL] Message`
- **Fichier** : Optionnelle avec `--log-file true/false`

### 📁 Gestion des Fichiers de Log

#### Rotation Automatique
- **Rotation quotidienne** : Un nouveau fichier chaque jour
- **Format des noms** : `logs/sftp-copy-YYYYMMDD.txt`
- **Rétention** : Conserve automatiquement les 10 derniers fichiers
- **Format détaillé** : `[YYYY-MM-dd HH:mm:ss.fff zzz LVL] Message` avec timezone

#### Structure des Logs
```
logs/
├── sftp-copy-20250814.txt    # Logs d'aujourd'hui
├── sftp-copy-20250813.txt    # Logs d'hier
└── sftp-copy-20250812.txt    # Logs plus anciens
```

### 🎯 Exemples d'Utilisation des Logs

#### Logging Minimal (Production)
```bash
# Affichage uniquement des erreurs sur la console, sans fichier de log
./SftpCopyTool --host sftp.example.com --username user --password pass \
  --remote-path /remote/file --local-path /local/ \
  --log-level Error --log-file false
```

#### Logging Standard (Recommandé)
```bash
# Logs informatifs en console et fichier (configuration par défaut)
./SftpCopyTool --host sftp.example.com --username user --password pass \
  --remote-path /remote/file --local-path /local/
```

#### Logging de Débogage
```bash
# Logs très détaillés pour diagnostic
./SftpCopyTool --host sftp.example.com --username user --password pass \
  --remote-path /remote/file --local-path /local/ \
  --log-level Debug --log-file true
```

#### Monitoring d'Erreurs
```bash
# Affichage uniquement des avertissements et erreurs
./SftpCopyTool --host sftp.example.com --username user --password pass \
  --remote-path /remote/file --local-path /local/ \
  --log-level Warning
```

### 🔍 Formats de Sortie

#### Format Console (Compact)
```
[10:03:40 INF] 🔌 Connexion au serveur SFTP test.example.com:22
[10:03:41 ERR] ❌ Erreur lors de la copie SFTP
[10:03:41 FTL] 💀 Erreur critique: Hôte inconnu
```

#### Format Fichier (Détaillé)
```
[2025-08-14 10:03:40.123 +02:00 INF] 🔌 Connexion au serveur SFTP test.example.com:22
[2025-08-14 10:03:41.456 +02:00 ERR] ❌ Erreur lors de la copie SFTP
[2025-08-14 10:03:41.789 +02:00 FTL] 💀 Erreur critique: Hôte inconnu
```

### ⚙️ Configuration Avancée

#### Variables d'Environnement (pour scripts)
```bash
# Configuration des logs via variables
export SFTP_LOG_LEVEL="Debug"
export SFTP_LOG_FILE="true"

# Utilisation avec configuration automatique
./SftpCopyTool --host $SFTP_HOST --username $SFTP_USER \
  --log-level $SFTP_LOG_LEVEL --log-file $SFTP_LOG_FILE \
  # ... autres paramètres
```

#### Intégration avec systemd
```bash
# Voir les logs en temps réel
sudo journalctl -u sftp-copy.service -f

# Voir les logs avec niveau de détail
sudo journalctl -u sftp-copy.service --since today
```

**Note :** Les logs conservent tous les **emojis** pour une meilleure lisibilité, même dans les fichiers de log ! 🎉

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

- `scripts/install.sh` : Installation automatique sur Linux
- `scripts/uninstall.sh` : Désinstallation complète
- `scripts/run-example.sh` : Exemple de script d'exécution
- `config/config.env` : Modèle de configuration
- `scripts/build.sh` : Script de compilation et publication
- `scripts/deploy.sh` : Script de déploiement automatisé

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
├── src/                            # Code source
│   ├── Program.cs                  # Point d'entrée et configuration CLI
│   ├── SftpService.cs              # Service principal pour les opérations SFTP
│   └── SftpCopyTool.csproj         # Configuration du projet
├── scripts/                        # Scripts d'installation et de déploiement
│   ├── install.sh                  # Script d'installation Linux automatique
│   ├── uninstall.sh                # Script de désinstallation
│   ├── build.sh                    # Script de compilation et publication
│   ├── deploy.sh                   # Script de déploiement automatisé
│   └── run-example.sh              # Exemple d'exécution
├── config/                         # Configuration
│   ├── config.env                  # Modèle de configuration
│   ├── sftp-copy.service           # Service systemd
│   └── sftp-copy.service.example   # Exemple de service systemd
├── docs/                           # Documentation
│   └── BUILD_GUIDE.md              # Guide de compilation détaillé
├── README.md                       # Documentation principale
├── LICENSE                         # Licence MIT
└── .gitignore                      # Fichiers à ignorer par Git
```

## Dépendances

**Core :**
- **SSH.NET** (2024.0.0) : Bibliothèque pour les connexions SSH/SFTP
- **System.CommandLine** (2.0.0-beta4) : Parsing des arguments CLI

**Logging (Serilog) :**
- **Serilog** (4.0.1) : Framework de logging structuré
- **Serilog.Extensions.Logging** (8.0.0) : Intégration avec Microsoft.Extensions.Logging
- **Serilog.Sinks.Console** (6.0.0) : Sortie console avec formatage personnalisé
- **Serilog.Sinks.File** (6.0.0) : Sortie fichier avec rotation quotidienne

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
sudo ./scripts/uninstall.sh

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
