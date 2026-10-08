# Trajectoires

## Principes
- **Le chrono se fait en sortie de virage.** La vitesse en début de ligne droite se conserve sur toute la ligne droite : un virage qui commande une longue ligne droite vaut plus qu'un virage lent entre deux virages.
- **Ligne la plus large possible** : utiliser toute la largeur de piste en entrée, à la corde et en sortie pour ouvrir le rayon et donc la vitesse possible.
- **Trois points** : point de braquage (début de la mise sur l'angle), point de corde (point le plus proche de l'intérieur), point de sortie (moto revenue au bord extérieur).

## Corde géométrique vs corde retardée
- **Géométrique** (au milieu du virage) : vitesse de passage maximale, mais sortie tardive à l'accélération. Utile dans les courbes rapides où l'on reste en appui.
- **Retardée** (après le milieu) : on entre plus lentement, on tourne tôt la moto, on redresse et on accélère plus tôt. **Règle par défaut** pour un virage qui commande une ligne droite.
- **Corde trop tôt** = erreur classique : la moto élargit en sortie, le pilote doit garder de l'angle et retarder la remise des gaz.

## Ligne en V vs ligne en U
- **V** : freinage tard et appuyé, rotation courte au point de corde, redresser vite et accélérer fort. Efficace avec une moto puissante et dans les virages lents (épingles).
- **U** : vitesse de passage élevée, rayon constant. Efficace avec une moto légère / peu puissante et dans les courbes rapides.
- Une moto sportive de grosse cylindrée gagne en général à « casser » le virage en V pour maximiser la phase moto droite à pleine accélération.

## Enchaînements et chicanes
- **Sacrifier le premier virage pour le second** si le second commande la ligne droite.
- Dans une chicane, chercher la ligne la plus droite possible ; la transition gauche-droite est une seule action fluide (pas deux virages séparés).
- Les changements d'angle rapides demandent un contre-braquage franc et un corps déjà en mouvement vers le côté suivant.

## Repères
- Des repères fixes (plaques, vibreurs, marques au sol) pour chaque point : freinage, braquage, corde, sortie. Sans repère, pas de régularité.
- Les repères se déplacent par petites touches (quelques mètres) et un seul à la fois.

## Signatures dans les données
- **Corde trop tôt** : angle maintenu longtemps après la Vmin, accélération qui démarre tard ou plafonne, trajectoire GPS qui finit au bord extérieur bien avant la sortie.
- **Ligne trop serrée** : Vmin plus basse que la référence avec une distance parcourue plus courte.
- **Ligne trop large en entrée** : distance plus longue au même temps, Vmin correcte mais delta qui se dégrade avant la corde.
