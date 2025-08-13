# Guide des Emojis dans les Logs 📋

Ce guide décrit tous les emojis ajoutés aux logs de l'application SftpCopyTool pour améliorer l'expérience utilisateur.

## 🎨 Types d'Emojis Utilisés

### 🔌 Connexion/Réseau
- **🔌 Connexion au serveur SFTP** - Indique le début de la tentative de connexion
- **✅ Connexion établie avec succès** - Confirmation que la connexion est active
- **🔌 Déconnexion du serveur SFTP** - Indique la fermeture propre de la connexion

### 📁 Gestion des Dossiers
- **📁 Dossier créé** - Indique la création d'un nouveau dossier local
- **📂 Copie récursive du dossier** - Indique le début d'une copie récursive
- **📂 Traitement du dossier** - Indique qu'un dossier est en cours de traitement

### 📄 Gestion des Fichiers
- **📄 Copie du fichier** - Indique la copie d'un fichier unique
- **⬇️ Téléchargement** - Indique qu'un téléchargement est en cours
- **✅ Fichier copié** - Confirmation qu'un fichier a été téléchargé avec succès

### 🎉 Succès et Erreurs
- **🎉 Copie terminée avec succès** - Indique que toute l'opération s'est bien déroulée
- **❌ Erreur lors de la copie SFTP** - Indique qu'une erreur s'est produite

## 📝 Exemple de Sortie

Voici un exemple de ce à quoi ressemblent les logs avec les nouveaux emojis :

```
🔌 Connexion au serveur SFTP exemple.com:22
✅ Connexion établie avec succès
📂 Copie récursive du dossier '/home/user/documents' vers '/tmp/downloads/'
📁 Dossier créé : /tmp/downloads/documents
📂 Traitement du dossier : /home/user/documents/photos
📁 Dossier créé : /tmp/downloads/documents/photos
⬇️ Téléchargement : /home/user/documents/photos/photo1.jpg -> /tmp/downloads/documents/photos/photo1.jpg
✅ Fichier copié : 2048576 octets
⬇️ Téléchargement : /home/user/documents/readme.txt -> /tmp/downloads/documents/readme.txt
✅ Fichier copié : 1024 octets
🎉 Copie terminée avec succès
🔌 Déconnexion du serveur SFTP
```

## 🌟 Avantages

- **Lisibilité améliorée** : Les emojis permettent d'identifier rapidement le type d'opération
- **Expérience utilisateur** : Interface plus moderne et engageante
- **Diagnostic rapide** : Les emojis d'erreur (❌) et de succès (✅, 🎉) sont facilement repérables
- **Contextualisation** : Chaque type d'opération a son emoji spécifique

## 🔧 Utilisation

Les emojis sont automatiquement inclus dans tous les logs de l'application. Aucune configuration supplémentaire n'est nécessaire.

Pour tester les nouveaux logs :
```bash
# Test avec aide (pour voir l'interface)
dotnet run --project src/ -- --help

# Test avec paramètres réels
dotnet run --project src/ -- --host votre-serveur --username user --password pass --remote-path /chemin --local-path /destination
```
