// Script de test pour vérifier le fonctionnement du localStorage
// À exécuter dans la console du navigateur (F12)

console.log("=== Test localStorage SFTP Copy Tool ===");

// Vérifier si localStorage est disponible
if (typeof(Storage) !== "undefined") {
    console.log("✅ localStorage est supporté");

    // Afficher les clés existantes
    console.log("📋 Clés localStorage existantes:");
    for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        if (key.startsWith("sftpcopy")) {
            const value = localStorage.getItem(key);
            console.log(`  ${key}: ${value}`);
        }
    }

    // Tester la sauvegarde
    const testParams = {
        host: "test.example.com",
        port: 22,
        username: "testuser",
        remotePath: "/home/test",
        localPath: "/downloads/test",
        recursive: true,
        logLevel: "Information",
        enableFileLogging: true,
        lastSaved: new Date().toISOString()
    };

    console.log("💾 Test de sauvegarde...");
    localStorage.setItem("sftpcopy-parameters", JSON.stringify(testParams));
    localStorage.setItem("sftpcopy-last-update", new Date().toISOString());

    // Vérifier la lecture
    const saved = JSON.parse(localStorage.getItem("sftpcopy-parameters"));
    console.log("📖 Données lues:", saved);

    // Nettoyage du test
    console.log("🧹 Nettoyage du test...");
    // Décommentez pour nettoyer :
    // localStorage.removeItem("sftpcopy-parameters");
    // localStorage.removeItem("sftpcopy-last-update");

} else {
    console.log("❌ localStorage n'est pas supporté");
}

console.log("=== Test terminé ===");

// Fonctions utiles pour debug
window.sftpDebug = {
    showSaved: () => {
        const params = localStorage.getItem("sftpcopy-parameters");
        const lastUpdate = localStorage.getItem("sftpcopy-last-update");
        console.log("Paramètres sauvegardés:", JSON.parse(params || "null"));
        console.log("Dernière mise à jour:", lastUpdate);
    },

    clearAll: () => {
        localStorage.removeItem("sftpcopy-parameters");
        localStorage.removeItem("sftpcopy-last-update");
        console.log("Données effacées");
    }
};

console.log("🔧 Fonctions disponibles: sftpDebug.showSaved(), sftpDebug.clearAll()");
