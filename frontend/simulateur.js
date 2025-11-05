// ==================== SIMULATEUR DE COMBAT ====================
// Fichier dédié à la gestion du simulateur de combat
// Utilise la formule: Dégâts = (Force - Défense) × Facteur aléatoire [0.8 - 1.25]

let currentMonstreData = null;

// Ouvre le simulateur
function ouvrirSimulateur() {
  const modal = document.getElementById('combat-simulator');
  if (modal) {
    modal.classList.remove('hidden');
    chargerStatsJoueur();
  }
}

// Ferme le simulateur
function fermerSimulateur() {
  const modal = document.getElementById('combat-simulator');
  if (modal) modal.classList.add('hidden');
  // Reset
  document.getElementById('simulation-results').classList.add('hidden');
}

// Charge les stats du joueur dans le simulateur
function chargerStatsJoueur() {
  if (personnage) {
    document.getElementById('sim-player-nom').textContent = personnage.nom;
    document.getElementById('sim-player-pv').textContent = personnage.pointsVie;
    document.getElementById('sim-player-force').textContent = personnage.force;
    document.getElementById('sim-player-defense').textContent = personnage.defense;
  } else {
    document.getElementById('sim-player-nom').textContent = '—';
    document.getElementById('sim-player-pv').textContent = '0';
    document.getElementById('sim-player-force').textContent = '0';
    document.getElementById('sim-player-defense').textContent = '0';
  }
}

// Charge les stats d'un monstre depuis une tuile
function chargerMonstreDansSimulateur(monstre) {
  ouvrirSimulateur();
  document.getElementById('sim-monstre-nom').value = monstre.nom || '';
  document.getElementById('sim-monstre-pv').value = monstre.pointsVieMax || 0;
  // Utiliser les stats calculées (force/defense) au lieu des stats de base
  document.getElementById('sim-monstre-attaque').value = monstre.force || monstre.forceBase || 0;
  document.getElementById('sim-monstre-defense').value = monstre.defense || monstre.defenseBase || 0;
  currentMonstreData = monstre;
}

// Simule un combat avec facteur aléatoire (0.8 - 1.25) selon l'énoncé
function simulerCombat() {
  if (!personnage) {
    alert('Vous devez être connecté pour utiliser le simulateur.');
    return;
  }

  // Récupère les stats du monstre
  const monstreNom = document.getElementById('sim-monstre-nom').value.trim();
  const monstrePV = parseInt(document.getElementById('sim-monstre-pv').value) || 0;
  const monstreAttaque = parseInt(document.getElementById('sim-monstre-attaque').value) || 0;
  const monstreDefense = parseInt(document.getElementById('sim-monstre-defense').value) || 0;

  if (!monstreNom || monstrePV <= 0 || monstreAttaque < 0) {
    alert('Veuillez entrer des statistiques valides pour le monstre.');
    return;
  }

  // Stats du joueur
  const joueurPV = personnage.pointsVie;
  const joueurForce = personnage.force;
  const joueurDefense = personnage.defense;

  // Calcul des dégâts de base (avant facteur aléatoire)
  const degatsBaseJoueur = joueurForce - monstreDefense;
  const degatsBaseMonstre = monstreAttaque - joueurDefense;

  // Si les dégâts de base sont négatifs ou nuls, aucun dégât n'est infligé
  const degatsJoueurMin = degatsBaseJoueur > 0 ? Math.max(1, Math.floor(degatsBaseJoueur * 0.8)) : 0;
  const degatsJoueurMax = degatsBaseJoueur > 0 ? Math.ceil(degatsBaseJoueur * 1.25) : 0;
  const degatsJoueurMoyen = degatsBaseJoueur > 0 ? Math.round(degatsBaseJoueur * 1.025) : 0;

  const degatsMonstreMin = degatsBaseMonstre > 0 ? Math.max(1, Math.floor(degatsBaseMonstre * 0.8)) : 0;
  const degatsMonstreMax = degatsBaseMonstre > 0 ? Math.ceil(degatsBaseMonstre * 1.25) : 0;
  const degatsMonsترeMoyen = degatsBaseMonstre > 0 ? Math.round(degatsBaseMonstre * 1.025) : 0;

  // Fonction pour simuler un combat complet
  function simulerUnCombat(degatsJ, degatsM) {
    if (degatsJ <= 0 && degatsM <= 0) return { victoire: false, tours: 0, pvRestants: joueurPV, impossible: true };
    if (degatsJ <= 0) return { victoire: false, tours: Math.ceil(joueurPV / degatsM), pvRestants: 0, impossible: false };
    if (degatsM <= 0) return { victoire: true, tours: Math.ceil(monstrePV / degatsJ), pvRestants: joueurPV, impossible: false };

    let pvJ = joueurPV;
    let pvM = monstrePV;
    let tours = 0;

    while (pvJ > 0 && pvM > 0) {
      tours++;
      // Les deux attaquent simultanément
      pvM -= degatsJ;
      pvJ -= degatsM;
    }

    return {
      victoire: pvM <= 0 && pvJ > 0,
      tours: tours,
      pvRestants: Math.max(0, pvJ),
      impossible: false
    };
  }

  // Simulations des 3 scénarios
  const meilleurCas = simulerUnCombat(degatsJoueurMax, degatsMonstreMin);
  const casMoyen = simulerUnCombat(degatsJoueurMoyen, degatsMonsترeMoyen);
  const pireCas = simulerUnCombat(degatsJoueurMin, degatsMonstreMax);

  // Monte Carlo : simulation de 1000 combats avec facteurs aléatoires
  let victoires = 0;
  const nbSimulations = 1000;
  for (let i = 0; i < nbSimulations; i++) {
    const facteurJ = 0.8 + Math.random() * 0.45; // Entre 0.8 et 1.25
    const facteurM = 0.8 + Math.random() * 0.45;
    const degatsJ = degatsBaseJoueur > 0 ? Math.max(1, Math.round(degatsBaseJoueur * facteurJ)) : 0;
    const degatsM = degatsBaseMonstre > 0 ? Math.max(1, Math.round(degatsBaseMonstre * facteurM)) : 0;
    
    const resultat = simulerUnCombat(degatsJ, degatsM);
    if (resultat.victoire) victoires++;
  }

  const tauxVictoire = Math.round((victoires / nbSimulations) * 100);

  // Affichage des résultats
  const resultsDiv = document.getElementById('results-content');
  const resultSection = document.getElementById('simulation-results');
  
  let html = `
    <h4>Combat contre ${monstreNom}</h4>
    
    <div class="combat-stats">
      <div class="stat-block">
        <h5>Vous</h5>
        <p>PV: ${joueurPV}</p>
        <p>Force: ${joueurForce}</p>
        <p>Défense: ${joueurDefense}</p>
      </div>
      <div class="vs-separator">VS</div>
      <div class="stat-block">
        <h5>${monstreNom}</h5>
        <p>PV: ${monstrePV}</p>
        <p>Attaque: ${monstreAttaque}</p>
        <p>Défense: ${monstreDefense}</p>
      </div>
    </div>

    <div class="damage-info">
      <p><strong>Dégâts potentiels :</strong></p>
      <p>• Vous infligez : <span class="damage-range">${degatsJoueurMin} - ${degatsJoueurMax}</span> par coup (base: ${degatsBaseJoueur > 0 ? degatsBaseJoueur : 0})</p>
      <p>• Vous subissez : <span class="damage-range">${degatsMonstreMin} - ${degatsMonstreMax}</span> par coup (base: ${degatsBaseMonstre > 0 ? degatsBaseMonstre : 0})</p>
      <p class="info-note">Formule : (Force - Défense) × Facteur aléatoire [0.8 - 1.25]</p>
    </div>
  `;

  // Analyse globale
  if (tauxVictoire >= 80) {
    html += `
      <div class="result-success">
        <strong>VICTOIRE TRÈS PROBABLE</strong><br>
        Chance de victoire : <strong>${tauxVictoire}%</strong><br>
        Ce combat devrait bien se passer !
      </div>
    `;
  } else if (tauxVictoire >= 50) {
    html += `
      <div class="result-warning">
        <strong>COMBAT INCERTAIN</strong><br>
        Chance de victoire : <strong>${tauxVictoire}%</strong><br>
        Le combat peut tourner dans un sens ou dans l'autre.
      </div>
    `;
  } else {
    html += `
      <div class="result-danger">
        <strong>DÉFAITE PROBABLE</strong><br>
        Chance de victoire : <strong>${tauxVictoire}%</strong><br>
        Combat fortement déconseillé !
      </div>
    `;
  }

  // Scénarios détaillés
  html += `
    <div class="scenarios">
      <h5>Scénarios possibles :</h5>
      
      <div class="scenario scenario-best">
        <strong>Meilleur cas</strong> (chance maximale)<br>
        ${meilleurCas.impossible ? 'Impossible de gagner' : 
          meilleurCas.victoire ? 
            `Victoire en ${meilleurCas.tours} tour(s) - PV restants: ${meilleurCas.pvRestants}` :
            `Défaite en ${meilleurCas.tours} tour(s)`
        }
      </div>
      
      <div class="scenario scenario-average">
        <strong>Cas moyen</strong> (probabilité ~50%)<br>
        ${casMoyen.impossible ? 'Impossible de gagner' :
          casMoyen.victoire ? 
            `Victoire en ${casMoyen.tours} tour(s) - PV restants: ${casMoyen.pvRestants}` :
            `Défaite en ${casMoyen.tours} tour(s)`
        }
      </div>
      
      <div class="scenario scenario-worst">
        <strong>Pire cas</strong> (malchance maximale)<br>
        ${pireCas.impossible ? 'Impossible de gagner' :
          pireCas.victoire ? 
            `Victoire en ${pireCas.tours} tour(s) - PV restants: ${pireCas.pvRestants}` :
            `Défaite en ${pireCas.tours} tour(s)`
        }
      </div>
    </div>
  `;

  // Recommandations
  if (tauxVictoire < 70) {
    html += `
      <div class="recommendations">
        <h5>Recommandations :</h5>
    `;

    if (degatsBaseJoueur <= 0) {
      const forceMin = monstreDefense + 1;
      html += `<p>Votre force (${joueurForce}) est trop faible ! Augmentez-la à ${forceMin}+ pour infliger des dégâts.</p>`;
    } else if (degatsBaseJoueur < 5) {
      html += `<p>• Augmentez votre force pour infliger plus de dégâts (actuellement seulement ${degatsBaseJoueur} de base)</p>`;
    }

    if (degatsBaseMonstre > joueurPV / 3) {
      html += `<p>• Augmentez votre défense pour réduire les dégâts subis (${degatsBaseMonstre} de base)</p>`;
      html += `<p>• Augmentez vos PV pour survivre plus longtemps</p>`;
    }

    if (tauxVictoire < 30) {
      html += `<p class="danger-text"><strong>Ce monstre est trop puissant pour vous actuellement !</strong></p>`;
    }

    html += `</div>`;
  } else {
    html += `
      <div class="recommendations">
        <p>Vous êtes prêt pour ce combat !</p>
        <p>Bonne chance, aventurier !</p>
      </div>
    `;
  }

  resultsDiv.innerHTML = html;
  resultSection.classList.remove('hidden');
}

// Initialise les événements du simulateur
function initSimulateur() {
  const btnOpen = document.getElementById('btn-open-simulator');
  const btnClose = document.getElementById('close-simulator');
  const btnSimulate = document.getElementById('btn-simulate');

  if (btnOpen) btnOpen.onclick = ouvrirSimulateur;
  if (btnClose) btnClose.onclick = fermerSimulateur;
  if (btnSimulate) btnSimulate.onclick = simulerCombat;

  // Fermer en cliquant à l'extérieur du modal
  const modal = document.getElementById('combat-simulator');
  if (modal) {
    modal.onclick = (e) => {
      if (e.target === modal) fermerSimulateur();
    };
  }
}

// Initialiser le simulateur au chargement de la page
document.addEventListener('DOMContentLoaded', () => {
  initSimulateur();
});
