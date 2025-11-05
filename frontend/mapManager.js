// Stockage des tuiles chargées depuis l'API
const tuiles = {};
let selectedTuile = null;
let positionPersoX = POSITION_DEPART_X;
let positionPersoY = POSITION_DEPART_Y;
let vueOrigineX = positionPersoX - Math.floor(TAILLE_VUE / 2);
let vueOrigineY = positionPersoY - Math.floor(TAILLE_VUE / 2);

// Fonction pour explorer dans une direction : décale la grille d'1 case, charge les nouvelles tuiles, le héros reste fixe
async function explorerDirection(dx, dy) {
  // Calculer la nouvelle origine de la vue (déplacement d'1 case seulement)
  let nouvelleOrigineX = vueOrigineX + dx;
  let nouvelleOrigineY = vueOrigineY + dy;

  // Borner pour rester dans la carte
  nouvelleOrigineX = Math.max(0, Math.min(TAILLE_CARTE - TAILLE_VUE, nouvelleOrigineX));
  nouvelleOrigineY = Math.max(0, Math.min(TAILLE_CARTE - TAILLE_VUE, nouvelleOrigineY));

  // Si on n'a pas bougé (bord de carte), on ne fait rien
  if (nouvelleOrigineX === vueOrigineX && nouvelleOrigineY === vueOrigineY) {
    return;
  }

  // Déterminer quelles nouvelles tuiles charger selon la direction
  const tuilesACharger = [];

  if (dx > 0) { // Est - charger la colonne de droite
    const colonneX = nouvelleOrigineX + TAILLE_VUE - 1;
    for (let y = nouvelleOrigineY; y < nouvelleOrigineY + TAILLE_VUE; y++) {
      tuilesACharger.push({x: colonneX, y: y});
    }
  } else if (dx < 0) { // Ouest - charger la colonne de gauche
    const colonneX = nouvelleOrigineX;
    for (let y = nouvelleOrigineY; y < nouvelleOrigineY + TAILLE_VUE; y++) {
      tuilesACharger.push({x: colonneX, y: y});
    }
  }

  if (dy > 0) { // Sud - charger la ligne du bas
    const ligneY = nouvelleOrigineY + TAILLE_VUE - 1;
    for (let x = nouvelleOrigineX; x < nouvelleOrigineX + TAILLE_VUE; x++) {
      tuilesACharger.push({x: x, y: ligneY});
    }
  } else if (dy < 0) { // Nord - charger la ligne du haut
    const ligneY = nouvelleOrigineY;
    for (let x = nouvelleOrigineX; x < nouvelleOrigineX + TAILLE_VUE; x++) {
      tuilesACharger.push({x: x, y: ligneY});
    }
  }

  // Mettre à jour l'origine de la vue (SANS déplacer le personnage)
  vueOrigineX = nouvelleOrigineX;
  vueOrigineY = nouvelleOrigineY;

  // Charger les nouvelles tuiles en parallèle (uniquement celles qui ne sont pas déjà chargées)
  const promisesChargement = tuilesACharger
    .filter(tuile => !tuiles[`${tuile.x},${tuile.y}`]) // Seulement les tuiles non chargées
    .map(tuile => loadTuile(tuile.x, tuile.y));

  if (promisesChargement.length > 0) {
    await Promise.all(promisesChargement);
  }

  // Rafraîchir l'affichage
  afficherCarte();
  updateUI();
}

// Charge une tuile via l'API à l'ecran et met à jour l'interface
async function loadTuile(x, y) {
    const key = `${x},${y}`;

    if (!tuiles[key]) {
        try {
            // Afficher un indicateur de chargement
            showLoadingIndicator(x, y);

            const tuile = await fetchTuileFromAPI(x, y);
            tuiles[key] = tuile;
            afficherCarte();
            updateUI();

        } catch (error) {
            console.error(`Erreur lors du chargement de la tuile (${x},${y}):`, error);
            showErrorMessage(`Impossible de charger la tuile (${x},${y})`);
        } finally {
            hideLoadingIndicator();
        }
    }
}

// Affiche la carte
function afficherCarte() {
  const divCarte = document.getElementById('map');
  divCarte.innerHTML = '';

  for (let ligne = 0; ligne < TAILLE_VUE; ligne++) {
    for (let col = 0; col < TAILLE_VUE; col++) {
      const tuileX = vueOrigineX + col;
      const tuileY = vueOrigineY + ligne;
      const cle = `${tuileX},${tuileY}`;
      const tuile = tuiles[cle];

      const divTuile = document.createElement('div');
      divTuile.className = 'tuile';
      divTuile.dataset.pos = cle;

      if (tuile) {
        const infoType = TUILE_TYPES[tuile.type];
        if (infoType) {
          const img = document.createElement('img');
          img.src = infoType.img;
          img.alt = tuile.type;
          divTuile.appendChild(img);
        }
      } else {
    divTuile.classList.add('unknown');
    divTuile.innerHTML = `
      <span class="tuile-icon">?</span>
      <button class="load-btn">Charger</button>
    `;
    divTuile.title = `Tuile non chargée (${tuileX},${tuileY})`;
    const btn = divTuile.querySelector('.load-btn');
    btn.onclick = (e) => {
      e.stopPropagation();
      loadTuile(tuileX, tuileY);
    };
  }


      // Sprite animé du personnage
      if (tuileX === positionPersoX && tuileY === positionPersoY) {
        // Déterminer la direction (à adapter selon votre logique de mouvement)
        let directionRow = 1; // Par défaut 'down'
        if (typeof lastMovementDirection !== 'undefined') {
          switch(lastMovementDirection) {
            case 'up': directionRow = 0; break;
            case 'down': directionRow = 1; break;
            case 'left': directionRow = 2; break;
            case 'right': directionRow = 3; break;
          }
        }
        // Créer le sprite animé
        const sprite = new SpriteAnimator('./img/characterSprite.png', 32, 32, 2, 2);
        sprite.playAnimation(directionRow, 0.75, true);
        divTuile.appendChild(sprite.element);
      }

      // Affichage du sprite du monstre si présent sur la tuile
      if (tuile && tuile.monstre) {
        const monstreImg = document.createElement('img');
        monstreImg.src = tuile.monstre.spriteURL || 'https://img.pokemondb.net/sprites/black-white/normal/rattata-f.png';
        monstreImg.alt = tuile.monstre.nom || 'Monstre';
        monstreImg.style.width = '64px';
        monstreImg.style.height = '64px';
        monstreImg.style.position = 'absolute';
        monstreImg.style.top = '50%';
        monstreImg.style.left = '50%';
        monstreImg.style.transform = 'translate(-50%, -50%)';
        monstreImg.style.zIndex = '11';
        divTuile.appendChild(monstreImg);
      }


      // Ajout du gestionnaire de clic pour afficher les infos de la tuile
      divTuile.onclick = (e) => {
        e.stopPropagation();
        const cle = `${tuileX},${tuileY}`;
        const tuile = tuiles[cle];
        if (tuile) selectTuile(tuile);
      };

      divCarte.appendChild(divTuile);
    }
  }
}

// Sélectionne une tuile
function selectTuile(tuile) {
    selectedTuile = tuile;
    document.getElementById('selected-pos').textContent = `(${tuile.positionX}, ${tuile.positionY})`;
    document.getElementById('selected-type').textContent = tuile.type;
    document.getElementById('selected-traversable').textContent = tuile.traversable ? 'Oui' : 'Non';
    document.getElementById('selected-desc').textContent = `Tuile ${tuile.type.toLowerCase()} en position (${tuile.positionX}, ${tuile.positionY})`;
    // Affichage des infos du monstre si présent
    const monstreInfo = document.getElementById('selected-monstre');
    const btnLoadMonstre = document.getElementById('btn-load-monstre');
    if (monstreInfo) {
      if (tuile.monstre) {
        monstreInfo.style.display = '';
        const m = tuile.monstre;
        // Utiliser les stats calculées (force, defense) ou forceBase/defenseBase en fallback
        const force = m.force || m.forceBase || '?';
        const defense = m.defense || m.defenseBase || '?';
        monstreInfo.innerHTML = `<strong>Monstre :</strong> ${m.nom} (Niv. ${m.niveau || '?'})<br>
          PV: ${m.pointsVieActuels || m.pointsVieMax || '?'}/${m.pointsVieMax || '?'}<br>
          Force: ${force} - Défense: ${defense}`;
        if (btnLoadMonstre) {
          btnLoadMonstre.style.display = '';
          btnLoadMonstre.onclick = () => chargerMonstreDansSimulateur(tuile.monstre);
        }
      } else {
        monstreInfo.style.display = 'none';
        monstreInfo.innerHTML = '';
        if (btnLoadMonstre) btnLoadMonstre.style.display = 'none';
      }
    }
    afficherCarte();
}

// Initialisation : charge automatiquement la position du personnage et les cases adjacentes
async function initializeApp() {
    afficherCarte();
    updateUI();
    wireAuthUI();
    // brancherBoutonsNavigation(); // Retiré - sera appelé après connexion réussie

    // Charger automatiquement la position de départ du personnage et les 8 cases adjacentes
    await chargerPositionInitialePersonnage();
}

// Charge les 8 tuiles adjacentes autour d'une position donnée
async function chargerTuilesAdjacentes(centreX, centreY) {
    const tuilesACharger = [];

    // Charger les 8 tuiles adjacentes + la tuile centrale si elle n'est pas chargée
    for (let dx = -1; dx <= 1; dx++) {
        for (let dy = -1; dy <= 1; dy++) {
            const x = centreX + dx;
            const y = centreY + dy;
            if (x >= 0 && x < TAILLE_CARTE && y >= 0 && y < TAILLE_CARTE) {
                tuilesACharger.push({ x, y });
            }
        }
    }

    // Charger toutes les tuiles en parallèle (uniquement celles non chargées)
    const promisesChargement = tuilesACharger
        .filter(pos => !tuiles[`${pos.x},${pos.y}`])
        .map(async (pos) => {
            try {
                await loadTuile(pos.x, pos.y);
            } catch (error) {
                console.warn(`Impossible de charger la tuile (${pos.x},${pos.y}):`, error);
            }
        });

    if (promisesChargement.length > 0) {
        await Promise.all(promisesChargement);
        console.log(`Chargé ${promisesChargement.length} tuiles adjacentes autour de (${centreX},${centreY})`);
    }
}

// Charge la position du personnage (10,10) et les 8 cases adjacentes au démarrage
async function chargerPositionInitialePersonnage() {
    console.log('Chargement de la position initiale du personnage...');

    // Position du personnage
    const centreX = POSITION_DEPART_X;
    const centreY = POSITION_DEPART_Y;

    // Charger les 9 tuiles (centre + 8 adjacentes)
    const tuilesACharger = [];

    for (let dx = -1; dx <= 1; dx++) {
        for (let dy = -1; dy <= 1; dy++) {
            const x = centreX + dx;
            const y = centreY + dy;
            tuilesACharger.push({ x, y });
        }
    }

    // Charger toutes les tuiles en parallèle pour optimiser
    const promisesChargement = tuilesACharger.map(async (pos) => {
        try {
            await loadTuile(pos.x, pos.y);
        } catch (error) {
            console.warn(`Impossible de charger la tuile (${pos.x},${pos.y}):`, error);
        }
    });

    await Promise.all(promisesChargement);
    // Injecte les monstres de test pour le développement
    injecterMonstresDeTest();
    console.log(`Position initiale chargée: ${tuilesACharger.length} tuiles autour de (${centreX},${centreY})`);
    // Rafraîchir l'affichage
    afficherCarte();
    updateUI();
}

/* pour la mise à jour des infos de la tuile qui est en desous du joueur*/

async function majTuileSelectionneeSurPerso() {
  const cle = `${positionPersoX},${positionPersoY}`;
  let tuileCourante = tuiles[cle];

  // Si pas en cache, on va la chercher et on la stocke
  if (!tuileCourante) {
    try {
      tuileCourante = await fetchTuileFromAPI(positionPersoX, positionPersoY);
      tuiles[cle] = tuileCourante;
    } catch (e) {
      console.error('Impossible de récupérer la tuile courante', e);
      return;
    }
  }

  // Mettre à jour le panneau
  selectTuile(tuileCourante);

  // S'assurer que le bloc est visible
  const bloc = document.querySelector('.selected-tile');
  if (bloc) bloc.classList.remove('hidden');
}

// Export des variables et fonctions
window.tuiles = tuiles;
window.selectedTuile = selectedTuile;
window.positionPersoX = positionPersoX;
window.positionPersoY = positionPersoY;
window.vueOrigineX = vueOrigineX;
window.vueOrigineY = vueOrigineY;
window.explorerDirection = explorerDirection;
window.loadTuile = loadTuile;
window.afficherCarte = afficherCarte;
window.selectTuile = selectTuile;
window.chargerPositionInitialePersonnage = chargerPositionInitialePersonnage;
window.chargerTuilesAdjacentes = chargerTuilesAdjacentes;
window.initializeApp = initializeApp;
window.majTuileSelectionneeSurPerso = majTuileSelectionneeSurPerso;