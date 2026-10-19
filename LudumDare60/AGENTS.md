# Contexte des projets Unity

Ce contenu est à mettre à jour en fonction des changements effectués et des nouveaux ajouts, mais seulement quand c’est vraiment important pour la suite, éviter de surcharger ce fichier.

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

- Ne fais pas de tests fonctionnels en play mode toi-même. Pas besoin de mentionner dans tes résumés que tu ne fais pas ces tests.

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

## Versioning

- Laisse moi faire les commits. Pas besoin de mentionner le fait que tu ne l’as pas fait toi-même.

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
- À chaque fois qu’il est blessé, qu’il se fait rattraper par la caméra, ou qu’il tombe dans un trou, il meurt et réapparait au dernier checkpoint.
  - La caméra retourne en arrière jusqu’au checkpoint avec un mouvement fluide.
  - Les checkpoints sont des prefabs `Checkpoint` placés à la main : le plus avancé dont l’avatar a dépassé le X (racine, avec le cairn) devient actif, et l’avatar réapparaît sur son enfant `SpawnPoint`. Le niveau démarre au checkpoint `startPoint` le plus avancé (à défaut le premier) avec la même séquence de spawn (caméra placée instantanément), les positions de l’avatar et de la caméra dans la scène sont donc écrasées.
  - Un checkpoint `debug` est désactivé en build : combiné à `startPoint`, il sert à tester une section.
  - Le côté droit et le haut de la caméra ne provoquent pas de mort, ils doivent juste bloquer le passage.

### Obstacles

- Buissons et rochers sont à éviter avec un saut.
- Tout contact mortel (ennemis, buissons) passe par le composant `KillsPlayerOnContact` (collision non-trigger avec le Rigidbody2D de l’avatar).
- Des animaux peuvent charger en ligne droite ou mordre quand l’avatar est trop proche.
- Des humains peuvent soit lancer des armes, soit frapper quand l’avatar est trop proche.
- Les humains et animaux disparaissent s’ils se font rattraper par la caméra.
- Si le joueur les tuent, ils disparaissent aussi.
- À chaque (ré)apparition de l'avatar, tous les ennemis reviennent à leur position et état initiaux avec un fondu sortant puis entrant, y compris ceux qui ont disparu. Leur mort (`EnemyController.Kill()`) est instantanée avec un burst de particules, et désactive le GameObject : ne jamais les détruire.

### Pick ups

- Placés à la main dans le décor.
- Armes comme des lances, à usage unique. On peut récupérer plusieurs armes du même type, mais ramasser un nouveau type d’arme discard les autres armes.
  - La lance se lance en relâchant `Player/Attack` (`SpearThrower`, réglages dans `Data/SpearThrowSettings`) : la durée d’appui donne la vitesse, en dessous du seuil le lancer est annulé. Sauf si la charge vient d’une manette, la souris (`UI/Point`) oriente l’avatar pendant la charge et donne l’angle de lancer (borné par `MinAimAngle`/`MaxAimAngle`). La course est bloquée de l’appui à la fin de l’animation de lancer ; l’état `avatar_aiming` est piloté en motion time par `AimProgress`. Dans `avatar_aiming_max`, le bras avant est un enfant séparé (`AimingArmVisual`, composant `AimingArm`) : affiché seulement dans cet état, frames keyées dans le clip, orienté selon `SpearThrower.AimAngle`. La `ThrownSpear` est physique, ne collide qu'avec le layer `Ground` et tue les ennemis via un trigger au-delà d'une vitesse minimum. Elle est détruite hors caméra (gauche, droite, bas) ou au respawn.
  - Ramasser une arme quand on a les mains vides ou quand elle est du même type se fait automatiquement. Pour swap avec une autre arme, il faut utiliser un input.
- Bûches de bois utilisées comme bouclier, à usage unique. On ne peut en ramasser qu’un à la fois.

### Level design

- Tout est placé à la main dans la même scène, il me faudra donc des outils pour facilement placer le décor, les pick ups, les obstacles, les ennemis, et définir la vitesse du side scrolling en fonction des sections.

### UI

- Le HUD est très minimaliste.
- Les objets possédés (armes, bouclier) sont indiqués dans le coin inférieur droit.
  - GameObject `HUD` de la scène (UIDocument + `SpearHud`), UXML/USS/PanelSettings dans `Data/UI/`. Les slots de lances se remplissent via la classe `spear-slot--full`.
- Des tutoriels peuvent s’afficher sur la partie supérieure de l’écran, mais ils sont des dessins sur la paroi et ne font donc techniquement pas partie du HUD.
  - Ils s’affichent tant que l’avatar reste dans une certaine zone, et le side scroll est désactivé tant qu’il y reste.

## Tech art

- La caméra est en mode perspective, sans rotation ni zoom : elle translate uniquement sur l'axe X pour le side scrolling.
- Le gameplay utilise la physique 2D (plan z=0), les visuels restent des objets 3D (quads) rendus en URP.
- Le niveau est statique dans la scène ; la paroi de la caverne est une succession de quads recyclés devant la caméra pour paraître infinie.
  - Height map nécessaire ou juste normal map?
- Les dessins sur la paroi suivent ses irrégularités (normal map, metallic, smoothness de la paroi, seule la couleur vient du dessin).
  - Ce sont des `SpriteRenderer` avec le material `Paintings` (shader `LD60/Painting`), qui n'a qu'une passe `LightMode=PaintLayer` : invisibles pour la caméra principale, ils sont rendus par la caméra `PaintLayerCamera` (renderer `PaintLayer_Renderer`, index 1 des RP assets) dans la RenderTexture `Art/Textures/PaintLayer`. Animations, `flipX` et `color` fonctionnent normalement.
  - `PaintLayerCapture` (sur `PaintLayerCamera`) cale la caméra ortho sur le frustum de la caméra principale et publie `_PaintLayer`/`_PaintLayerRect`. Le material `Wall` (Shader Graph `LD60/PaintedWall`, mêmes maps que URP Lit) projette cette texture selon l'axe Z et remplace uniquement l'albédo : toute géométrie ajoutée au mur avec ce material reçoit les dessins (jusqu'à `maxWallDepth` derrière le plan z=0 ; étirement sur les faces parallèles à Z).
  - Le composant `PaintingFlash` transmet `Flash` (0–1) en `_Flash`, qui fait tendre la couleur du dessin vers le blanc (utilisé à l'activation des checkpoints), via une instance du material créée au premier flash.
  - Ne jamais mettre de `MaterialPropertyBlock` sur ces sprites : le SpriteRenderer ne transmet alors plus son flip ni sa couleur au shader et le dessin disparaît.
  - En Scene view, `PaintingPreviewFeature` (sur `PC_Renderer` et `Mobile_Renderer`) dessine les sprites à plat pour le level design.
  - Compatible WebGL (pas de decals, qui exigent le DBuffer non supporté en GLES3).
- Toutes les animations se font en frame by frame.
- La paroi est uniquement éclairée par des torches avec une lumière vacillante.
  - Les torches sont des point lights enfants de la caméra, avec le composant `TorchFlicker` (réglages partagés dans `Data/TorchFlickerSettings`). L'intensité, la range et la position de base restent sur chaque light.
