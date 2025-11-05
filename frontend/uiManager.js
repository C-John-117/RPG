// Affiche un indicateur de chargement
function showLoadingIndicator(x, y) {
    const button = document.querySelector(`[data-pos="${x},${y}"] .load-btn`);
    if (button) {
        button.textContent = 'Chargement...';
        button.disabled = true;
    }
}

// Cache l'indicateur de chargement
function hideLoadingIndicator() {
    const buttons = document.querySelectorAll('.load-btn');
    buttons.forEach(button => {
        if (button.textContent === 'Chargement...') {
            button.textContent = 'Charger';
            button.disabled = false;
        }
    });
}

// Affiche un message d'erreur à l'utilisateur
function showErrorMessage(message) {
    // Créer ou récupérer l'élément d'erreur
    let errorDiv = document.getElementById('error-message');
    if (!errorDiv) {
        errorDiv = document.createElement('div');
        errorDiv.id = 'error-message';
        errorDiv.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: #e74c3c;
            color: white;
            padding: 10px 15px;
            border-radius: 5px;
            z-index: 1000;
            display: none;
        `;
        document.body.appendChild(errorDiv);
    }

    errorDiv.textContent = message;
    errorDiv.style.display = 'block';

    // Cacher le message après 5 secondes
    setTimeout(() => {
        errorDiv.style.display = 'none';
    }, 5000);
}

// fonction pour afficher CONNECTÉ ou DÉCONNECTÉ
function statutUtilisateur(text) {
  const el = document.getElementById('auth-status');
  if (el) el.textContent = text;
}

function updateCharacterPanel() {
  if (!personnage) return;
  const pos = document.getElementById('position');
  if (pos) pos.textContent = `(${personnage.positionX}, ${personnage.positionY})`;
}

// Met à jour tous les spans du panneau "Personnage"
function updateCharacterPanel() {
  if (!personnage) return;

  document.getElementById('p-nom').textContent     = personnage.nom;
  document.getElementById('p-niveau').textContent  = personnage.niveau;
  document.getElementById('p-xp').textContent      = personnage.experience;
  document.getElementById('p-force').textContent   = personnage.force;
  document.getElementById('p-defense').textContent = personnage.defense;


  document.getElementById('p-pv').textContent    = personnage.pointsVie;
  document.getElementById('p-pvmax').textContent = personnage.pointsVieMax;

  //  Barre de PV : calculer le pourcentage en évitant la division par 0
  const pvMax = personnage.pointsVieMax > 0 ? personnage.pointsVieMax : 1;
  const pvPct = Math.max(0, Math.min(100, Math.round((personnage.pointsVie / pvMax) * 100)));
  document.getElementById('p-pvbar').style.width = pvPct + '%';

  // 4) Position (sert aussi pour l'en-tête haut)
  document.getElementById('p-x').textContent = personnage.positionX;
  document.getElementById('p-y').textContent = personnage.positionY;

  // Si tu veux aussi maj le header déjà présent :
  const posHeader = document.getElementById('position');
  if (posHeader) posHeader.textContent = `(${personnage.positionX}, ${personnage.positionY})`;
}

// Remet des valeurs neutres quand on se déconnecte
function clearCharacterPanel() {
  const ids = ['p-nom','p-niveau','p-xp','p-pv','p-pvmax','p-force','p-defense','p-x','p-y'];
  for (let i = 0; i < ids.length; i++) {
    const el = document.getElementById(ids[i]);
    if (el) el.textContent = '—';
  }
  const bar = document.getElementById('p-pvbar');
  if (bar) bar.style.width = '0%';
}

// on sassure que les champs de connaxion er inscroption seront cachés
function setLoginVisible(visible) {
  ['login-email','login-password','btn-login','register-block'].forEach(id => {
    const el = document.getElementById(id);
    if (el) el.style.display = visible ? '' : 'none';
  });
  const logout = document.getElementById('btn-logout');
  if (logout) logout.style.display = visible ? 'none' : '';
}

// Cacher de decacher les sections en fonction de si on n'est connecté ou pas
function setProtectedVisible(visible) {
  const protectedEls = document.querySelectorAll('.guarded');
  for (let i = 0; i < protectedEls.length; i++) {
    if (visible) protectedEls[i].classList.remove('hidden');
    else protectedEls[i].classList.add('hidden');
  }
}

function setAuthStatus(text) {
  const el = document.getElementById('auth-status');
  if (el) el.textContent = text;
}

function toggleAuthButtons(connected) {
  const btnLogout = document.getElementById('btn-logout');
  const btnLogin  = document.getElementById('btn-login');
  const btnReg    = document.getElementById('btn-register');

  if (btnLogout) btnLogout.disabled = !connected;
  if (btnLogin)  btnLogin.disabled  = connected;
  if (btnReg)    btnReg.disabled    = connected;
}

// Met à jour l'interface
function updateUI() {
    document.getElementById('tuiles-loaded').textContent = Object.keys(tuiles).length;
    const posText = personnage
  ? `(${personnage.positionX}, ${personnage.positionY})`
  : `(${POSITION_DEPART_X}, ${POSITION_DEPART_Y})`;
document.getElementById('position').textContent = posText;

  // borne de la zone affichée
  const xMin = vueOrigineX;
  const yMin = vueOrigineY;
  const xMax = vueOrigineX + TAILLE_VUE - 1;
  const yMax = vueOrigineY + TAILLE_VUE - 1;

  const zoneText = `Zone: (${xMin}, ${yMin}) - (${xMax}, ${yMax})`;
  const zoneInfoEl = document.getElementById('zone-info');
  if (zoneInfoEl) zoneInfoEl.textContent = zoneText;
}

// Couleurs de fallback
function getTuileColor(type) {
    const colors = {
        'HERBE': '#7bed9f',
        'EAU': '#1e90ff',
        'MONTAGNE': '#a4b0be',
        'FORET': '#2ecc40',
        'VILLE': '#ff6348',
        'ROUTE': '#ffe066'
    };
    return colors[type] || '#34495e';
}

// Affiche le résultat d'un combat dans l'interface
function afficherResultatCombat(combat, resultat) {
  const message = document.createElement('div');
  message.className = 'combat-notification';

  let contenu = '<h3>Combat!</h3>';
  contenu += `<p>Dégâts au monstre: <span class="degats-monstre">${combat.degatsAuMonstre}</span></p>`;
  contenu += `<p>Dégâts reçus: <span class="degats-joueur">${combat.degatsAuJoueur}</span></p>`;

  if (combat.joueurVainqueur) {
    contenu += `<p class="victoire">Victoire!</p>`;
    contenu += `<p>PV restants: ${combat.hpJoueurApres}/${personnage.pointsVieMax}</p>`;
  } else if (combat.monstreVainqueur) {
    contenu += `<p class="defaite">Défaite...</p>`;
  } else {
    contenu += `<p>Combat en cours...</p>`;
    contenu += `<p>Tes PV: ${combat.hpJoueurApres} | Monstre: ${combat.hpMonstreApres}</p>`;
  }

  message.innerHTML = contenu;

  // Ajouter au DOM
  const container = document.getElementById('combat-notifications') || document.body;
  container.appendChild(message);

  // Retirer après 4 secondes
  setTimeout(() => {
    message.classList.add('fade-out');
    setTimeout(() => message.remove(), 500);
  }, 4000);
}

// Mode clair par défaut
if (!document.body.classList.contains('light-mode')) {
  document.body.classList.add('light-mode');
}

const toggleThemeBtn = document.getElementById('toggle-theme');
if (toggleThemeBtn) {
  toggleThemeBtn.onclick = () => {
    if (document.body.classList.contains('light-mode')) {
      document.body.classList.remove('light-mode');
      document.body.classList.add('dark-mode');
      toggleThemeBtn.textContent = 'Mode clair';
    } else {
      document.body.classList.remove('dark-mode');
      document.body.classList.add('light-mode');
      toggleThemeBtn.textContent = 'Mode sombre';
    }
  };
}

// Export des fonctions
window.showLoadingIndicator = showLoadingIndicator;
window.hideLoadingIndicator = hideLoadingIndicator;
window.showErrorMessage = showErrorMessage;
window.statutUtilisateur = statutUtilisateur;
window.updateCharacterPanel = updateCharacterPanel;
window.clearCharacterPanel = clearCharacterPanel;
window.setLoginVisible = setLoginVisible;
window.setProtectedVisible = setProtectedVisible;
window.setAuthStatus = setAuthStatus;
window.toggleAuthButtons = toggleAuthButtons;
window.updateUI = updateUI;
window.getTuileColor = getTuileColor;
window.afficherResultatCombat = afficherResultatCombat;