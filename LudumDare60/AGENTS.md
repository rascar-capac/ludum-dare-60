# Contexte des projets Unity

Ce contenu est à mettre à jour en fonction des changements effectués et des nouveaux ajouts.

## Environnement technique

- Utiliser Unity `6000.3.9f1`.
- Utiliser Universal Render Pipeline (URP).
- Utiliser UI Toolkit pour les interfaces utilisateur.

## Architecture et contenu

- Privilégier les prefabs et la configuration dans les scènes plutôt que la génération d’objets au runtime.
- Privilégier les scriptable objects pour stocker les paramètres de game design, plutôt qu’en champs des composants. Les valeurs propres à une instance placée dans le niveau (ex. vitesse d’une `ScrollSpeedZone`) restent dans la scène.
- Répartir les assets du projet dans les dossiers suivants, ne les créer qu’en cas de besoin:
  - `Art/`
    - `Sprites/`
    - `Animations/`
    - `Textures/`
    - `Models/`
    - `Materials/`
    - `Shaders/`
    - `Art_Prefabs/` (contiennent uniquement les visuels, à placer en enfant des prefabs logiques)
  - `Scripts/`
  - `Audio/`
  - `Scenes/`
  - `Prefabs/`
  - `Data/` (tous les settings du jeu, les scriptable objects, les inputs, etc.)
- Respecter l’organisation et les conventions déjà présentes dans chaque projet lorsque leur modification n’est pas nécessaire.

## Éditeur

- Référencer des InputActionReference dans l’inspecteur pour binder les inputs.

## Fichiers sérialisés Unity

- Ne pas lire en entier les fichiers `.unity`, `.prefab` et `.asset` : ce sont de volumineux fichiers YAML coûteux en tokens.
- Pour inspecter ou modifier une scène, un prefab ou un asset, passer par l’éditeur Unity via le Unity CLI plutôt que d’éditer le YAML à la main.
- Si une lecture directe est inévitable, se limiter à une recherche ciblée (`grep`) sur la partie utile.

## Tests

- Ne fais pas de tests fonctionnels en play mode toi-même.

## Délivrables

Lorsqu’un délivrable est demandé :

1. Réaliser les tests unitaires pertinents.
2. Réaliser les tests fonctionnels pertinents.
3. Corriger les erreurs détectées et relancer les vérifications concernées.
4. Produire un build Windows.
5. Produire un build OpenGL.
6. Envoyer les builds validés sur la page itch.io du projet :
   `https://rascar-capac.itch.io/ludum-dare-60`

Avant une publication, vérifier que les tests et les builds ont réussi. Ne jamais exposer de secrets ou de jetons d’authentification dans le dépôt, les logs ou les réponses.

## Game Design

### Pitch

Side scroller de peintures rupestres. Le joueur peut se déplacer librement mais il est sans arrêt poussé vers la droite par la caméra. Il va devoir éviter des obstacles, trouver les meilleurs chemins, et éviter des ennemis ou les combattre avec les armes trouvées en chemin. Tout est défini manuellement, pas de génération aléatoire.

### Visuels et caméra

- Le fond est une paroi de caverne infinie.
- L’avatar, les obstacles et les éléments du décor sont des dessins sur la paroi.

#### Side scrolling

- Plutôt lent afin de laisser le joueur assez de liberté de mouvement.
- La vitesse peut être variable en fonction des sections.
- La vitesse est définie par des zones placées dans la scène (plage X → vitesse), avec une vitesse par défaut hors zone.
- Toujours de gauche à droite.

### Avatar

- Humain qui peut courir, sauter et utiliser des objets qu’il ramasse.
- Le joueur peut le déplacer à la manette ou au clavier.
- À chaque fois qu’il est blessé ou qu’il se fait rattraper par la caméra, il meurt et réapparait au dernier checkpoint.
  - La caméra side scroll en arrière pour revenir jusqu’au checkpoint.

### Obstacles

- Buissons et rochers sont à éviter avec un saut.
- Des animaux peuvent charger en ligne droite ou mordre quand l’avatar est trop proche.
- Des humains peuvent soit lancer des armes, soit frapper quand l’avatar est trop proche.
- Les humains et animaux disparaissent s’ils se font rattraper par la caméra.
- Si le joueur les tuent, ils disparaissent aussi.

### Pick ups

- Placés à la main dans le décor.
- Armes comme des lances, à usage unique. On peut récupérer plusieurs armes du même type, mais ramasser un nouveau type d’arme discard les autres armes.
  - Ramasser une arme quand on a les mains vides ou quand elle est du même type se fait automatiquement. Pour swap avec une autre arme, il faut utiliser un input.
- Bûches de bois utilisées comme bouclier, à usage unique. On ne peut en ramasser qu’un à la fois.

### Level design

- Tout est placé à la main dans la même scène, il me faudra donc des outils pour facilement placer le décor, les pick ups, les obstacles, les ennemis, et définir la vitesse du side scrolling en fonction des sections.

### UI

- Le HUD est très minimaliste.
- Les objets possédés (armes, bouclier) sont indiqués dans le coin inférieur droit.
- Des tutoriels peuvent s’afficher sur la partie supérieure de l’écran, mais ils sont des dessins sur la paroi et ne font donc techniquement pas partie du HUD.
  - Ils s’affichent tant que l’avatar reste dans une certaine zone, et le side scroll est désactivé tant qu’il y reste.

## Tech art

- La caméra est en mode perspective, sans rotation ni zoom : elle translate uniquement sur l'axe X pour le side scrolling.
- Le gameplay utilise la physique 2D (plan z=0), les visuels restent des objets 3D (quads) rendus en URP.
- Le niveau est statique dans la scène ; la paroi de la caverne est une succession de quads recyclés devant la caméra pour paraître infinie.
  - Height map nécessaire ou juste normal map?
- Les dessins sur la paroi sont aussi des quads qui doivent suivre les irrégularités de la paroi (les textures utilisées par la paroi doivent être appliquées sur les dessins aussi, sauf la couleur).
- Toutes les animations se font en frame by frame.
- La paroi est uniquement éclairée par des torches avec une lumière vacillante.
