# Tic Tac Toe en Realite Augmentee (AR)
## Auteur
- Nom: bady p
- Cours: Exercice sommatif - Tic Tac Toe en Realite Augmentee

## Description breve
Application mobile AR de Tic Tac Toe (X/O) avec:
- detection de plans AR;
- placement de la grille par tap sur une surface;
- ancrage de la grille pour la stabiliser dans l'espace;
- logique complete de partie (tour, victoire, match nul);
- interface UI pour l'etat du jeu, nouvelle partie et repositionnement.

## Version Unity et packages
- Unity Editor: `6000.3.5f1`
- Scene principale: `Assets/Scenes/Scene_TicTactToe.unity`

Packages principaux utilises:
- `com.unity.xr.arfoundation` `6.3.3`
- `com.unity.xr.arcore` `6.3.3`
- `com.unity.xr.arkit` `6.3.3`
- `com.unity.inputsystem` `1.17.0`
- `com.unity.render-pipelines.universal` `17.3.0`
- `com.unity.ugui` `2.0.0`

## Fonctionnalites implementees
- Detection des plans et visualisation des surfaces detectees.
- Placement de la grille par tap sur un plan detecte.
- Grille ancree (AR Anchor) pour rester fixe.
- Alternance joueur X / joueur O.
- Refus de placement sur case deja occupee.
- Detection de victoire (8 combinaisons) et match nul.
- Highlight de la ligne gagnante.
- Bouton `Nouvelle partie` (reset de partie).
- Bouton `Repositionner` (reset placement + nouvelle detection).
- Instructions utilisateur et etat de tour en UI.

## Structure du projet
Principaux scripts:
- `Assets/Scripts/ARPlacementManager.cs`
- `Assets/Scripts/ARInteractionManager.cs`
- `Assets/Scripts/GameController.cs`
- `Assets/Scripts/CellController.cs`
- `Assets/Scripts/GridBuilder.cs`
- `Assets/Scripts/UIRuntimePolish.cs`

Objets/ressources importants:
- `Assets/Scenes/Scene_TicTactToe.unity`
- `Assets/Prefabs/ARPlanePrefab.prefab`
- `Assets/Prefabs/RedSphere.prefab`
- `Assets/Prefabs/BlueSphere.prefab`

## Defis rencontres et solutions
1. Grille placee mais references de cellules non synchronisees
- Probleme: le controleur de jeu gardait des references sur une ancienne grille.
- couleur de la tuil demeure rose
- comprehension AR
- apparition de plusieurs surface 
- detection de plisuers surface

2. Mauvaise configuration du prefab de plan AR
- Probleme: la grille de jeu etait referencee comme `Plane Prefab`.
- Solution: remplacement par `ARPlanePrefab` pour un feedback de plan correct.

4. Mode de detection de plans incomplet
- Probleme: le mode etait force horizontal.
5. Incoherence des assets Input
- Probleme: configurations d'input mixtes.
- Solution: alignement du  sur l'asset utilise par la scene.

## Instructions de test rapide
1. Lancer la scene `Scene_TicTactToe`.
2. Scanner une surface jusqu'a voir les plans.
3. Taper une surface pour faire apparaitre la grille.
4. Jouer une partie complete (victoire et match nul).
5. Tester `Nouvelle partie`.
6. Tester `Repositionner`.


## Annexe - Requetes utilisees pour ecrire du code

- Comment fonctionne la détection de plans dans AR Foundation et quels types de surfaces sont pris en - - charge dans le projet ?

- Quelle est la différence entre un objet simplement positionné dans la scène et un objet ancré avec un AR Anchor ?

- Comment le système de tap utilisateur est-il relié au placement de la grille sur une surface détectée ?

- Comment la stabilité spatiale de la grille est-elle assurée lorsque l’utilisateur se déplace autour de la scène ?

- Quelle architecture logicielle a été retenue pour séparer la logique AR, la logique du jeu et l’interface utilisateur ?

- Comment les cellules de la grille sont-elles générées dynamiquement et référencées dans le contrôleur de jeu ?

- Comment le projet empêche-t-il le placement d’un symbole sur une case déjà occupée ?



- Comment la ligne gagnante est-elle mise en évidence visuellement dans l’espace AR ?

- Comment la gestion des tours (joueur X / joueur O) est-elle synchronisée avec l’interface utilisateur ?

- Quels problèmes peuvent apparaître lorsqu’on détecte plusieurs surfaces AR en même temps et comment les gérer ?