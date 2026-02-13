# Tic Tac Toe en Réalité Augmentée (AR)

## Auteur  
**Nom :** Bady P.  
**Contexte :** Exercice sommatif – Développement d’un jeu Tic Tac Toe en Réalité Augmentée

---

## Présentation du projet

Ce projet est une application mobile de Tic Tac Toe (X / O) en Réalité Augmentée, développée avec Unity et AR Foundation  
Il permet à l’utilisateur de placer une grille de jeu dans son environnement réel et de jouer une partie complète de manière stable et intuitive.

Le projet met l’accent sur :
- la compréhension des concepts fondamentaux de la réalité augmentée ;
- une architecture logicielle claire et modulaire ;
- la stabilité spatiale et l’expérience utilisateur.

---

## Description générale

L’application propose :
- la détection automatique des surfaces réelles (plans AR) ;
- le placement de la grille par simple tap sur une surface détectée ;
- l’ancrage de la grille afin qu’elle reste fixe dans l’espace ;
- la gestion complète des règles du Tic Tac Toe ;
- une interface utilisateur claire et fonctionnelle.

---

## Environnement technique

### Unity
- **Version : Unity Editor `6000.3.5f1`
- **Scène principale :**  
  `Assets/Scenes/Scene_TicTactToe.unity`

### Packages principaux
- `com.unity.xr.arfoundation` `6.3.3`
- `com.unity.xr.arcore` `6.3.3`
- `com.unity.xr.arkit` `6.3.3`
- `com.unity.inputsystem` `1.17.0`
- `com.unity.render-pipelines.universal` `17.3.0`
- `com.unity.ugui` `2.0.0`

---

## Fonctionnalités implémentées

- Détection et visualisation des plans AR.
- Placement de la grille par tap utilisateur.
- Ancrage AR pour une stabilité spatiale fiable.
- Alternance automatique joueur X / joueur O.
- Blocage des cases déjà occupées.
- Détection des 8 combinaisons gagnantes.
- Gestion du match nul.
- Mise en évidence visuelle de la ligne gagnante.
- Bouton **Nouvelle partie**.
- Bouton **Repositionner la grille**.
- Affichage des instructions et de l’état du jeu.

---

## Structure du projet

### Scripts principaux
- `ARPlacementManager.cs`
- `ARInteractionManager.cs`
- `GameController.cs`
- `CellController.cs`
- `GridBuilder.cs`
- `UIRuntimePolish.cs`

### Ressources importantes
- `Assets/Scenes/Scene_TicTactToe.unity`
- `Assets/Prefabs/ARPlanePrefab.prefab`
- `Assets/Prefabs/RedSphere.prefab`
- `Assets/Prefabs/BlueSphere.prefab`

---

## Défis rencontrés et solutions

### 1. Désynchronisation des cellules
**Problème :** le contrôleur de jeu conservait des références vers une ancienne grille après repositionnement.  
**Solution :** réinitialisation complète des références lors de chaque création de grille.

### 2. Mauvais prefab de plan AR
**Problème :** la grille était utilisée comme `Plane Prefab`.  
**Solution :** utilisation d’un prefab dédié (`ARPlanePrefab`).

### 3. Multiples surfaces AR détectées
**Problème :** surcharge visuelle et ambiguïté de placement.  
**Solution :** meilleure gestion de l’affichage et du reset des surfaces.

### 4. Mode de détection incomplet
**Problème :** détection limitée aux plans horizontaux.  
**Solution :** configuration appropriée des modes de détection.

### 5. Système d’Input incohérent
**Problème :** mélange entre anciens et nouveaux systèmes d’Input.  
**Solution :** alignement sur un seul asset d’Input.

---

## Instructions de test rapide

1. Lancer la scène `Scene_TicTactToe`.
2. Scanner l’environnement jusqu’à la détection des surfaces.
3. Taper sur une surface pour placer la grille.
4. Jouer une partie complète.
5. Tester **Nouvelle partie**.
6. Tester **Repositionner**.

---

## Médiagraphie
Unity Documentation – AR Plane Manager (AR Foundation)
https://docs.unity.cn/Packages/com.unity.xr.arfoundation%406.0/manual/features/plane-detection/arplanemanager.html

Utilisée pour comprendre le fonctionnement de la détection des surfaces (plans horizontaux et verticaux).

Unity Documentation – AR Anchor Manager
https://docs.unity.cn/Packages/com.unity.xr.arfoundation%405.0/manual/features/anchors.html

Référence principale pour l’ancrage des objets virtuels dans l’espace réel.

Unity Documentation – Anchors Platform Support
https://docs.unity.cn/Packages/com.unity.xr.arfoundation%406.1/manual/features/anchors/platform-support.html

Consultée afin de vérifier la compatibilité des anchors entre ARCore et ARKit.

Unity Learn – Configuring Plane Detection for AR Foundation
https://learn.unity.com/course/ar-curricular-framework-resources/tutorial/configuring-plane-detection-for-ar-foundation

Ressource pédagogique utilisée pour configurer correctement les modes de détection AR.

Google ARCore Documentation – Working with Anchors
https://developers.google.com/ar/develop/anchors

Utilisée pour mieux comprendre le comportement des anchors côté Android.

Unity Technologies – AR Foundation Demo Projects (GitHub)
https://github.com/Unity-Technologies/arfoundation-demos
- Cours : https://envimmersif-cegepvicto.github.io/

Exemples de projets servant de référence pour les bonnes pratiques AR.
### Documentation officielle
- Unity – AR Plane Manager (AR Foundation)
- Unity – AR Anchor Manager
- Unity Learn – Configuring Plane Detection
- Google ARCore – Anchors
- Unity Technologies – AR Foundation Demo Projects


### Projets de référence
- Tic Tac Toe Unity 
- Tic Tac Toe multijoueur 
- Tic Tac Toe XR / AR
- Projets et tutoriels AR Tic Tac Toe 
---

## Annexe – Utilisation de l’IA

L’IA a été utilisée comme outil d’assistance pour :
- comprendre les mécanismes de la détection de plans AR ;
- clarifier l’usage des AR Anchors ;
- structurer l’architecture logicielle ;
- améliorer la logique de jeu et l’interface utilisateur.
Questions initiales

Comment fonctionne la détection de plans dans AR Foundation et quels types de surfaces sont pris en charge dans le projet ?

Quelle est la différence entre un objet simplement positionné dans la scène et un objet ancré à l’aide d’un AR Anchor ?

Comment le système de tap utilisateur est-il relié au placement de la grille sur une surface détectée ?

Comment la stabilité spatiale de la grille est-elle assurée lorsque l’utilisateur se déplace autour de la scène ?

Quelle architecture logicielle permet de séparer efficacement la logique AR, la logique du jeu et l’interface utilisateur ?

Comment les cellules de la grille sont-elles générées dynamiquement et référencées dans le contrôleur de jeu ?

Quels mécanismes empêchent le placement d’un symbole sur une case déjà occupée ?

Comment la ligne gagnante est-elle mise en évidence visuellement dans l’espace de réalité augmentée ?

Comment la gestion des tours (joueur X / joueur O) est-elle synchronisée avec l’interface utilisateur ?

Quels problèmes peuvent apparaître lors de la détection simultanée de plusieurs surfaces AR et comment les gérer ?
Toutes les décisions finales ont été analysées, adaptées et validées dans le cadre académique du projet.
