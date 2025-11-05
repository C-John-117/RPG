// État auth
// Note : Plus de token JWT, on utilise juste la présence d'un personnage pour vérifier la connexion
let personnage = null;

function wireAuthUI() {
  const btnRegister = document.getElementById('btn-register');
  const btnLogin = document.getElementById('btn-login');
  const btnLogout   = document.getElementById('btn-logout');

  if (btnRegister) {

    btnRegister.onclick = async () => {
      try {
        const email = document.getElementById('reg-email').value.trim();
        const pseudo = document.getElementById('reg-pseudo').value.trim();
        const password = document.getElementById('reg-password').value;
        await enregsitrementUtilisateur(email, pseudo, password);
        showErrorMessage('Compte créé. Connecte-toi maintenant.');
      } catch (e) {
        showErrorMessage(e.message);
      }
    };

      // pour gerer le fait que deconnecter

    if (btnLogout) {
      btnLogout.onclick = async () => {
        try {
          await fetch(`${API_BASE_URL}/auth/logout`, { method: 'POST' });
        } catch (_) {
        }
        resetAuthState();
      };
    }





}

  if (btnLogin) {
    btnLogin.onclick = async () => {
      try {
        const email = document.getElementById('login-email').value.trim();
        const password = document.getElementById('login-password').value;
        const payload = await connexion(email, password);

        personnage = payload.personnage;
        // S'assurer que les propriétés sont accessibles (ASP.NET sérialise en camelCase)
        personnage.utilisateurId = personnage.utilisateurId || personnage.utilisateurid;
        personnage.positionX = personnage.positionX || personnage.positionx;
        personnage.positionY = personnage.positionY || personnage.positiony;
        // Mettre à jour les variables globales
        window.personnage = personnage;
        console.log("Personnage après login:", personnage);
        positionPersoX = personnage.positionX || personnage.positionx;
        positionPersoY = personnage.positionY || personnage.positiony;



        statutUtilisateur('Connecté');
        setLoginVisible(false);
        toggleAuthButtons(true);
        // Après login réussi
        updateCharacterPanel();

        // Afficher les sections protégées si tu ne le fais pas déjà
        setProtectedVisible(true);


        updateCharacterPanel();
        afficherCarte();  // tu l'as déjà
        updateUI();   // tu l'as déjà
        brancherBoutonsNavigation();  // Brancher les boutons après que les éléments soient visibles
      } catch (e) {
        showErrorMessage(e.message);
        statutUtilisateur('Déconnecté');
      }
    };

  }
}

function resetAuthState() {
  personnage = null;
  // Mettre à jour les variables globales
  window.personnage = personnage;
  setAuthStatus('Déconnecté');
  toggleAuthButtons(false);
  setLoginVisible(true);

  // on vide ou efface les infos du personnage
  clearCharacterPanel();

  // reafficher le bouton créer le compte ici
  const registerBlock = document.getElementById('register-block');
  if (registerBlock) registerBlock.style.display = 'block';
  setProtectedVisible(false);
  afficherCarte();
  updateUI();
}

// Export des variables et fonctions
// Note: window.token et window.personnage sont mis à jour dynamiquement lors du login/logout
window.wireAuthUI = wireAuthUI;
window.resetAuthState = resetAuthState;