// Variables globales restantes
let lastMovementDirection = 'down';

// Fonctions utilitaires restantes
function estDansCarte(x, y) {
  return x >= 0 && y >= 0 && x < TAILLE_CARTE && y < TAILLE_CARTE;
}

function estDansVision(x, y) {
  return (
    x >= vueOrigineX &&
    x <= vueOrigineX + TAILLE_VUE - 1 &&
    y >= vueOrigineY &&
    y <= vueOrigineY + TAILLE_VUE - 1
  );
}

function adjacentes8(cx, cy) {
  const out = [];
  for (let dy = -1; dy <= 1; dy++) {
    for (let dx = -1; dx <= 1; dx++) {
      if (dx === 0 && dy === 0) continue;
      out.push([cx + dx, cy + dy]);
    }
  }
  return out;
}

async function chargerTuilesBatch(coords) {
  let aCharger = coords.filter(([x, y]) =>
    estDansCarte(x, y) &&
    estDansVision(x, y) &&
    !tuiles[`${x},${y}`]
  );

  if (aCharger.length === 0) return;

  await Promise.all(aCharger.map(async ([x, y]) => {
    try {
      await loadTuile(x, y);
    } catch (e) {
      console.warn(`Échec chargement tuile (${x},${y}):`, e);
    }
  }));
}

async function explorerAdjacentesAuto(x, y) {
  let autour = adjacentes8(x, y);
  await chargerTuilesBatch(autour);
}

// Initialisation de l'application
window.onload = async function () {
  // Attendre que tous les scripts soient chargés
  const checkReady = () => {
    if (typeof initializeApp === 'function' &&
        typeof setProtectedVisible === 'function' &&
        typeof setLoginVisible === 'function' &&
        typeof wireAuthUI === 'function') {
      setProtectedVisible(false);
      setLoginVisible(true);
      initializeApp();
    } else {
      // Réessayer dans 100ms
      setTimeout(checkReady, 100);
    }
  };

  checkReady();
};
