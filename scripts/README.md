# Scripts

Ce dossier contient tous les scripts d'installation, de déploiement et d'utilisation de SftpCopyTool.

## Scripts disponibles

### Installation et déploiement

- **`install.sh`** : Installation automatique complète sur Linux (nécessite sudo)
- **`uninstall.sh`** : Désinstallation propre de l'application
- **`build.sh`** : Compilation et publication avec options avancées
- **`deploy.sh`** : Déploiement de l'application publiée

### Utilisation

- **`run-example.sh`** : Exemple d'utilisation de l'application

## Utilisation

### Installation rapide

```bash
chmod +x scripts/install.sh
sudo ./scripts/install.sh
```

### Build personnalisé

```bash
chmod +x scripts/build.sh
./scripts/build.sh --trim --clean -r linux-x64
```

### Déploiement après build

```bash
chmod +x scripts/deploy.sh
sudo ./scripts/deploy.sh
```

## Notes

- Tous les scripts doivent être exécutés depuis la racine du projet
- Les scripts d'installation et de déploiement nécessitent des privilèges sudo
- Les chemins dans les scripts sont relatifs à la racine du projet
