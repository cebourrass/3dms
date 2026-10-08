# TODO List - 3DMS Analyzer

Liste des fonctionnalités et améliorations planifiées pour l'analyse de pilotage.

## 📊 Analyse de Performance
- [x] **Graphique de Delta Time** : Afficher une zone sous la télémétrie montrant l'écart cumulé (gain/perte de temps) en temps réel par rapport au tour de référence.
- [x] **Amélioration Visuelle Delta** : Synchronisation de la couleur de la courbe avec le tour comparé et ajout d'une ligne de référence à zéro.
- [x] **Correctif Superposition Delta** : Résoudre le conflit d'axes Y quand le Delta est affiché simultanément avec l'Angle ou les G (axe de droite partagé).
- [x] **Régularité par Secteur** : Calculer l'écart-type des temps par secteur sur les tours sélectionnés pour identifier les zones d'inconstance.
- [x] **Analyse des Vmin** : Identifier automatiquement les virages et comparer les vitesses minimales de passage entre les tours.
  - [x] Marqueurs Vmin sur la carte (couleur = gain/perte vs référence, virage sélectionné mis en évidence avec la Vmin de référence).
  - [x] Seuils de détection réglables (angle d'entrée/sortie, longueur minimale) dans PARAMÈTRES.
  - [ ] Détection plus robuste (minima locaux de vitesse en complément de l'angle, virages enchaînés/chicanes).
- [ ] **Potentiel Inexploité** : Améliorer le calcul du tour idéal en découpant le circuit en mini-secteurs (ex: tous les 100m) pour montrer la vitesse maximale théorique du pilote.

## ⚙️ Paramétrages Techniques
- [x] **Force du lissage** : Rendre ajustable la fenêtre de la moyenne glissante (actuellement fixée à 3 points).
- [x] **Densité d'interpolation** : Permettre de choisir la fréquence d'interpolation (50Hz, 100Hz, etc.).
- [ ] **Export de données** : Possibilité d'exporter les tours lissés en format CSV/Excel.

## 🎨 UI / UX
- [ ] **Légende Interactive** : Cliquer sur un tour dans la légende pour le mettre en surbrillance.
- [ ] **Zoom Synchronisé** : Améliorer le comportement du zoom pour qu'il reste centré sur le curseur.
- [ ] **Profils Pilotes** : Gérer une liste de pilotes persistante (aujourd'hui codée en dur) pour le panneau Infos Session.
- [ ] **Persistance des Infos Session** : Sauvegarder évènement, pilote, véhicule, pneus et notes avec chaque session.

## 🌐 Hub Team
- [ ] **Partage en ligne** : Création d'un espace pour partager et comparer ses datas avec les membres de son équipe/club.

## 🤖 Coach IA
- [ ] **Serveur MCP « Coach Claude »** : Exposer les données de l'analyseur via un serveur MCP pour qu'un coach Claude puisse analyser les sessions et conseiller le pilote.
  - Outils de lecture : lister les sessions/circuits, résumé d'une session (tours, meilleur tour, tour idéal, régularité), télémétrie d'un tour (échantillonnée), comparaison de deux tours (Delta Time, Vmin par virage).
  - Réutiliser les services existants (`Ra1ReaderService`, `LapService`, `CornerService`) dans une bibliothèque partagée entre l'appli WPF et le serveur MCP.
  - S'appuyer sur [GUIDE_PILOTAGE.md](GUIDE_PILOTAGE.md) et [docs/analysis_vmin.md](docs/analysis_vmin.md) comme base de connaissances du coach.

## 🛠️ Technique
- [ ] **Réglages dans %AppData%** : Stocker `user_settings.json` et `dock_layout.xml` dans `%AppData%` plutôt qu'à côté de l'exécutable (perdus à chaque changement de framework/dossier de build).
- [ ] **Mise à jour des paquets** : LiveCharts2 `2.0.0-rc2` → `2.0.5`, CommunityToolkit.Mvvm `8.2.2` → `8.4.2`, AvalonDock compatible .NET 10 (supprime le warning NU1701).
- [ ] **Sous-ViewModels** : Extraire de vrais sous-ViewModels (Charts, Map, Corners...) à partir des fichiers partiels de `MainViewModel`, en commençant par `MainViewModel.Charts.cs` (~1 160 lignes).
- [ ] **Projet de tests** : Tests automatisés (lecture .ra1, découpage des tours, détection des virages/Vmin) sur les sessions de test versionnées.
