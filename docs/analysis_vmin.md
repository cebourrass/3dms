# Analyse des Vmin (Vitesse Minimum en Virage)

L'analyse des **Vmin** est l'un des outils les plus puissants pour un pilote ou un coach afin d'améliorer la performance sur circuit. Elle permet de passer d'une analyse globale ("je perds du temps") à une analyse chirurgicale ("je rentre trop lentement dans l'épingle").

## 1. Logique de Fonctionnement

### Détection automatique des virages
Le système doit identifier les zones où le pilote est en phase de "négociation de courbe". Il existe deux méthodes complémentaires :
1.  **Par l'Angle d'Inclinaison (Lean Angle)** : Dès que l'inclinaison dépasse un certain seuil (ex: > 15°), nous sommes dans une zone de virage.
2.  **Par la Vitesse (Minima Locaux)** : Un virage se traduit presque toujours par une décélération suivie d'une accélération. Le point le plus bas de cette courbe est la **Vmin**.

### Extraction de la donnée
Pour chaque virage identifié sur un tour :
- On repère le point de distance où la vitesse est la plus basse.
- On extrait la valeur exacte de cette vitesse (ex: 74.2 km/h).
- On compare cette valeur entre le tour actuel et le tour de référence.

## 2. Intérêt pour le Pilote

L'intérêt est triple :

### A. Identifier la "Vitesse de Passage"
La vitesse au point de corde (apex) détermine souvent la qualité de l'entrée et l'efficacité de la sortie. 
- **Si votre Vmin est plus élevée** : Vous avez probablement lâché les freins plus tôt ou gardé plus de vitesse en entrée (meilleur "trail braking").
- **Si votre Vmin est trop basse** : Vous avez peut-être trop freiné ("over-braking") ou votre trajectoire est trop fermée.

### B. Actionnabilité immédiate
Contrairement au Delta Time qui est une accumulation, la Vmin est **ponctuelle**. 
- *Exemple* : "Au virage 4, je passe à 85 km/h alors que ma référence passe à 92 km/h". C'est un objectif clair pour la session suivante.

### C. Analyse de la Constance
En comparant les Vmin sur plusieurs tours, on peut voir si un pilote est régulier dans ses prises de risques ou s'il hésite sur certains virages spécifiques.

## 3. Implémentation Technique prévue

Dans le cadre du projet **3DMS-CED**, voici comment nous allons l'intégrer :

1.  **Algorithme de Slicing** : Découper le tour en segments basés sur les pics d'inclinaison ou les creux de vitesse.
2.  **Tableau Comparatif** : Ajouter un panneau sous les graphiques listant les virages (Virage 1, Virage 2, etc.) avec :
    *   Vmin Tour Actuel
    *   Vmin Référence
    *   Différence (Δ Vmin)
3.  **Indicateurs Visuels** : Placer des marqueurs sur la carte (Map) indiquant l'emplacement exact des Vmin pour corréler la vitesse avec la trajectoire.

---
> [!TIP]
> L'analyse de la Vmin est souvent plus importante que la Vmax. Gagner 5 km/h en Vmin sur un virage qui commande une ligne droite peut faire gagner plusieurs dixièmes à la fin de cette ligne droite.
