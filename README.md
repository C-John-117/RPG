# My Little RPG - Équipe 10

Projet de jeu RPG développé en ASP.NET Core et JavaScript vanilla dans le cadre du cours de programmation web.

## Description

My Little RPG est un jeu de rôle en 2D avec système de combat automatisé contre des monstres générés aléatoirement sur une carte de 50x50 tuiles. Le joueur explore le monde, combat des monstres et monte de niveau pour devenir plus puissant.

## Prérequis

- .NET 8.0 SDK ou supérieur
- MySQL 8.0 ou supérieur
- Navigateur web moderne (Chrome, Firefox, Edge)

## Installation

### 1. Configuration de la base de données

Créez une base de données MySQL nommée `a25_mylittlerpg_equipe10` et configurez les identifiants dans le fichier `appsettings.json` :

```json
{
  "ConnectionStrings": {
    "Default": "server=localhost;database=a25_mylittlerpg_equipe10;user=root;password=VotreMotDePasse"
  }
}
```

### 2. Migration de la base de données

Depuis le dossier `backend/Système_De_Tuiles/MyLittleRpg`, exécutez :

```bash
dotnet ef database update
```

## Lancement du jeu

### Démarrer le serveur API

1. Ouvrez un terminal dans `backend/Système_De_Tuiles/MyLittleRpg`
2. Exécutez la commande :

```bash
dotnet run
```

3. Le serveur démarre sur `http://localhost:5296`
4. Au démarrage, 300 monstres sont générés automatiquement

### Démarrer l'application web

1. Ouvrez le fichier `frontend/index.html` dans votre navigateur
2. Créez un compte ou connectez-vous
3. Utilisez les flèches du clavier pour vous déplacer

## Fonctionnalités

### Système de combat

- Combat automatique lors du déplacement sur une tuile contenant un monstre
- Formule de dégâts : `(Force - Défense) × Facteur aléatoire [0.8-1.25]`
- Trois issues possibles :
  - Victoire : le monstre est vaincu, gain d'expérience
  - Défaite : téléportation à la ville domicile
  - Combat indécis : le joueur reste sur place, le monstre garde ses HP réduits

### Progression

- Gain d'expérience : `BaseXP + Niveau × 10`
- Montée de niveau : toutes les statistiques augmentent de +1
- Restauration des HP à chaque niveau gagné

### Monstres

- 300 monstres générés aléatoirement sur la carte
- Niveau basé sur la distance de la ville la plus proche
- Statistiques calculées : `StatBase + Niveau`
- Régénération automatique après 10 monstres vaincus

### Interface

- Carte de jeu 5x5 avec défilement
- Sélection de tuile pour voir les informations
- Simulateur de combat hors-ligne
- Mode clair/sombre

## Commandes

- **Flèches directionnelles** : Déplacer le personnage
- **Clic sur une tuile** : Afficher les informations (type, monstre)
- **Boutons Nord/Sud/Est/Ouest** : Déplacer la vue de 5 cases

## Architecture technique

### Backend (API)

- ASP.NET Core 8.0
- Entity Framework Core avec MySQL
- Architecture en couches (Controllers, Services, Models, DTOs)
- CORS configuré pour le développement

### Frontend

- HTML5, CSS3, JavaScript vanilla
- Communication avec l'API REST
- Animations de sprites pour le personnage
- Simulateur de combat côté client

## Structure du projet

```
myLittleRPG_equipe10/
├── backend/
│   └── Système_De_Tuiles/
│       └── MyLittleRpg/
│           ├── Controllers/
│           ├── Services/
│           ├── Models/
│           ├── DTO/
│           └── Migrations/
└── frontend/
    ├── index.html
    ├── app.js
    ├── simulateur.js
    ├── style.css
    └── assets/
```

## Équipe

Projet développé par l'Équipe 10 dans le cadre du cours de développement web.

## Notes importantes

- Le serveur réinitialise tous les personnages et monstres à chaque démarrage
- Les statistiques de départ du joueur sont : Force=20, Défense=12, PV=100
- Les monstres ne peuvent pas apparaître sur les villes ou les tuiles non-traversables
