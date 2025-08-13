# Guide de compilation et publication

## Options de compilation

### 1. Compilation de développement
```bash
dotnet build
```

### 2. Compilation Release
```bash
dotnet build -c Release
```

## Options de publication

### 1. Publication basique (nécessite .NET Runtime sur la cible)
```bash
dotnet publish -c Release -o ./publish
```
- Taille : ~13 MB (sans runtime)
- Nécessite .NET Runtime 8.0 sur la machine cible
- Plus rapide à publier

### 2. Publication self-contained (runtime inclus)
```bash
dotnet publish -c Release -o ./publish --self-contained true -r linux-x64
```
- Taille : ~70 MB (avec runtime)
- Ne nécessite pas .NET sur la machine cible
- Portable et autonome

### 3. Publication optimisée avec trimming
```bash
dotnet publish -c Release -o ./publish --self-contained true -r linux-x64 -p:PublishTrimmed=true
```
- Taille : Identique ou légèrement réduite
- Supprime les dépendances inutilisées
- Peut améliorer les performances de démarrage

### 4. Publication pour différentes architectures

#### Linux x64 (Intel/AMD 64-bit)
```bash
dotnet publish -c Release -o ./publish-x64 --self-contained true -r linux-x64
```

#### Linux ARM64 (Raspberry Pi 4, etc.)
```bash
dotnet publish -c Release -o ./publish-arm64 --self-contained true -r linux-arm64
```

#### Alpine Linux (containers Docker légers)
```bash
dotnet publish -c Release -o ./publish-musl --self-contained true -r linux-musl-x64
```

## Recommandations

### Pour le développement
- Utilisez `dotnet run` ou `dotnet build`
- Plus rapide pour les itérations

### Pour la production
- Utilisez `dotnet publish --self-contained true -r linux-x64`
- Garantit la compatibilité sans dépendances externes
- Facilite le déploiement

### Pour les containers Docker
- Utilisez `dotnet publish --self-contained true -r linux-musl-x64`
- Compatible avec les images Alpine Linux
- Taille optimisée pour les containers

## Automatisation

### Avec le script build.sh
```bash
chmod +x build.sh
./build.sh --trim --clean -r linux-x64
```

### Avec le script deploy.sh (après build)
```bash
chmod +x deploy.sh
sudo ./deploy.sh
```

## Vérification de la publication

Après publication, vérifiez que l'exécutable fonctionne :

```bash
# Testez l'aide
./publish/SftpCopyTool --help

# Testez la version
./publish/SftpCopyTool --version
```

## Dépendances système requises

### Pour la compilation
- .NET SDK 8.0+

### Pour l'exécution (publication self-contained)
- Aucune dépendance .NET requise
- Bibliothèques système standard Linux

### Pour l'exécution (publication normale)
- .NET Runtime 8.0
- Bibliothèques système standard Linux
