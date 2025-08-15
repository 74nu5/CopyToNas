# Sauvegarde Automatique des Paramètres

## Vue d'ensemble

L'interface web SFTP Copy Tool inclut maintenant une **sauvegarde automatique** des paramètres du formulaire dans le localStorage du navigateur. Cette fonctionnalité permet de conserver vos configurations entre les sessions et améliore grandement l'expérience utilisateur.

## ✨ Fonctionnalités

### 🔄 Sauvegarde Automatique
- **Sauvegarde immédiate** lors de toute modification des champs
- **Persistence** entre les sessions et fermetures de navigateur
- **Chargement automatique** des paramètres au démarrage de l'application
- **Sauvegarde en temps réel** sans action utilisateur requise

### 🔒 Sécurité
- **Mot de passe exclu** de la sauvegarde pour des raisons de sécurité
- **Données locales** uniquement (jamais envoyées au serveur)
- **Nettoyage possible** avec boutons dédiés

### 📱 Interface Utilisateur
- **Indication visuelle** de la sauvegarde automatique
- **Boutons de contrôle** pour effacer les données sauvegardées
- **Message informatif** sous le titre principal

## 🛠️ Architecture Technique

### Services Créés

#### `LocalStorageService.cs`
```csharp
public interface ILocalStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
    Task ClearAsync();
}
```

**Fonctionnalités** :
- Sérialisation/désérialisation JSON automatique
- Gestion d'erreurs robuste
- Interface asynchrone pour compatibilité Blazor
- Support générique pour tous types de données

#### `SavedSftpParameters.cs`
```csharp
public class SavedSftpParameters
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    // Pas de Password pour la sécurité !
    public string RemotePath { get; set; }
    public string LocalPath { get; set; }
    public bool Recursive { get; set; }
    public string LogLevel { get; set; }
    public bool EnableFileLogging { get; set; }
    public DateTime LastSaved { get; set; }
}
```

#### `SftpParametersExtensions.cs`
```csharp
public static class SftpParametersExtensions
{
    public static SavedSftpParameters ToSaved(this SftpParameters parameters)
    public static void ApplyFrom(this SftpParameters parameters, SavedSftpParameters saved)
}
```

### Intégration dans l'Application

#### `Program.cs`
```csharp
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
```

#### `Index.razor`
- **Chargement automatique** : `OnInitializedAsync()` charge les paramètres sauvegardés
- **Sauvegarde en temps réel** : `@bind-Value:after="OnParameterChangedAsync"`
- **Méthodes de gestion** : `SaveParametersAsync()`, `LoadSavedParametersAsync()`, `ClearSavedParametersAsync()`

## 📋 Paramètres Sauvegardés

### ✅ Sauvegardés
- **Serveur SFTP** (host)
- **Port** (port)
- **Nom d'utilisateur** (username)
- **Chemin distant** (remotePath)
- **Chemin local** (localPath)
- **Copie récursive** (recursive)
- **Niveau de logging** (logLevel)
- **Logs dans fichier** (enableFileLogging)

### ❌ Non Sauvegardés (Sécurité)
- **Mot de passe** (password) - L'utilisateur doit le saisir à chaque session

## 🎯 Utilisation

### Sauvegarde Automatique
1. **Saisissez** vos paramètres de connexion
2. **Modifiez** n'importe quel champ du formulaire
3. **Sauvegarde immédiate** en arrière-plan
4. **Rechargez** la page - vos paramètres sont restaurés !

### Contrôles Manuels
- **Bouton "Effacer sauvegarde"** 🗑️ : Supprime toutes les données sauvegardées
- **Bouton "Réinitialiser"** 🔄 : Remet les valeurs par défaut ET efface la sauvegarde
- **Navigation avec explorateurs** : Sauvegarde automatique des chemins sélectionnés

## 🔧 Événements de Sauvegarde

### Déclencheurs Automatiques
- **Modification de champs** : `@bind-Value:after="OnParameterChangedAsync"`
- **Sélection avec explorateurs** : Après confirmation d'un chemin
- **Soumission du formulaire** : Avant démarrage de l'opération SFTP

### Méthodes Internes
```csharp
// Sauvegarde immédiate
private async Task OnParameterChangedAsync()

// Chargement au démarrage
private async Task LoadSavedParametersAsync()

// Nettoyage
private async Task ClearSavedParametersAsync()
```

## 💾 Stockage Local

### Clés localStorage
- `"sftpcopy-parameters"` : Paramètres JSON sérialisés
- `"sftpcopy-last-update"` : Timestamp de dernière sauvegarde

### Gestion d'Erreurs
- **Désactivation localStorage** : Dégradation gracieuse, pas de blocage
- **Quota dépassé** : Erreurs ignorées silencieusement
- **Corruption JSON** : Valeurs par défaut utilisées
- **Exceptions JS** : Application continue de fonctionner

## 🌐 Compatibilité

- **Navigateurs modernes** : Chrome, Firefox, Safari, Edge
- **Mode privé** : Fonctionne mais données perdues à la fermeture
- **Cookies désactivés** : N'affecte pas localStorage
- **JavaScript requis** : Fonctionnalité désactivée si JS désactivé

## 🔍 Indicateurs Visuels

### Interface Utilisateur
- **Message informatif** : "Vos paramètres sont automatiquement sauvegardés"
- **Icône sauvegarde** : 💾 dans l'en-tête
- **Note sécurité** : "🔒 Non sauvegardé par sécurité" pour le mot de passe
- **Boutons d'action** : Contrôles clairs pour la gestion des données

Cette fonctionnalité améliore considérablement l'expérience utilisateur en évitant la re-saisie répétitive des paramètres de connexion tout en maintenant un niveau de sécurité approprié.
