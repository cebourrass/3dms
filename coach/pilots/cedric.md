# Fiche pilote : Cedric

> Source : export de l'analyse « Base de connaissances — Cedric / Yamaha YZF-R1 2023 » du 08/10/2026, établie sur peu de données. À enrichir à partir des sessions disponibles (voir [TODO.md](../../TODO.md), « Mise à jour du profil pilote »).

## Identité et communication
- Nom affiché dans l'analyseur : Cedric
- Niveau : pilote track day expérimenté (France).
- Objectif général : gagner du temps au tour par l'optimisation technique, **réglage mécanique et technique de pilotage**.
- Un contact semi-pro donne son avis ; **les décisions finales reviennent à Cedric**, selon son propre rapport risque / bénéfice. Le coach propose et argumente, il ne tranche pas.

## Moto(s)
| Moto | Pneus | Réglages notables | Remarques |
| :--- | :--- | :--- | :--- |
| Yamaha YZF-R1 2023 | | Faisceau modifié pour reprogrammation ECU. Fichier ECU const : rapport primaire, rapports de boîte, rapport secondaire, rayon de roue. | Données disponibles : GPS, fichiers const ECU, canaux de vitesse par section de circuit. |

### Transmission (baseline)
| Circuit | Pignon x couronne |
| :--- | :--- |
| Alès | 15 x 43 |
| Issoire | 15 x 41 |

## Circuits
| Circuit | Sens / longueur | Meilleur temps | Repères / particularités |
| :--- | :--- | :--- | :--- |
| Alès | Horaire, 2 504 m | 1'22.56 | Virage 5 : travail placement du corps / coude au sol. |
| Lédenon | | 1'36.10 | |
| Alcarras | | 1'45.49 | |
| Issoire (CEERTA) | 2 475 m | 1'06.65 | Couronne 41 dents. P8 : plafond de 2e atteint (~172 km/h). P13 : épingle lente avant la ligne droite. |

Sessions de test versionnées dans le repo : Alcarras (2025-10), Alès (2025-10, 2025-11, 2026-05), Lédenon (2026-04), Issoire (2026-07).

## Objectifs
- Saison : gagner du temps au tour en combinant réglages mécaniques et technique.
- Prochaine session : selon les chantiers ouverts ci-dessous.

## Points forts
- Démarche rigoureuse : hypothèse → test piste → validation données, une variable à la fois.
- Déport du corps efficace : vitesse de passage tenue avec un angle châssis modéré (voir apprentissage 1).

## Axes de travail (par priorité suggérée)
1. **Issoire P8** : la vitesse dépasse maintenant le plafond de 2e (~172 km/h) → évaluer le passage en 3e.
2. **Issoire P13** (épingle lente avant la ligne droite) : priorité à la vitesse de sortie.
3. **Technique** : blocage de la jambe extérieure, utilisation du frein arrière, glisse de la moto en sortie de virage, gestion du wheeling.

## Apprentissages validés
1. **L'angle châssis n'est pas un indicateur de performance** : le déport du corps garde le châssis plus droit à vitesse égale. La comparaison pertinente est la **vitesse minimale en courbe**. Repères : ~37–38° d'angle châssis sur la R1 (contre ~45° sur l'ancienne 600), jusqu'à 50° dans les chicanes.
2. **Couronne 41 dents** : axe arrière reculé (+8 mm d'empattement) → changements de direction un peu plus lents en chicane. Gérable au niveau actuel, à surveiller.
3. **Gains de rapport** visibles là où l'ancien réglage tapait le limiteur, confirmés par les vitesses GPS sur deux sections d'Issoire.
4. **Pneus usés** : ils masquent les gains de technique / réglage aux points de corde. Contrôler l'état des pneus avant de conclure.
5. **Technique** : placement du corps et coude au sol (virage 5 d'Alès).

## Historique des réglages testés
| Date | Circuit | Changement | Effet mesuré |
| :--- | :--- | :--- | :--- |
| 2026 | Issoire | Couronne 43 → 41 dents | 1'07.81 → 1'06.65 (**-1,16 s confirmé**) |

## Notes pour le coach
- Méthode de travail : hypothèse → test piste → validation données ; une variable à la fois ; comparaison avant / après (temps + canaux).
- Le profil repose sur peu de données : signaler explicitement quand une conclusion s'appuie sur une seule session.
- Toujours vérifier l'état des pneus et les conditions avant d'attribuer un écart au pilotage ou au réglage.

<!-- AUTO:MESURES:DEBUT - section générée par Tools/ProfileStats, ne pas modifier à la main -->
## Mesures (automatique)

_Mis à jour le 10/10/2026 à partir de 40 session(s). Temps et tours calculés par l'analyseur (détection de la ligne Start du .map) ; Vmin sur les données brutes du meilleur tour, seuils de détection par défaut._

_n/d : sessions anciennes où le boîtier n'enregistrait pas l'angle (accélération saturée à ±2 G) ; vitesses et temps restent fiables._

_Tours écartés : distance anormale (> 10 % d'écart avec la médiane de la session), exclus du meilleur tour, du tour idéal et de la régularité._

### Alès (Sens horaire) - 4 journée(s), 14 session(s) - record 1'22.59 (2026-10-04 14h13, tour 4)

| Session | Tours | Meilleur | Idéal | Régularité | Vmax | Angle max G / D | Frein max |
| :--- | ---: | ---: | ---: | :--- | ---: | ---: | ---: |
| 2025-10-19 09h16 | 7 | 1'29.02 | 1'28.67 | ± 1,59 s (3 tours) | 207 km/h | n/d | n/d |
| 2025-10-19 10h05 | 2 | 1'50.97 | 1'30.06 | - | 197 km/h | n/d | n/d |
| 2025-10-19 11h12 | 7 | 1'24.92 | 1'24.68 | ± 1,90 s (6 tours) | 208 km/h | n/d | n/d |
| 2025-11-09 11h30 | 6 | 1'29.48 | 1'26.88 | - | 206 km/h | n/d | n/d |
| 2025-11-09 14h30 | 7 | 1'30.34 | 1'28.98 | ± 1,28 s (2 tours) | 193 km/h | n/d | n/d |
| 2025-11-09 15h28 | 6 | 1'25.29 | 1'25.28 | ± 1,05 s (2 tours) | 208 km/h | n/d | n/d |
| 2025-11-09 16h35 | 7 | 1'25.99 | 1'25.47 | ± 1,21 s (4 tours) | 206 km/h | n/d | n/d |
| 2026-05-02 09h14 | 6 | 1'27.99 | 1'27.97 | ± 0,66 s (5 tours) | 184 km/h | 43° / 38° | 1,06 G |
| 2026-05-02 10h03 | 2 | 1'33.48 | 1'27.97 | ± 3,93 s (2 tours) | 182 km/h | 43° / 42° | 0,88 G |
| 2026-10-04 10h15 | 9 | 1'28.17 | 1'27.09 | ± 1,71 s (7 tours) | 205 km/h | 28° / 36° | 0,99 G |
| 2026-10-04 11h14 | 10 | 1'23.87 | 1'22.77 | ± 1,62 s (8 tours) | 213 km/h | 27° / 35° | 1,02 G |
| 2026-10-04 14h13 | 8 | 1'22.59 | 1'22.19 | ± 1,10 s (5 tours) | 216 km/h | 26° / 33° | 1,07 G |
| 2026-10-04 15h10 | 7 | 1'23.31 | 1'22.88 | ± 0,31 s (5 tours) | 213 km/h | 25° / 35° | 1,09 G |
| 2026-10-04 16h27 | 9 | 1'23.24 | 1'23.18 | ± 1,20 s (6 tours) | 210 km/h | 25° / 35° | 1,02 G |

Vmin par virage sur le record (km/h) : V1 96 · V2 71 · V3 76 · V4 92 · V5 62 · V6 79 · V7 49 · V8 109 · V9 78

### Issoire - 2 journée(s), 9 session(s) - record 1'06.69 (2026-09-04 16h24, tour 9)

| Session | Tours | Meilleur | Idéal | Régularité | Vmax | Angle max G / D | Frein max |
| :--- | ---: | ---: | ---: | :--- | ---: | ---: | ---: |
| 2026-07-24 11h22 | 14 | 1'07.81 | 1'07.81 | ± 0,51 s (4 tours) | 260 km/h | 30° / 37° | 1,31 G |
| 2026-07-24 15h03 | 16 | 1'17.11 | 1'14.88 | ± 1,32 s (11 tours) | 256 km/h | 27° / 35° | 1,07 G |
| 2026-07-24 17h12 | 17 | 1'09.00 | 1'09.00 | ± 0,79 s (2 tours) | 256 km/h | 28° / 35° | 1,12 G |
| 2026-07-24 17h31 | 2 | 1'25.02 | 1'25.02 | ± 0,95 s (2 tours) | 245 km/h | 26° / 38° | 1,01 G |
| 2026-09-04 11h39 | 3 | 1'06.79 | 1'06.79 | ± 1,33 s (2 tours) | 260 km/h | 30° / 38° | 1,26 G |
| 2026-09-04 11h47 | 4 | 1'07.82 | 1'07.82 | ± 0,95 s (2 tours) | 262 km/h | 25° / 36° | 1,20 G |
| 2026-09-04 14h45 | 10 | 1'08.39 | 1'07.31 | ± 1,55 s (9 tours) | 258 km/h | 26° / 37° | 1,17 G |
| 2026-09-04 16h24 | 10 | 1'06.69 | 1'06.68 | ± 1,26 s (9 tours) | 259 km/h | 28° / 36° | 1,18 G |
| 2026-09-04 16h36 | 0 | - | - | - | - | - | - |

Vmin par virage sur le record (km/h) : V1 138 · V2 131 · V3 110 · V4 70 · V5 59 · V6 102 · V7 98

### Lédenon - 2 journée(s), 6 session(s) - record 1'36.59 (2026-04-12 16h59, tour 5)

| Session | Tours | Meilleur | Idéal | Régularité | Vmax | Angle max G / D | Frein max |
| :--- | ---: | ---: | ---: | :--- | ---: | ---: | ---: |
| 2026-04-12 10h29 | 3 | 1'41.56 | 1'41.56 | ± 1,22 s (2 tours) | 224 km/h | 26° / 28° | 0,87 G |
| 2026-04-12 11h55 | 7 | 1'38.88 | 1'38.12 | ± 1,58 s (7 tours) | 232 km/h | 30° / 28° | 0,98 G |
| 2026-04-12 14h28 | 6 | 1'38.94 | 1'38.78 | ± 1,26 s (5 tours) | 234 km/h | 30° / 29° | 0,98 G |
| 2026-04-12 15h48 | 8 | 1'37.95 | 1'37.30 | ± 2,02 s (8 tours) | 235 km/h | 29° / 30° | 1,09 G |
| 2026-04-12 16h59 | 6 | 1'36.59 | 1'35.98 | ± 1,48 s (5 tours) | 233 km/h | 30° / 30° | 0,98 G |
| 2026-04-13 11h43 | 9 | 1'50.98 | 1'50.27 | ± 1,17 s (7 tours) | 222 km/h | 32° / 43° | 0,80 G |

Vmin par virage sur le record (km/h) : V1 101 · V2 107 · V3 61 · V4 55 · V5 135 · V6 99 · V7 68 · V8 77 · V9 97 · V10 135 · V11 76 · V12 129

### Alcarras - 2 journée(s), 11 session(s) - record 1'45.40 (2025-10-12 14h17, tour 6)

| Session | Tours | Meilleur | Idéal | Régularité | Vmax | Angle max G / D | Frein max |
| :--- | ---: | ---: | ---: | :--- | ---: | ---: | ---: |
| 2025-10-11 10h18 | 8 | 1'49.80 | 1'49.79 | ± 2,06 s (5 tours) | 248 km/h | n/d | n/d |
| 2025-10-11 11h19 | 9 | 1'46.28 | 1'45.59 | ± 1,60 s (8 tours) | 255 km/h | n/d | n/d |
| 2025-10-11 12h17 | 8 | 1'47.19 | 1'46.62 | ± 1,86 s (7 tours) | 250 km/h | n/d | n/d |
| 2025-10-11 14h14 | 7 | 1'47.19 | 1'46.73 | ± 0,65 s (6 tours) | 253 km/h | n/d | n/d |
| 2025-10-11 15h09 | 1 | 1'52.52 | 1'52.51 | - | 248 km/h | n/d | n/d |
| 2025-10-11 16h15 | 7 | 1'47.50 | 1'47.26 | ± 2,49 s (7 tours) | 252 km/h | n/d | n/d |
| 2025-10-11 17h05 | 1 | 1'53.49 | 1'53.47 | - | 251 km/h | n/d | n/d |
| 2025-10-12 09h18 | 7 (+1 écarté) | 1'45.71 | 1'45.70 | ± 1,73 s (7 tours) | 254 km/h | n/d | n/d |
| 2025-10-12 10h11 | 5 | 1'46.49 | 1'46.13 | ± 3,46 s (5 tours) | 256 km/h | n/d | n/d |
| 2025-10-12 11h17 | 8 | 1'46.70 | 1'45.94 | ± 0,85 s (7 tours) | 256 km/h | n/d | n/d |
| 2025-10-12 14h17 | 7 | 1'45.40 | 1'45.24 | ± 1,23 s (6 tours) | 254 km/h | n/d | n/d |

Vmin par virage : n/d (angle non enregistré sur la session du record).

<!-- AUTO:MESURES:FIN -->
