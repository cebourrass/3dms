# Coach Claude - Base de connaissances

Connaissances utilisées par le coach Claude (futur serveur MCP, voir [TODO.md](../TODO.md)) pour analyser les sessions 3DMS et conseiller le pilote.

Deux niveaux, combinés à chaque analyse :

| Dossier | Rôle | Portée |
| :--- | :--- | :--- |
| [`knowledge/`](knowledge/) | **Expertise pilotage moto sur circuit** : technique, lecture de la télémétrie, méthode de coaching | Commune à tous les pilotes |
| [`pilots/`](pilots/) | **Connaissance dédiée de chaque pilote** : moto, niveau, objectifs, points forts, axes de travail, historique des apprentissages | Une fiche par pilote |

## Utilisation par le coach

1. Charger toute l'expertise `knowledge/` (socle commun).
2. Charger la fiche du pilote de la session (`pilots/<pilote>.md`, le pilote vient des infos de session). Sans fiche : coacher avec l'expertise seule et proposer de créer la fiche à partir de [`pilots/_modele.md`](pilots/_modele.md).
3. Analyser les données de la session via les outils MCP (tours, Delta Time, Vmin, régularité).
4. Formuler les conseils selon [`knowledge/06-methode-coaching.md`](knowledge/06-methode-coaching.md), en tenant compte des priorités et du vocabulaire de la fiche pilote.
5. Après la séance : proposer la mise à jour de la fiche pilote (apprentissages validés, nouveaux chantiers). La fiche n'est modifiée qu'avec l'accord du pilote.

## Exposition MCP prévue

- Chaque fichier est exposé comme **ressource MCP** (`coach://knowledge/<fichier>`, `coach://pilots/<pilote>`).
- Un **prompt MCP** « Débrief de session » assemble expertise + fiche pilote + résumé de la session.
- Les fiches pilotes peuvent aussi être reprises dans un skill Claude (c'est le cas du profil Cedric) : le fichier du repo reste la source de vérité versionnée.

## Règles de contenu

- Français, phrases courtes, orientées action.
- L'expertise reste générale (vraie pour tout pilote) ; tout ce qui est propre à un pilote ou à sa moto va dans sa fiche.
- Les fiches pilotes contiennent des données personnelles : ne versionner que ce que le pilote accepte de partager.
