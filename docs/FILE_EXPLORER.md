# Fonctionnalité Explorateur de Fichiers

## Vue d'ensemble

La nouvelle fonctionnalité d'explorateur de fichiers permet aux utilisateurs de naviguer facilement dans les systèmes de fichiers locaux et distants (SFTP) pour sélectionner les chemins source et destination.

## Fonctionnalités

### 🌍 Explorateur Distant (SFTP)
- Navigation dans l'arborescence du serveur distant
- Affichage des fichiers et dossiers avec icônes contextuelles
- Support du fil d'Ariane (breadcrumb) pour navigation rapide
- Gestion des erreurs de connexion et permissions
- Tri automatique (dossiers en premier, puis alphabétique)

### 💽 Explorateur Local
- Navigation dans l'arborescence du système local
- Affichage des fichiers et dossiers avec icônes par type
- Support Windows/Linux avec chemins appropriés
- Filtrage des fichiers cachés/système
- Gestion des permissions et erreurs d'accès

### ✨ Interface Utilisateur
- Modal responsive avec design Bootstrap
- Barre de navigation avec breadcrumbs cliquables
- Saisie manuelle de chemin avec validation
- Boutons de rafraîchissement et navigation
- Icônes FontAwesome pour types de fichiers
- Indication visuelle des sélections

## Architecture

### Composants Créés

1. **`FileExplorerModels.cs`**
   - `FileItem`: Représente un fichier ou dossier
   - `DirectoryContents`: Contenu d'un répertoire
   - `PathBreadcrumb`: Éléments du fil d'Ariane

2. **`FileExplorerService.cs`**
   - Service principal pour la navigation
   - Méthodes pour explorateurs local et distant
   - Génération des breadcrumbs

3. **`FileExplorer.razor`**
   - Composant principal d'exploration
   - Interface utilisateur avec tableau
   - Gestion des événements de navigation

4. **`FileExplorerModal.razor`**
   - Modal wrapper pour l'explorateur
   - Gestion de la sélection et confirmation
   - Interface modale responsive

### Intégration dans l'Application

#### Page Index.razor
- Boutons d'exploration ajoutés aux champs de chemin
- Modals intégrés pour chaque type d'exploration
- Gestion des états et callbacks

#### Program.cs
- Enregistrement du service `IFileExplorerService`

## Utilisation

1. **Navigation Distante**:
   - Renseigner les paramètres de connexion SFTP
   - Cliquer sur le bouton 📁 à côté du champ "Chemin distant"
   - Naviguer dans l'arborescence du serveur
   - Sélectionner un dossier ou fichier
   - Confirmer la sélection

2. **Navigation Locale**:
   - Cliquer sur le bouton 📁 à côté du champ "Chemin local"
   - Naviguer dans l'arborescence locale
   - Sélectionner le dossier de destination
   - Confirmer la sélection

## Types de Fichiers Supportés

Les icônes sont automatiquement assignées selon l'extension :
- 📄 Documents (PDF, DOC, XLS, PPT)
- 🖼️ Images (JPG, PNG, GIF)
- 🎵 Audio (MP3, WAV, FLAC)
- 🎬 Vidéo (MP4, AVI, MKV)
- 📦 Archives (ZIP, RAR, 7Z)
- 💻 Code (HTML, CSS, JS, CS)
- 📁 Dossiers
- 📄 Autres fichiers

## Sécurité

- Filtrage des fichiers système/cachés
- Gestion sécurisée des connexions SFTP
- Validation des permissions d'accès
- Timeout de connexion configuré
- Gestion d'erreurs robuste

## Performance

- Navigation asynchrone pour SFTP
- Chargement optimisé des répertoires
- Cache des connexions pendant la session
- Limitation d'affichage (50 logs max)
- Interface responsive sans blocage
