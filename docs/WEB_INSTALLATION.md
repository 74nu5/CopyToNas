# Installation de l'Interface Web SFTP Copy Tool sur Linux

Ce guide décrit l'installation complète de l'interface web SFTP Copy Tool sur un serveur Linux.

## Prérequis

- Serveur Linux avec systemd (Ubuntu 20.04+, Debian 11+, CentOS 8+, RHEL 8+)
- Accès root (sudo)
- Reverse proxy YARP déjà installé et configuré
- Connexion Internet pour télécharger .NET Runtime

## Installation Rapide

### 1. Cloner le projet et naviguer vers les scripts

```bash
git clone https://github.com/74nu5/CopyToNas.git
cd CopyToNas/scripts
```

### 2. Rendre les scripts exécutables

```bash
chmod +x deploy-web.sh
chmod +x update-web.sh
chmod +x monitor-web.sh
```

### 3. Exécuter l'installation

```bash
sudo ./deploy-web.sh
```

Le script va automatiquement :
- ✅ Installer .NET 9 Runtime (si nécessaire)
- ✅ Créer l'utilisateur système `sftpcopy`
- ✅ Créer les répertoires dans `/opt/sftpcopy-web`
- ✅ Compiler et déployer l'application
- ✅ Configurer le service systemd
- ✅ Configurer le pare-feu (si ufw/firewalld détectés)
- ✅ Démarrer le service

## Configuration du Reverse Proxy YARP

### Option 1: Route avec préfixe /sftp-web

Ajoutez cette configuration à votre `appsettings.json` de YARP :

```json
{
  "ReverseProxy": {
    "Routes": {
      "sftp-web-route": {
        "ClusterId": "sftp-web-cluster",
        "Match": {
          "Path": "/sftp-web/{**catch-all}"
        },
        "Transforms": [
          {
            "PathPattern": "/{**catch-all}"
          }
        ]
      }
    },
    "Clusters": {
      "sftp-web-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5000/"
          }
        }
      }
    }
  }
}
```

### Option 2: Route sur sous-domaine

```json
{
  "ReverseProxy": {
    "Routes": {
      "sftp-web-root-route": {
        "ClusterId": "sftp-web-cluster",
        "Match": {
          "Hosts": ["sftp.votre-domaine.com"]
        }
      }
    },
    "Clusters": {
      "sftp-web-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5000/"
          }
        }
      }
    }
  }
}
```

## Scripts de Maintenance

### Monitor et Status

```bash
# Vérifier le statut
sudo ./monitor-web.sh status

# Voir les logs (50 dernières lignes)
sudo ./monitor-web.sh logs

# Suivre les logs en temps réel
sudo ./monitor-web.sh follow

# Test de santé complet
sudo ./monitor-web.sh health

# Métriques de performance
sudo ./monitor-web.sh metrics

# Nettoyage des fichiers anciens
sudo ./monitor-web.sh cleanup
```

### Mise à Jour

```bash
# Mise à jour complète (avec sauvegarde automatique)
sudo ./update-web.sh

# Créer une sauvegarde manuelle
sudo ./update-web.sh backup

# Restaurer la dernière sauvegarde
sudo ./update-web.sh restore
```

### Commandes Systemd

```bash
# Statut du service
sudo systemctl status sftpcopy-web

# Redémarrer le service
sudo systemctl restart sftpcopy-web

# Arrêter le service
sudo systemctl stop sftpcopy-web

# Démarrer le service
sudo systemctl start sftpcopy-web

# Voir les logs
sudo journalctl -u sftpcopy-web -f
```

## Structure des Fichiers

```
/opt/sftpcopy-web/                 # Application web
├── SftpCopyTool.Web               # Exécutable principal
├── appsettings.json               # Configuration de base
├── appsettings.Production.json    # Configuration de production
├── logs/                          # Logs de l'application
│   └── web-sftp-copy-*.txt        # Logs rotatifs
└── wwwroot/                       # Fichiers statiques

/etc/systemd/system/
└── sftpcopy-web.service           # Service systemd

/opt/sftpcopy-web-backups/         # Sauvegardes
└── backup-YYYYMMDD-HHMMSS.tar.gz
```

## Configuration Avancée

### Variables d'Environnement

Le service systemd utilise ces variables :

```ini
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=DOTNET_USE_POLLING_FILE_WATCHER=true
```

### Personnaliser la Configuration

Éditez `/opt/sftpcopy-web/appsettings.Production.json` :

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Urls": "http://localhost:5000"
}
```

### Changer le Port

1. Modifier le script `deploy-web.sh` (variable `PORT`)
2. Redéployer avec `sudo ./deploy-web.sh`
3. Mettre à jour votre configuration YARP

## Dépannage

### Le service ne démarre pas

```bash
# Voir les logs détaillés
sudo journalctl -u sftpcopy-web -n 50

# Vérifier les permissions
sudo ls -la /opt/sftpcopy-web/

# Vérifier la configuration
sudo systemctl cat sftpcopy-web
```

### Problème de connectivité

```bash
# Tester localement
curl http://localhost:5000

# Vérifier les ports ouverts
sudo netstat -tlnp | grep :5000

# Tester le health check
curl http://localhost:5000/health
```

### Problème de performance

```bash
# Métriques détaillées
sudo ./monitor-web.sh metrics

# Utilisation mémoire
sudo ps aux | grep SftpCopyTool.Web

# Espace disque
df -h /opt/sftpcopy-web
```

## Sécurité

### Pare-feu

Le port 5000 doit être ouvert seulement localement (pas d'accès externe direct).
L'accès se fait via le reverse proxy YARP.

### Permissions

- Application s'exécute sous l'utilisateur `sftpcopy` (non privilégié)
- Répertoire `/opt/sftpcopy-web` appartient à `sftpcopy:sftpcopy`
- Service systemd avec restrictions de sécurité activées

### SSL/TLS

Le SSL/TLS est géré par le reverse proxy YARP, pas par l'application directement.

## Surveillance

### Health Check

L'application expose un endpoint `/health` pour la surveillance :

```bash
curl http://localhost:5000/health
# Retourne: Healthy
```

### Logs

Les logs sont disponibles via :
- systemd: `sudo journalctl -u sftpcopy-web`
- Fichiers: `/opt/sftpcopy-web/logs/web-sftp-copy-*.txt`

### Monitoring automatique

Vous pouvez ajouter le script de monitoring à cron pour des vérifications automatiques :

```bash
# Ajouter à crontab (vérification toutes les 5 minutes)
*/5 * * * * /opt/CopyToNas/scripts/monitor-web.sh health >> /var/log/sftpcopy-web-monitor.log 2>&1
```

## Support

- Logs de l'application : `/opt/sftpcopy-web/logs/`
- Logs système : `sudo journalctl -u sftpcopy-web`
- Configuration : `/opt/sftpcopy-web/appsettings.Production.json`
- Service : `/etc/systemd/system/sftpcopy-web.service`
