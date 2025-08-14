# 📊 Guide Complet du Système de Logging Serilog

Ce guide détaille le système de logging avancé basé sur **Serilog** implémenté dans SftpCopyTool.

## 🎯 Vue d'Ensemble

SftpCopyTool utilise **Serilog**, un framework de logging professionnel qui offre :

- ✅ **Logging structuré** avec niveaux configurables
- ✅ **Rotation automatique** des fichiers de log
- ✅ **Formatage personnalisé** pour console et fichiers
- ✅ **Configuration dynamique** via CLI
- ✅ **Performance optimisée** pour la production

## 🎛️ Configuration des Niveaux de Log

### Niveaux Disponibles (du plus verbeux au moins verbeux)

| Niveau | Code | Description | Cas d'Usage | Recommandation |
|--------|------|-------------|-------------|----------------|
| **Verbose** | `VRB` | Traces très détaillées | Débogage approfondi | 🔧 Développement uniquement |
| **Debug** | `DBG` | Informations de débogage | Investigation, diagnostic | 🔍 Résolution de problèmes |
| **Information** | `INF` | Logs informatifs normaux | Suivi des opérations | ✅ **Production (défaut)** |
| **Warning** | `WRN` | Avertissements | Situations anormales non critiques | ⚠️ Monitoring |
| **Error** | `ERR` | Erreurs | Échecs d'opérations | ❌ Surveillance d'erreurs |
| **Fatal** | `FTL` | Erreurs critiques | Arrêt de l'application | 💀 Alertes critiques |

### Configuration via CLI

```bash
# Paramètre de niveau de log
--log-level <niveau>

# Exemples
--log-level Information  # (défaut)
--log-level Debug       # Pour diagnostic
--log-level Warning     # Monitoring minimal
--log-level Error       # Erreurs uniquement
```

## 📁 Gestion des Fichiers de Log

### Structure et Rotation

```
logs/
├── sftp-copy-20250814.txt    # Aujourd'hui (actuel)
├── sftp-copy-20250813.txt    # Hier
├── sftp-copy-20250812.txt    # Avant-hier
├── ...                       # Jusqu'à 10 fichiers max
└── sftp-copy-20250804.txt    # Le plus ancien (sera supprimé automatiquement)
```

### Paramètres de Rotation

- **Fréquence** : Quotidienne (nouveau fichier à minuit)
- **Rétention** : 10 fichiers maximum (10 jours d'historique)
- **Format de nom** : `sftp-copy-YYYYMMDD.txt`
- **Création** : Automatique du dossier `logs/` si inexistant

### Activation/Désactivation

```bash
# Activer l'écriture dans un fichier (défaut)
--log-file true

# Désactiver l'écriture dans un fichier (console uniquement)
--log-file false
```

## 📝 Formats de Sortie

### Format Console (Compact et Lisible)

```
[HH:mm:ss LVL] Message avec emojis
```

**Exemple :**
```
[10:15:23 INF] 🔌 Connexion au serveur SFTP sftp.example.com:22
[10:15:24 INF] ✅ Connexion établie avec succès
[10:15:25 INF] 📄 Copie du fichier '/remote/data.zip' vers '/local/'
[10:15:26 ERR] ❌ Erreur lors de la copie SFTP
[10:15:26 FTL] 💀 Erreur critique: Connexion interrompue
```

### Format Fichier (Détaillé avec Horodatage Complet)

```
[YYYY-MM-dd HH:mm:ss.fff zzz LVL] Message avec emojis
```

**Exemple :**
```
[2025-08-14 10:15:23.123 +02:00 INF] 🔌 Connexion au serveur SFTP sftp.example.com:22
[2025-08-14 10:15:24.456 +02:00 INF] ✅ Connexion établie avec succès
[2025-08-14 10:15:25.789 +02:00 INF] 📄 Copie du fichier '/remote/data.zip' vers '/local/'
[2025-08-14 10:15:26.012 +02:00 ERR] ❌ Erreur lors de la copie SFTP
[2025-08-14 10:15:26.345 +02:00 FTL] 💀 Erreur critique: Connexion interrompue
```

## 🎯 Exemples d'Utilisation par Scénario

### 🚀 Production Standard
```bash
# Configuration recommandée pour la production
./SftpCopyTool --host prod.sftp.com --username prod_user --password "$PROD_PASS" \
  --remote-path /prod/backup --local-path /local/backup --recursive \
  --log-level Information --log-file true
```

**Avantages :**
- ✅ Logs informatifs sans surcharge
- ✅ Historique complet dans les fichiers
- ✅ Diagnostic possible en cas de problème

### 🔍 Débogage et Diagnostic
```bash
# Pour diagnostiquer un problème
./SftpCopyTool --host debug.sftp.com --username debug_user --password "$DEBUG_PASS" \
  --remote-path /test/data --local-path /tmp/debug --recursive \
  --log-level Debug --log-file true
```

**Avantages :**
- ✅ Maximum de détails pour identifier le problème
- ✅ Traces des opérations internes
- ✅ Sauvegarde complète pour analyse ultérieure

### ⚡ Performance/Monitoring Minimal
```bash
# Pour un monitoring léger (erreurs uniquement)
./SftpCopyTool --host monitor.sftp.com --username monitor_user --password "$MONITOR_PASS" \
  --remote-path /critical/data --local-path /backup/critical --recursive \
  --log-level Warning --log-file false
```

**Avantages :**
- ✅ Charge minimale sur le système
- ✅ Alertes uniquement si problème
- ✅ Pas de stockage de fichiers de log

### 🚨 Alertes Critiques Uniquement
```bash
# Pour des tâches automatisées avec alerting externe
./SftpCopyTool --host critical.sftp.com --username crit_user --password "$CRIT_PASS" \
  --remote-path /emergency/backup --local-path /safe/backup --recursive \
  --log-level Fatal --log-file true
```

**Avantages :**
- ✅ Silence total sauf erreurs critiques
- ✅ Idéal pour intégration avec systèmes d'alerting
- ✅ Fichier de log pour audit de sécurité

## ⚙️ Configuration Avancée

### Variables d'Environnement pour Scripts

```bash
#!/bin/bash
# Script de sauvegarde automatisée

# Configuration des logs selon l'environnement
if [[ "$ENVIRONMENT" == "production" ]]; then
    LOG_LEVEL="Information"
    LOG_FILE="true"
elif [[ "$ENVIRONMENT" == "development" ]]; then
    LOG_LEVEL="Debug"
    LOG_FILE="true"
else
    LOG_LEVEL="Warning"
    LOG_FILE="false"
fi

# Exécution avec configuration dynamique
./SftpCopyTool \
    --host "$SFTP_HOST" \
    --username "$SFTP_USER" \
    --password "$SFTP_PASS" \
    --remote-path "$REMOTE_PATH" \
    --local-path "$LOCAL_PATH" \
    --recursive \
    --log-level "$LOG_LEVEL" \
    --log-file "$LOG_FILE"
```

### Intégration avec systemd

```ini
# /etc/systemd/system/sftp-copy.service
[Unit]
Description=SFTP Copy Tool
After=network.target

[Service]
Type=oneshot
ExecStart=/opt/sftpcopy/SftpCopyTool \
    --host %i \
    --username ${SFTP_USER} \
    --password ${SFTP_PASS} \
    --remote-path ${REMOTE_PATH} \
    --local-path ${LOCAL_PATH} \
    --recursive \
    --log-level Information \
    --log-file true
Environment=SFTP_USER=backup_user
EnvironmentFile=/etc/sftp-copy/config.env
User=sftp-copy
Group=sftp-copy
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

### Consultation des Logs avec systemd

```bash
# Logs en temps réel
sudo journalctl -u sftp-copy.service -f

# Logs depuis aujourd'hui
sudo journalctl -u sftp-copy.service --since today

# Logs avec filtrage par niveau
sudo journalctl -u sftp-copy.service | grep "ERR\|FTL"

# Logs dans les fichiers (plus détaillés)
tail -f /opt/sftpcopy/logs/sftp-copy-$(date +%Y%m%d).txt
```

## 📊 Analyse et Monitoring

### Extraction de Métriques

```bash
# Nombre d'erreurs aujourd'hui
grep -c "ERR\|FTL" logs/sftp-copy-$(date +%Y%m%d).txt

# Fichiers copiés avec succès
grep -c "✅ Fichier copié" logs/sftp-copy-$(date +%Y%m%d).txt

# Temps de connexion moyen
grep "🔌 Connexion au serveur" logs/sftp-copy-*.txt | tail -n 20
```

### Surveillance Proactive

```bash
# Script de surveillance des erreurs
#!/bin/bash
LOG_FILE="logs/sftp-copy-$(date +%Y%m%d).txt"
ERROR_COUNT=$(grep -c "ERR\|FTL" "$LOG_FILE" 2>/dev/null || echo "0")

if [[ $ERROR_COUNT -gt 5 ]]; then
    echo "⚠️ ALERTE: $ERROR_COUNT erreurs détectées aujourd'hui"
    # Envoyer notification (email, Slack, etc.)
fi
```

## 🔧 Dépannage

### Problèmes Courants

#### Les logs ne s'affichent pas
```bash
# Vérifier le niveau de log
--log-level Debug  # Assurer que le niveau est suffisant
```

#### Fichier de log non créé
```bash
# Vérifier les permissions
ls -la logs/
# Créer le dossier manuellement si nécessaire
mkdir -p logs
chmod 755 logs
```

#### Fichiers de log trop volumineux
```bash
# Utiliser un niveau moins verbeux
--log-level Warning  # Au lieu de Debug/Verbose

# Ou désactiver les fichiers de log
--log-file false
```

### Diagnostic du Système de Logging

```bash
# Test rapide du système de logging
./SftpCopyTool --host fake.invalid --username test --password test \
  --remote-path /test --local-path /tmp \
  --log-level Debug --log-file true

# Vérifier que les fichiers sont créés
ls -la logs/

# Vérifier le contenu
head -n 5 logs/sftp-copy-$(date +%Y%m%d).txt
```

## 🎉 Conclusion

Le système de logging Serilog de SftpCopyTool offre une flexibilité et une robustesse adaptées à tous les environnements, du développement à la production. Les emojis rendent les logs lisibles et agréables, même dans les fichiers de log ! 

**Configuration recommandée pour démarrer :**
```bash
--log-level Information --log-file true
```

Cette configuration offre le meilleur équilibre entre détail utile et performance. ✨
