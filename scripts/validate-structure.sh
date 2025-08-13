#!/bin/bash

# Script de validation de la structure du projet
# Ce script vérifie que tous les fichiers sont à leur place et que les chemins sont corrects

echo "=== Validation de la structure du projet SftpCopyTool ==="
echo

# Couleurs
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Variables de validation
ERRORS=0
WARNINGS=0

# Fonction de validation
validate_file() {
    local file="$1"
    local description="$2"
    
    if [ -f "$file" ]; then
        echo -e "${GREEN}✓${NC} $description: $file"
    else
        echo -e "${RED}✗${NC} $description: $file (manquant)"
        ((ERRORS++))
    fi
}

validate_dir() {
    local dir="$1"
    local description="$2"
    
    if [ -d "$dir" ]; then
        echo -e "${GREEN}✓${NC} $description: $dir"
    else
        echo -e "${RED}✗${NC} $description: $dir (manquant)"
        ((ERRORS++))
    fi
}

# Vérifier la structure principale
echo "--- Structure des dossiers ---"
validate_dir "../src" "Dossier source"
validate_dir "../scripts" "Dossier scripts"
validate_dir "../config" "Dossier configuration"
validate_dir "../docs" "Dossier documentation"

echo
echo "--- Fichiers source ---"
validate_file "../src/Program.cs" "Fichier principal"
validate_file "../src/SftpService.cs" "Service SFTP"
validate_file "../src/SftpCopyTool.csproj" "Fichier projet"

echo
echo "--- Scripts ---"
validate_file "../scripts/build.sh" "Script de build"
validate_file "../scripts/deploy.sh" "Script de déploiement"
validate_file "../scripts/install.sh" "Script d'installation"
validate_file "../scripts/uninstall.sh" "Script de désinstallation"
validate_file "../scripts/run-example.sh" "Script d'exemple"

echo
echo "--- Configuration ---"
validate_file "../config/config.env" "Configuration environnement"
validate_file "../config/sftp-copy.service" "Service systemd"
validate_file "../config/sftp-copy.service.example" "Exemple de service"

echo
echo "--- Documentation ---"
validate_file "../README.md" "README principal"
validate_file "../docs/BUILD_GUIDE.md" "Guide de build"
validate_file "../.gitignore" "GitIgnore"
validate_file "../LICENSE" "Licence"

# Vérifier les permissions des scripts
echo
echo "--- Permissions des scripts ---"
for script in ../scripts/*.sh; do
    if [ -f "$script" ]; then
        if [ -x "$script" ]; then
            echo -e "${GREEN}✓${NC} Exécutable: $(basename "$script")"
        else
            echo -e "${YELLOW}⚠${NC} Non exécutable: $(basename "$script") (peut nécessiter chmod +x)"
            ((WARNINGS++))
        fi
    fi
done

# Vérifier les références de chemins dans les fichiers
echo
echo "--- Validation des chemins dans les fichiers ---"

# Vérifier README.md
if grep -q "../src" ../README.md && grep -q "../scripts" ../README.md; then
    echo -e "${GREEN}✓${NC} README.md contient les bons chemins relatifs"
else
    echo -e "${YELLOW}⚠${NC} README.md pourrait avoir des chemins incorrects"
    ((WARNINGS++))
fi

# Vérifier les scripts
for script in ../scripts/*.sh; do
    if [ -f "$script" ]; then
        script_name=$(basename "$script")
        if grep -q "\.\./src" "$script" 2>/dev/null; then
            echo -e "${GREEN}✓${NC} $script_name utilise les bons chemins relatifs"
        else
            if [[ "$script_name" == "run-example.sh" ]]; then
                echo -e "${GREEN}✓${NC} $script_name (pas de vérification de chemin nécessaire)"
            else
                echo -e "${YELLOW}⚠${NC} $script_name pourrait avoir des chemins incorrects"
                ((WARNINGS++))
            fi
        fi
    fi
done

# Résumé
echo
echo "=== Résumé de la validation ==="
if [ $ERRORS -eq 0 ] && [ $WARNINGS -eq 0 ]; then
    echo -e "${GREEN}✓ Tout est parfait ! Le projet est bien organisé.${NC}"
    exit 0
elif [ $ERRORS -eq 0 ]; then
    echo -e "${YELLOW}⚠ $WARNINGS avertissement(s) trouvé(s), mais pas d'erreurs critiques.${NC}"
    exit 0
else
    echo -e "${RED}✗ $ERRORS erreur(s) et $WARNINGS avertissement(s) trouvé(s).${NC}"
    exit 1
fi
