window.sftpCopyTool = {
    // Fonction pour afficher un toast de confirmation de sauvegarde
    showSaveNotification: function() {
        // Créer un toast simple
        const toast = document.createElement('div');
        toast.className = 'position-fixed top-0 end-0 m-3 alert alert-success alert-dismissible fade show';
        toast.style.zIndex = '9999';
        toast.innerHTML = `
            <i class="fas fa-check-circle me-2"></i>
            Paramètres sauvegardés
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(toast);

        // Auto-suppression après 2 secondes
        setTimeout(() => {
            if (toast.parentNode) {
                toast.classList.remove('show');
                setTimeout(() => {
                    if (toast.parentNode) {
                        document.body.removeChild(toast);
                    }
                }, 150);
            }
        }, 2000);
    },

    // Fonction pour debug localStorage
    debugLocalStorage: function() {
        console.group('SFTP Copy Tool - LocalStorage Debug');

        const params = localStorage.getItem('sftpcopy-parameters');
        const lastUpdate = localStorage.getItem('sftpcopy-last-update');

        if (params) {
            console.log('📋 Paramètres sauvegardés:', JSON.parse(params));
            console.log('🕒 Dernière sauvegarde:', new Date(lastUpdate || '').toLocaleString());
        } else {
            console.log('ℹ️ Aucune sauvegarde trouvée');
        }

        console.groupEnd();
    }
};

// Fonction globale pour faciliter les tests
window.debugSFTP = window.sftpCopyTool.debugLocalStorage;
