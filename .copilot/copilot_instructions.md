# Guide de bonnes pratiques - Développement .NET

## Configuration du projet

### Standards de configuration recommandés
- **TargetFramework** : Utiliser la version LTS ou la plus récente selon les besoins
- **LangVersion** : `latest` pour les projets en production, `preview` pour l'exploration
- **Nullable** : `enable` (fortement recommandé pour les nouveaux projets)
- **ImplicitUsings** : `enable` pour réduire le boilerplate
- **ManagePackageVersionsCentrally** : `true` pour les solutions multi-projets
- **EnableNETAnalyzers** : `true` pour l'analyse statique

### Organisation de code recommandée
- **Structure claire** : Séparer les couches (domain, infrastructure, application)
- **Responsabilité unique** : Un fichier, une responsabilité
- **Namespaces cohérents** : Refléter la structure des dossiers
- **Tests organisés** : Miroir de la structure source

## Conventions de codage

### Style et formatage (recommandations)
1. **Indentation** : 4 espaces (configurable via EditorConfig)
2. **Accolades** : Style K&R ou Allman selon les préférences de l'équipe
3. **Ordre des modificateurs** : `public,private,protected,internal,static,extern,new,virtual,abstract,sealed,override,readonly,unsafe,volatile,async`
4. **Utilisation de var** : Privilégier `var` quand le type est évident
5. **Expression-bodied members** : Utiliser pour les membres simples
6. **Qualification des membres** : Toujours utiliser `this.`

### Documentation et annotations
1. **Documentation XML** : Obligatoire pour les APIs publiques
2. **Annotations nullable** : Utiliser les annotations appropriées (`[NotNull]`, `[CanBeNull]`, etc.)
3. **Attributs spécialisés** : `[PublicAPI]`, `[UsedImplicitly]` selon les besoins
4. **Commentaires** : Expliquer le "pourquoi", pas le "quoi"
5. **Langues** : Cohérence dans tout le projet (anglais recommandé pour l'open source)

### Conventions de nommage
1. **Classes** : PascalCase, noms descriptifs
2. **Méthodes** : PascalCase, verbes ou actions
3. **Propriétés** : PascalCase, noms ou adjectifs
4. **Variables locales** : camelCase
5. **Constantes** : PascalCase ou UPPER_CASE selon le contexte
6. **Interfaces** : Préfixer avec `I` (ex: `IRepository`)
7. **Classes d'extension** : Suffixer avec `Extensions` (ex: `StringExtensions`)
8. **Tests** : Noms explicites décrivant le comportement testé

## Patterns et bonnes pratiques

### Structure des classes
```csharp
namespace MonProjet.Domain; // File-scoped namespace

/// <summary>Description claire et concise.</summary>
[AttributesAppropriés]
public sealed class ExempleClasse : IDisposable
{
    // 1. Champs privés readonly
    private readonly IService _service;
    private readonly ILogger<ExempleClasse> _logger;
    
    // 2. Constructeurs avec validation
    public ExempleClasse(IService service, ILogger<ExempleClasse> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // 3. Propriétés publiques avec init-only si applicable
    public string Name { get; init; } = string.Empty;
    public int Count { get; private set; }
    
    // 4. Méthodes publiques
    public async Task<Result<string>> DoSomethingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Début de l'opération pour {Name}", Name);
            var result = await _service.ProcessAsync(Name, cancellationToken);
            Count++;
            return Result<string>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement de {Name}", Name);
            return Result<string>.Failure($"Erreur: {ex.Message}");
        }
    }
    
    // 5. Méthodes privées
    private void HelperMethod() { }
    
    // 6. IDisposable implementation
    public void Dispose()
    {
        // Cleanup resources
        GC.SuppressFinalize(this);
    }
}

/// <summary>Type de résultat générique pour la gestion d'erreurs.</summary>
public readonly record struct Result<T>
{
    public T? Value { get; init; }
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    
    public static Result<T> Success(T value) => new() { Value = value, IsSuccess = true };
    public static Result<T> Failure(string error) => new() { Error = error, IsSuccess = false };
}
```

### Classes d'extension
```csharp
/// <summary>Extensions pour le type String.</summary>
public static class StringExtensions
{
    /// <summary>Vérifie si la chaîne est null ou vide.</summary>
    /// <param name="value">La valeur à tester.</param>
    /// <returns>True si null ou vide, false sinon.</returns>
    [Pure]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
        => string.IsNullOrEmpty(value);
        
    /// <summary>Vérifie si la chaîne est null, vide ou ne contient que des espaces.</summary>
    /// <param name="value">La valeur à tester.</param>
    /// <returns>True si null, vide ou whitespace uniquement, false sinon.</returns>
    [Pure]
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
        => string.IsNullOrWhiteSpace(value);
}
```

### Tests unitaires
```csharp
/// <summary>Tests pour StringExtensions.</summary>
public sealed class StringExtensionsTests
{
    /// <summary>Teste IsNullOrEmpty avec une chaîne null.</summary>
    [Fact]
    public void IsNullOrEmpty_WithNull_ReturnsTrue()
    {
        // Arrange
        string? value = null;
        
        // Act
        var result = value.IsNullOrEmpty();
        
        // Assert
        result.Should().BeTrue(); // FluentAssertions
    }
    
    [Theory]
    [InlineData("", true)]
    [InlineData("test", false)]
    [InlineData(" ", false)]
    public void IsNullOrEmpty_WithVariousInputs_ReturnsExpectedResult(string value, bool expected)
    {
        // Act & Assert
        value.IsNullOrEmpty().Should().Be(expected);
    }
    
    // Test avec AutoFixture pour des données complexes
    [Theory, AutoData]
    public void IsNullOrEmpty_WithNonEmptyString_ReturnsFalse(string nonEmptyString)
    {
        // Arrange - AutoFixture garantit une chaîne non-vide
        Assume.That(!string.IsNullOrEmpty(nonEmptyString));
        
        // Act & Assert
        nonEmptyString.IsNullOrEmpty().Should().BeFalse();
    }
}
```

### Gestion des erreurs et sécurité
1. **Validation des paramètres** : Vérifier et lever `ArgumentNullException`, `ArgumentException`
2. **États invalides** : Utiliser `InvalidOperationException`
3. **Documentation** : Documenter toutes les exceptions possibles avec `<exception>`
4. **Fail-fast principle** : Détecter les erreurs le plus tôt possible
5. **Exception handling** : Attraper spécifiquement, éviter les catch génériques
6. **Sécurité** : Ne jamais exposer d'informations sensibles dans les messages d'erreur
7. **Logging sécurisé** : Sanitiser les données avant de les logger
8. **Validation des entrées** : Toujours valider les données venant de l'extérieur
9. **Secrets management** : Utiliser Azure Key Vault, user-secrets en développement

### Performance et optimisation
1. **Types performants** : Privilégier `Span<T>`, `Memory<T>`, `ReadOnlySpan<T>` pour éviter les allocations
2. **Allocations** : Éviter les allocations inutiles, utiliser les object pools et `ArrayPool<T>`
3. **Async/await** : Utiliser correctement les patterns asynchrones (`ConfigureAwait(false)`, `ValueTask`)
4. **Collections** : Choisir la collection appropriée selon l'usage (`ImmutableArray<T>`, `FrozenSet<T>`)
5. **Benchmarking** : Mesurer avant d'optimiser avec BenchmarkDotNet
6. **String manipulation** : Utiliser `StringBuilder` pour les concaténations multiples, `string.Create()` pour les cas avancés
7. **LINQ optimisé** : Préférer `Count > 0` à `Any()`, utiliser `TryGetNonEnumeratedCount()`
8. **Records** : Utiliser `record` et `record struct` pour l'immutabilité et les performances

## Gestion des dépendances et outils

### Gestion des packages
**Approches recommandées :**
- **Monorepo** : `Directory.Packages.props` pour la gestion centralisée
- **Projets multiples** : `PackageReference` avec versions cohérentes
- **Versioning** : Semantic Versioning (SemVer)
- **Sécurité** : Audit régulier des vulnérabilités

### Analyseurs statiques recommandés
- **Microsoft.CodeAnalysis.NetAnalyzers** : Analyseurs .NET officiels
- **StyleCop.Analyzers** : Cohérence du style de code
- **SonarAnalyzer.CSharp** : Détection de bugs et code smells
- **Roslynator** : Refactoring et améliorations de code
- **Microsoft.CodeAnalysis.PublicApiAnalyzers** : Gestion des APIs publiques
- **Security analyzers** : Microsoft.CodeAnalysis.BannedApiAnalyzers

## Principes de qualité et revue de code

### Checklist de qualité essentielle
- [ ] **Lisibilité** : Code auto-documenté avec noms explicites
- [ ] **Responsabilité unique** : Chaque classe/méthode a une seule raison de changer
- [ ] **Documentation** : APIs publiques documentées, commentaires utiles
- [ ] **Tests** : Couverture appropriée, tests lisibles et maintenables
- [ ] **Gestion d'erreurs** : Exceptions appropriées et bien documentées
- [ ] **Performance** : Pas de goulots d'étranglement évidents
- [ ] **Sécurité** : Validation des entrées, pas de données sensibles exposées
- [ ] **Nullabilité** : Annotations nullable correctes
- [ ] **Immutabilité** : Privilégier l'immutabilité quand possible
- [ ] **Async/await** : Patterns asynchrones corrects

### Anti-patterns à éviter
1. **God classes** : Classes trop grandes avec trop de responsabilités
2. **Magic numbers/strings** : Utiliser des constantes nommées
3. **Deep nesting** : Éviter l'imbrication excessive (max 3-4 niveaux)
4. **Primitive obsession** : Créer des types métier appropriés
5. **Exception swallowing** : Ne pas ignorer silencieusement les exceptions
6. **Mutabilité excessive** : Préférer l'immutabilité quand possible
7. **Couplage fort** : Utiliser l'injection de dépendances
8. **Code dupliqué** : Factoriser le code commun

### Principes SOLID
1. **Single Responsibility** : Une classe = une responsabilité
2. **Open/Closed** : Ouvert à l'extension, fermé à la modification
3. **Liskov Substitution** : Les sous-types doivent être substituables
4. **Interface Segregation** : Interfaces spécifiques plutôt que générales
5. **Dependency Inversion** : Dépendre des abstractions, pas des implémentations

## Outils et écosystème .NET

### Frameworks de tests recommandés
- **xUnit** : Framework moderne, extensible, support des async tests
- **NUnit** : Framework mature avec beaucoup de fonctionnalités
- **MSTest** : Intégré à Visual Studio, simple d'usage
- **Moq/NSubstitute** : Pour les mocks et stubs
- **FluentAssertions** : Assertions expressives et lisibles
- **AutoFixture** : Génération automatique de données de test
- **Testcontainers** : Tests d'intégration avec containers Docker
- **WireMock.NET** : Mock de services HTTP pour les tests

### Tests de performance
- **BenchmarkDotNet** : Standard pour les micro-benchmarks
- **NBomber** : Tests de charge et de stress
- **dotMemory/PerfView** : Analyse de la mémoire
- **Application Insights** : Monitoring en production

### Outils de développement
- **EditorConfig** : Configuration cohérente des éditeurs
- **GitVersion** : Versioning automatique basé sur Git
- **Coverlet** : Couverture de code pour .NET
- **ReportGenerator** : Rapports de couverture
- **dotnet format** : Formatage automatique du code
- **Central Package Management** : Gestion centralisée des versions de packages
- **Directory.Build.props** : Propriétés communes aux projets
- **Global.json** : Verrouillage de la version du SDK .NET
- **Husky.NET** : Git hooks pour .NET
- **CSharpier** : Formateur de code opinionated

### CI/CD et DevOps
- **GitHub Actions** : Intégration native avec GitHub
- **Azure DevOps** : Suite complète Microsoft
- **GitLab CI** : Intégré à GitLab
- **Docker** : Containerisation des applications
- **NuGet** : Distribution des packages

## Patterns de conception courants

### Patterns modernes .NET 6+
- **Minimal APIs** : APIs légères avec configuration fluide
- **Global using** : Réduction du boilerplate avec les usings globaux
- **File-scoped namespaces** : Réduction de l'indentation
- **Init-only properties** : Immutabilité avec `init` accessor
- **Record types** : Types immuables avec égalité par valeur
- **Pattern matching** : Switch expressions et property patterns

### Patterns créationnels
- **Factory Method** : Création d'objets sans spécifier leur classe exacte
- **Builder** : Construction d'objets complexes étape par étape
- **Singleton** : Une seule instance d'une classe (utiliser l'injection de dépendances)

### Patterns structurels
- **Adapter** : Interface entre deux classes incompatibles
- **Decorator** : Ajouter des fonctionnalités sans modifier la classe
- **Facade** : Interface simplifiée pour un système complexe

### Patterns comportementaux
- **Strategy** : Encapsuler des algorithmes interchangeables
- **Observer** : Notification automatique des changements d'état
- **Command** : Encapsuler une requête en tant qu'objet
- **Template Method** : Définir le squelette d'un algorithme

---

## Ressources et références

### Documentation officielle
- [Microsoft .NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [C# Programming Guide](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [.NET API Reference](https://docs.microsoft.com/en-us/dotnet/api/)

### Guides de style
- [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [StyleCop Rules](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/DOCUMENTATION.md)

### Livres recommandés
- "Clean Code" par Robert C. Martin
- "Effective C#" par Bill Wagner
- "C# in Depth" par Jon Skeet
- "Patterns of Enterprise Application Architecture" par Martin Fowler

### Communautés
- [.NET Community](https://dotnet.microsoft.com/en-us/community)
- [C# Corner](https://www.c-sharpcorner.com/)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/c%23)
