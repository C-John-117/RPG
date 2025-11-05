// Configuration du monde
const TAILLE_CARTE = 50;
const TAILLE_VUE = 5;
const POSITION_DEPART_X = 10;
const POSITION_DEPART_Y = 10;
const API_BASE_URL = 'https://localhost:7008/api'; // URL de l'API backend

// Types de tuiles selon les spécifications API
const TUILE_TYPES = {
    HERBE: { img: 'assets/Plains.png', label: 'Herbe', traversable: true },
    EAU: { img: 'assets/River.png', label: 'Eau', traversable: false },
    MONTAGNE: { img: 'assets/Mountain.png', label: 'Montagne', traversable: false },
    FORET: { img: 'assets/Forest.png', label: 'Forêt', traversable: true },
    VILLE: { img: 'assets/Town.png', label: 'Ville', traversable: true },
    ROUTE: { img: 'assets/Road.png', label: 'Route', traversable: true }
};

// Conversion des types numériques API vers les chaînes frontend
const TYPE_CONVERSION = {
    0: 'HERBE',
    1: 'EAU',
    2: 'MONTAGNE',
    3: 'FORET',
    4: 'VILLE',
    5: 'ROUTE'
};

// Taille du "pas" quand on change de zone (une zone = 5x5)
const PAS_ZONE = TAILLE_VUE;

// Couleurs de fallback
const TUILE_COLORS = {
    'HERBE': '#7bed9f',
    'EAU': '#1e90ff',
    'MONTAGNE': '#a4b0be',
    'FORET': '#2ecc40',
    'VILLE': '#ff6348',
    'ROUTE': '#ffe066'
};

// Export des constantes
window.TAILLE_CARTE = TAILLE_CARTE;
window.TAILLE_VUE = TAILLE_VUE;
window.POSITION_DEPART_X = POSITION_DEPART_X;
window.POSITION_DEPART_Y = POSITION_DEPART_Y;
window.API_BASE_URL = API_BASE_URL;
window.TUILE_TYPES = TUILE_TYPES;
window.TYPE_CONVERSION = TYPE_CONVERSION;
window.PAS_ZONE = PAS_ZONE;
window.TUILE_COLORS = TUILE_COLORS;