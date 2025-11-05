// Gestionnaire de mouvement du personnage
async function deplacerPersonnage(direction) {
  // Vérifier que l'utilisateur a un personnage (s'est connecté)
  if (!window.personnage) {
    showErrorMessage('Vous devez être connecté pour vous déplacer');
    return;
  }

  // Calculer la nouvelle position basée sur la direction
  // Utiliser la position actuelle du personnage (synchronisée avec le backend)
  const currentX = window.personnage?.positionX ?? positionPersoX;
  const currentY = window.personnage?.positionY ?? positionPersoY;

  const dx = { 'up': 0, 'down': 0, 'left': -1, 'right': 1 }[direction] || 0;
  const dy = { 'up': -1, 'down': 1, 'left': 0, 'right': 0 }[direction] || 0;

  const nouvellePositionX = currentX + dx;
  const nouvellePositionY = currentY + dy;

  console.log('Position actuelle du personnage:', { currentX, currentY, fromPersonnage: window.personnage?.positionX + ',' + window.personnage?.positionY, fromGlobal: positionPersoX + ',' + positionPersoY });
  console.log('Nouvelle position calculée:', { nouvellePositionX, nouvellePositionY, direction, dx, dy });

  // Utiliser l'API backend pour le déplacement et le combat automatique
  try {
    const resultat = await deplacerPersonnageAPI(window.personnage.utilisateurId, nouvellePositionX, nouvellePositionY);
    console.log('Résultat du déplacement:', resultat);
    console.log('nouvellePosition détaillée:', resultat.nouvellePosition);
    console.log('Propriétés de nouvellePosition:', Object.keys(resultat.nouvellePosition));

    // Traiter le résultat du combat si présent
    if (resultat.combat) {
      const combat = resultat.combat;
      if (combat.joueurVainqueur) {
        showSuccessMessage(`Combat gagné ! Dégâts infligés: ${combat.degatsAuMonstre}, PV restants: ${combat.hpJoueurApres}/${resultat.joueur.pointsVieMax}`);
      } else if (combat.monstreVainqueur) {
        showErrorMessage(`Combat perdu ! Vous avez été téléporté à votre domicile. Dégâts subis: ${combat.degatsAuJoueur}`);
      } else {
        // Combat en cours (les deux encore en vie)
        showErrorMessage(`Combat en cours ! Dégâts infligés: ${combat.degatsAuMonstre}, Dégâts subis: ${combat.degatsAuJoueur}. PV restants: ${combat.hpJoueurApres}/${resultat.joueur.pointsVieMax}`);
      }
    }

    // Mettre à jour la position locale depuis la réponse
    if (resultat.nouvellePosition) {
      console.log('Avant mise à jour - positionPersoX:', positionPersoX, 'positionPersoY:', positionPersoY);
      console.log('Valeurs extraites - X:', resultat.nouvellePosition.X, 'Y:', resultat.nouvellePosition.Y);
      console.log('Valeurs extraites - x:', resultat.nouvellePosition.x, 'y:', resultat.nouvellePosition.y);
      positionPersoX = resultat.nouvellePosition.X || resultat.nouvellePosition.x;
      positionPersoY = resultat.nouvellePosition.Y || resultat.nouvellePosition.y;
      console.log('Après mise à jour - positionPersoX:', positionPersoX, 'positionPersoY:', positionPersoY);
      // Mettre à jour aussi le personnage global
      if (window.personnage) {
        window.personnage.positionX = positionPersoX;
        window.personnage.positionY = positionPersoY;
        // Mettre à jour aussi les autres stats si elles ont changé
        if (resultat.joueur) {
          window.personnage.niveau = resultat.joueur.niveau;
          window.personnage.experience = resultat.joueur.experience;
          window.personnage.pointsVie = resultat.joueur.pointsVie;
          window.personnage.pointsVieMax = resultat.joueur.pointsVieMax;
          window.personnage.force = resultat.joueur.force;
          window.personnage.defense = resultat.joueur.defense;
        }
      }
      console.log('Position mise à jour:', { positionPersoX, positionPersoY });
    } else {
      console.warn('NouvellePosition manquante dans la réponse, position inchangée');
    }

    // Sauvegarder la direction pour l'animation du sprite
    lastMovementDirection = direction;

    // Mettre à jour la vue si nécessaire
    const vueCentreX = vueOrigineX + Math.floor(TAILLE_VUE / 2);
    const vueCentreY = vueOrigineY + Math.floor(TAILLE_VUE / 2);

    if (Math.abs(positionPersoX - vueCentreX) > 1 || Math.abs(positionPersoY - vueCentreY) > 1) {
      vueOrigineX = Math.max(0, Math.min(TAILLE_CARTE - TAILLE_VUE, positionPersoX - Math.floor(TAILLE_VUE / 2)));
      vueOrigineY = Math.max(0, Math.min(TAILLE_CARTE - TAILLE_VUE, positionPersoY - Math.floor(TAILLE_VUE / 2)));
    }

    // Charger les 8 tuiles adjacentes autour de la nouvelle position
    await chargerTuilesAdjacentes(positionPersoX, positionPersoY);

    // Mettre à jour l'interface
    afficherCarte();
    updateUI();
    updateCharacterPanel();
    majTuileSelectionneeSurPerso();

  } catch (error) {
    console.error('Erreur lors du déplacement:', error);
    showErrorMessage('Erreur lors du déplacement');
  }
}

// Fonction pour lancer un combat contre un monstre (obsolète - le combat est maintenant automatique via l'API de déplacement)
async function lancerCombat(monstre) {
  console.warn('lancerCombat() est obsolète. Le combat se fait automatiquement lors du déplacement via deplacerPersonnageAPI()');
  showErrorMessage('Le combat se déclenche automatiquement lorsque vous vous déplacez sur une tuile contenant un monstre.');
  return { victoire: false, message: 'Combat automatique lors du déplacement' };
}

// Fonction pour brancher les boutons de navigation
function brancherBoutonsNavigation() {
  // Boutons de direction
  const btnNord = document.getElementById('btn-nord');
  const btnSud = document.getElementById('btn-sud');
  const btnOuest = document.getElementById('btn-ouest');
  const btnEst = document.getElementById('btn-est');

  if (btnNord) btnNord.onclick = () => deplacerPersonnage('up');
  if (btnSud) btnSud.onclick = () => deplacerPersonnage('down');
  if (btnOuest) btnOuest.onclick = () => deplacerPersonnage('left');
  if (btnEst) btnEst.onclick = () => deplacerPersonnage('right');

  // Gestion des touches clavier
  document.addEventListener('keydown', (event) => {
    switch(event.key) {
      case 'ArrowUp':
      case 'z':
      case 'Z':
        event.preventDefault();
        deplacerPersonnage('up');
        break;
      case 'ArrowDown':
      case 's':
      case 'S':
        event.preventDefault();
        deplacerPersonnage('down');
        break;
      case 'ArrowLeft':
      case 'q':
      case 'Q':
        event.preventDefault();
        deplacerPersonnage('left');
        break;
      case 'ArrowRight':
      case 'd':
      case 'D':
        event.preventDefault();
        deplacerPersonnage('right');
        break;
    }
  });
}

// Export des fonctions
window.deplacerPersonnage = deplacerPersonnage;
window.lancerCombat = lancerCombat;
window.brancherBoutonsNavigation = brancherBoutonsNavigation;