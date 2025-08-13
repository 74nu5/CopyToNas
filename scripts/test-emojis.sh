#!/bin/bash

# Script de test pour voir les nouveaux logs avec emojis
# Ce script simule un appel avec des paramètres invalides pour voir les logs d'erreur

echo "=== Test des logs avec emojis ==="
echo

echo "Test 1: Paramètres manquants (pour voir l'aide)"
echo "------------------------------------------------"
dotnet run -- --help

echo
echo "Test 2: Connexion avec serveur inexistant (pour voir les logs d'erreur)"
echo "--------------------------------------------------------------------"
echo "Note: Ce test va échouer volontairement pour montrer les logs d'erreur avec emojis"

# Test avec un serveur inexistant pour déclencher les logs d'erreur
dotnet run -- \
    --host "serveur-inexistant.local" \
    --username "test" \
    --password "test" \
    --remote-path "/test" \
    --local-path "/tmp/test" || echo "Test terminé (erreur attendue)"

echo
echo "=== Fin des tests ==="
