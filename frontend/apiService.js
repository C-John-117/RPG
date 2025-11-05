// Appel API pour charger une tuile depuis le backend
async function fetchTuileFromAPI(x, y) {
    try {
        // Utiliser le nouveau endpoint avec les détails du monstre
        const url = `${API_BASE_URL}/tuiles/details/${x}/${y}`;
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`Erreur HTTP: ${response.status}`);
        }

        const tuileDto = await response.json();

        // Adapter la réponse de l'API backend au format frontend
        const tuile = {
            positionX: tuileDto.x,
            positionY: tuileDto.y,
            type: tuileDto.type, // Le DTO retourne déjà le string
            traversable: tuileDto.estTraversable,
            imageUrl: tuileDto.imageUrl
        };

        // Ajouter le monstre s'il existe
        if (tuileDto.monstre) {
            tuile.monstre = {
                x: tuileDto.monstre.x,
                y: tuileDto.monstre.y,
                monstreId: tuileDto.monstre.monstreId,
                nom: tuileDto.monstre.nom,
                spriteURL: tuileDto.monstre.spriteURL,
                type1: tuileDto.monstre.type1,
                type2: tuileDto.monstre.type2,
                niveau: tuileDto.monstre.niveau,
                pointsVieActuels: tuileDto.monstre.pointsVieActuels,
                pointsVieMax: tuileDto.monstre.pointsVieMax,
                force: tuileDto.monstre.force,
                defense: tuileDto.monstre.defense
            };
        }

        return tuile;

    } catch (error) {
        console.error('Erreur API:', error);
        throw error;
    }
}

async function enregsitrementUtilisateur(email, pseudo, password) {
  const response = await fetch(`${API_BASE_URL}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ Email: email, Pseudo: pseudo, MotDePasse: password })
  });
  if (!response.ok) throw new Error('Désolé, votre inscription a échouée.');
  return await response.json(); // ceci je le mets au cas où dans l'API dans un possible futur on est des messages d'erreur renvoyés directement par l'API
}

async function connexion(email, password) {
  const response = await fetch(`${API_BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ Email: email, MotDePasse: password })
  });
  if (!response.ok) throw new Error('Connexion échouée');
  return await response.json(); // { personnage }
}

// Fonction de déconnexion
async function déconnexion() {
  // Plus besoin de token, on fait juste un appel simple
  const response = await fetch(`${API_BASE_URL}/auth/logout`, {
    method: 'POST'
  });
  // Même si le serveur renvoie 204/200, on nettoie localement
}

// Appel API pour déplacer le personnage
async function deplacerPersonnageAPI(utilisateurId, nouvellePositionX, nouvellePositionY) {
  console.log("Tentative de déplacement vers:", { utilisateurId, nouvellePositionX, nouvellePositionY });
  console.log("URL complète:", `${API_BASE_URL}/personnage/move`);

  const response = await fetch(`${API_BASE_URL}/personnage/move`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      UtilisateurId: utilisateurId,
      NouvellePositionX: nouvellePositionX,
      NouvellePositionY: nouvellePositionY
    })
  });

  console.log("Réponse reçue - Status:", response.status, "OK:", response.ok);

  if (!response.ok) {
    const erreur = await response.text();
    console.error("Erreur de déplacement:", erreur);
    console.error("Status code:", response.status);
    throw new Error(erreur || "Déplacement impossible");
  }

  return await response.json();
}

// Export des fonctions
window.fetchTuileFromAPI = fetchTuileFromAPI;
window.enregsitrementUtilisateur = enregsitrementUtilisateur;
window.connexion = connexion;
window.déconnexion = déconnexion;
window.deplacerPersonnageAPI = deplacerPersonnageAPI;