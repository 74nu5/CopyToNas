#!/bin/bash

# Script de démonstration des logs avec emojis
# Ce script montre comment les nouveaux logs apparaissent

echo "=== Démonstration des Logs avec Emojis SftpCopyTool ==="
echo

echo "Voici comment les nouveaux logs avec emojis apparaissent :"
echo

echo "1️⃣  Lors de la connexion :"
echo "   🔌 Connexion au serveur SFTP exemple.com:22"
echo "   ✅ Connexion établie avec succès"
echo

echo "2️⃣  Lors de la copie d'un fichier :"
echo "   📄 Copie du fichier '/remote/document.pdf' vers '/local/downloads/'"
echo "   ⬇️ Téléchargement : /remote/document.pdf -> /local/downloads/document.pdf"
echo "   ✅ Fichier copié : 2048576 octets"
echo

echo "3️⃣  Lors de la copie récursive :"
echo "   📂 Copie récursive du dossier '/remote/documents' vers '/local/backup/'"
echo "   📁 Dossier créé : /local/backup/documents"
echo "   📂 Traitement du dossier : /remote/documents/photos"
echo "   📁 Dossier créé : /local/backup/documents/photos"
echo "   ⬇️ Téléchargement : /remote/documents/photos/photo1.jpg -> /local/backup/documents/photos/photo1.jpg"
echo "   ✅ Fichier copié : 1024768 octets"
echo

echo "4️⃣  En cas de succès :"
echo "   🎉 Copie terminée avec succès"
echo "   🔌 Déconnexion du serveur SFTP"
echo

echo "5️⃣  En cas d'erreur :"
echo "   ❌ Erreur lors de la copie SFTP : Connection timeout"
echo

echo "═══════════════════════════════════════════════════════════"
echo "✨ Ces emojis rendent les logs plus lisibles et visuellement agréables !"
echo "📖 Consultez docs/EMOJI_LOGS.md pour plus de détails"
echo "═══════════════════════════════════════════════════════════"
