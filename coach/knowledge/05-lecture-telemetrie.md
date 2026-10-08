# Lecture de la télémétrie 3DMS

## Ce que mesure le 3DMS
| Grandeur | Remarques |
| :--- | :--- |
| Position GPS | Précision de quelques mètres : suffisante pour comparer des lignes entre tours, pas pour des écarts de trajectoire inférieurs à ~2 m. |
| Vitesse | Vitesse GPS, fiable ; c'est la donnée la plus robuste. |
| Angle | Angle **châssis** (pas l'angle pilote + moto). Gauche négatif, droite positif. |
| Accélération longitudinale | En G ; positive = freinage (courbe « Decel »), négative = accélération (courbe « Accel »). |

L'analyseur lisse et rééchantillonne les données (réglages dans PARAMÈTRES) : un lissage fort masque les attaques de frein et les hésitations courtes.

## Outils de l'analyseur et ce qu'ils révèlent
- **Delta Time** (vs référence, en distance) : où le temps se gagne ou se perd. Une pente descendante du delta localise la perte ; chercher la **cause en amont** (freinage, entrée) de l'endroit où le delta se dégrade.
- **Vmin par virage** : vitesse de passage. Vmin plus haute = meilleur portage de vitesse en entrée ; mais vérifier la sortie (une Vmin haute avec une sortie lente peut être une corde trop tôt).
- **Régularité par secteur** (écart-type) : un secteur irrégulier = repères absents ou zone de manque de confiance. Travailler la régularité avant la vitesse.
- **Carte** : comparer les lignes, situer les marqueurs Vmin, voir les zones d'accélération (dégradé).
- **Tour idéal / potentiel** (meilleurs secteurs combinés) : temps théorique accessible ; l'écart au meilleur tour indique le gain de régularité possible.

## Méthode d'analyse d'un tour
1. Comparer au tour de référence (meilleur tour ou tour d'un pilote plus rapide) **en distance**.
2. Repérer les 2 ou 3 zones où le delta se dégrade le plus.
3. Pour chaque zone, lire dans l'ordre : point et forme du freinage → Vmin et position de la Vmin → remise des gaz → vitesse en bout de ligne droite.
4. Formuler une hypothèse de cause unique par zone.

## Pièges
- Comparer des tours dans des conditions différentes (trafic, piste qui sèche, pneus usés) : vérifier les conditions et la place du tour dans la session.
- Conclure sur un seul tour : confirmer sur plusieurs tours.
- Confondre conséquence et cause : une sortie lente vient souvent d'une entrée ratée.
- Pneus usés en fin de session : Vmin et accélérations en baisse qui ne sont pas des erreurs de pilotage.
