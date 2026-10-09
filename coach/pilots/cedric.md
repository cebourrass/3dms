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
