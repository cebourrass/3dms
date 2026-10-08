# Documentation Technique - 3DMS Analyzer

Application WPF d'analyse des sessions enregistrées par un boîtier **3DMS Evo** (télémétrie moto : GPS, vitesse, angle, accélération).

## Stack

| Élément | Choix |
| :--- | :--- |
| Framework | .NET 10 (`net10.0-windows`), WPF |
| IDE conseillé | Visual Studio 2026 (ou `dotnet build` / `dotnet run` en ligne de commande) |
| Thème | ModernWpfUI |
| Graphiques | LiveChartsCore (SkiaSharp, WPF) |
| Fenêtres ancrables | DotNetProjects.AvalonDock |
| MVVM | CommunityToolkit.Mvvm (`ObservableObject`, `RelayCommand`) |

WinForms est activé (`UseWindowsForms`) uniquement pour `ColorDialog`, toujours appelé par son nom complet. `System.Windows.Forms` est retiré des usings implicites dans le `.csproj` pour éviter les conflits de noms avec WPF (`TextBox`, `MenuItem`, `DataGrid`...).

---

## Arborescence

```
Models/          Données (TelemetryPoint, LapData, SessionData, TrackMap, Corner, UserSettings, ExplorerItems)
Services/        Lecture fichiers et calculs sans état UI
  Ra1ReaderService      Lecture des sessions .ra1
  MapReaderService      Lecture des circuits .map
  LapService            Découpage en tours (passage de la ligne Start)
  CornerService         Détection des virages et comparaison des Vmin
  SettingsService       Persistance user_settings.json / dock_layout.xml
ViewModels/      MainViewModel (classe partielle, un fichier par domaine) + DisplayItems
Languages/       French.xaml / English.xaml (dictionnaires de ressources, mêmes clés)
Map/Circuits/    Bibliothèque de circuits .map (par pays) ; Map/Routes/ pour les routes
3DMS Evo (...)/  Sessions .ra1 de test versionnées (un sous-dossier par journée)
MainWindow.xaml  Vue unique : ruban (SESSION / VUE / PARAMÈTRES) + panneaux AvalonDock
```

### MainViewModel (classe partielle)

| Fichier | Contenu |
| :--- | :--- |
| `MainViewModel.cs` | Cœur : services, constructeur (chargement des réglages), `SaveSettings`, chargement de session, tours, tour de référence, langue, visibilité des panneaux |
| `MainViewModel.Explorer.cs` | Dossier des données (`DataFolderPath`) et arbre de l'explorateur |
| `MainViewModel.Circuits.cs` | Liste des circuits, détection automatique du circuit depuis le GPS, association session / .map |
| `MainViewModel.Charts.cs` | Styles des courbes, lissage / interpolation, construction des séries LiveCharts, Delta Time, curseur |
| `MainViewModel.Map.cs` | Projection GPS → canvas, trajectoires, zoom / rotation / pan, curseur carte, dégradé d'accélération |
| `MainViewModel.Corners.cs` | Analyse Vmin : seuils de détection, tableau comparatif, marqueurs sur la carte |
| `MainViewModel.Regularity.cs` | Écart-type par secteur et profils de seuils pilote |
| `MainViewModel.SessionInfo.cs` | Panneau infos session (pilote, véhicule, pneus, notes...) |

`DisplayItems.cs` regroupe les petites classes d'affichage (`CursorLapValue`, `LegendEntry`, `LapTrajectory`, `PilotProfile`, `CircuitMetadata`, `RegularityItem`).

---

## Flux de traitement

1. **Explorateur** : liste les `.ra1` du dossier des données (réglage SESSION → *Dossier des données*). Le dossier contient un sous-dossier par journée ; les `.ra1` posés à la racine sont aussi listés.
2. **Chargement** (`LoadSession`) : `Ra1ReaderService.ReadFile`, puis détection du circuit (dossier → cache, sinon comparaison GPS avec les `.map`).
3. **Tours** (`LapService.CalculateLaps`) : passage au plus près du marqueur `Start*` du `.map` (seuil 25 m), avec interpolation temporelle du passage exact.
4. **Lissage / interpolation** (`InterpolateAndSmooth`) : moyenne glissante par grandeur (vitesse, angle, accélération, GPS), fenêtres *Brut / Standard / Fort / Très fort* = 1 / 3 / 5 / 8 points, puis rééchantillonnage linéaire (10 à 100 Hz, 50 Hz par défaut).
5. **Graphiques** : axe X en temps pour un tour seul, en distance dès qu'on compare plusieurs tours ou qu'une référence est affichée. Delta Time cumulé par rapport à la référence.
6. **Carte** : projection équirectangulaire (cos(latitude) pour la longitude) dans un canvas 500×500 ; tracé du `.map`, trajectoires des tours (épaisseur selon le temps en comparaison), dégradé d'accélération optionnel.
7. **Analyse Vmin** (`CornerService`) : virages détectés sur le tour de référence (angle > seuil d'entrée → angle < seuil de sortie, longueur minimale), puis Vmin de chaque virage sur la référence et sur le tour sélectionné, à distance relative au début du tour. Marqueurs numérotés sur la carte (vert = gain, rouge = perte).
8. **Régularité** : écart-type des temps par secteur sur les tours sélectionnés, badges selon les seuils (profils pilote).

## Réglages et persistance

- `user_settings.json` et `dock_layout.xml` sont écrits **à côté de l'exécutable** (`bin/Debug/<tfm>/`). Changer de framework cible change le dossier : penser à recopier ces fichiers.
- Les réglages sont chargés dans le constructeur de `MainViewModel` et sauvegardés à la fermeture (`SaveSettings`).
- Les infos de session (pilote, véhicule, notes...) ne sont **pas** persistées pour l'instant.

## Localisation

`Languages/French.xaml` et `Languages/English.xaml` doivent contenir exactement les mêmes clés. Le XAML utilise `{DynamicResource Clé}` ; le code peut lire une ressource via `Application.Current.TryFindResource("Clé")`.

---

## Format .ra1 (session)

En-tête de 16 octets (signature `RA1` + version ASCII), puis des enregistrements de **28 octets**, little-endian :

| Offset (dans l'enregistrement) | Grandeur | Type |
| :--- | :--- | :--- |
| 0x00 | Temps depuis le début (ms) | uint32 |
| 0x04 | Longitude | float32 |
| 0x08 | Latitude | float32 |
| 0x0C | Vitesse (km/h) | float32 |
| 0x10 | Angle d'inclinaison (°) | float32 |
| 0x14 | Accélération longitudinale (G) | float32 |
| 0x18 | Réservé | 4 octets |

Les points physiquement impossibles (NaN, coordonnées hors bornes, vitesse > 600 km/h) sont écartés et comptés (`LastCorruptedPointsCount`, badge optionnel dans l'UI).

## Format .map (circuit)

| Offset | Contenu |
| :--- | :--- |
| 0 | En-tête (8 octets) + infos (32 octets) |
| 40 | Orientation (int32 : 0, 90, 180, 270) |
| 44 | Nombre de marqueurs (int32) |
| 48 | Marqueurs : longueur du nom (1 octet), nom ASCII, longitude (double), latitude (double) |
| ... | Largeur de piste en mètres (double), nombre de points (int32) |
| ... | Trajectoire : couples longitude / latitude (double, double) |

Le marqueur dont le nom commence par `Start` sert de ligne de chronométrage.

---

## Roadmap

Voir [TODO.md](TODO.md). Guide d'interprétation des courbes pour le pilote : [GUIDE_PILOTAGE.md](GUIDE_PILOTAGE.md). Analyse Vmin : [docs/analysis_vmin.md](docs/analysis_vmin.md).
