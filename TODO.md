# TODO List - 3DMS Analyzer

Liste des fonctionnalités et améliorations planifiées pour l'analyse de pilotage.

## 📊 Analyse de Performance
- [ ] **Graphique de Delta Time** : Afficher une zone sous la télémétrie montrant l'écart cumulé (gain/perte de temps) en temps réel par rapport au tour de référence.
- [x] **Régularité par Secteur** : Calculer l'écart-type des temps par secteur sur les tours sélectionnés pour identifier les zones d'inconstance.
- [ ] **Analyse des Vmin** : Identifier automatiquement les virages et comparer les vitesses minimales de passage entre les tours.
- [ ] **Potentiel Inexploité** : Améliorer le calcul du tour idéal en découpant le circuit en mini-secteurs (ex: tous les 100m) pour montrer la vitesse maximale théorique du pilote.

## ⚙️ Paramétrages Techniques
- [ ] **Force du lissage** : Rendre ajustable la fenêtre de la moyenne glissante (actuellement fixée à 3 points).
- [ ] **Densité d'interpolation** : Permettre de choisir la fréquence d'interpolation (50Hz, 100Hz, etc.).
- [ ] **Export de données** : Possibilité d'exporter les tours lissés en format CSV/Excel.

## 🎨 UI / UX
- [ ] **Légende Interactive** : Cliquer sur un tour dans la légende pour le mettre en surbrillance.
- [ ] **Zoom Synchronisé** : Améliorer le comportement du zoom pour qu'il reste centré sur le curseur.
