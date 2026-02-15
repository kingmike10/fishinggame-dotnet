# 🎣 FishingGame

Projet académique en C# (.NET 8) – Jeu de pêche (variante sans paires).

## Objectifs
- Appliquer les concepts avancés vus en cours : struct, enum, classes, héritage, patterns (Strategy, Observer).
- Développer une application modulaire avec une CLI.
- Respecter la démarche Git/GitLab en phases incrémentales.

## Structure de la solution
FishingGame.sln
├── FishingGame/ → Domaine du jeu (cartes, joueurs, plateau, moteur)
├── FishingGame.Cli/ → Interface console (entrée utilisateur, affichage)
├── .gitignore
└── README.md

## Routine Git/GitLab
1. Créer une issue “Phase X – …” sur GitLab.  
2. Créer une branche : git checkout -b <branche>.  
3. Commits atomiques : git commit -m "feat: …".  
4. git push -u origin <branche>.  
5. MR en *draft* → revue → merge.  
6. git pull main local, tag si milestone atteinte.

---
© 2025 – Projet universitaire UQTR INF1035
