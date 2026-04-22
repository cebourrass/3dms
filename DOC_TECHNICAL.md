# Documentation Technique - 3DMS Analyzer

Cette documentation rÃ©sume la structure des classes et les responsabilitÃ©s du projet.

## FonctionnalitÃ©s d'Analyse AvancÃ©e

### 1. Traitement des DonnÃ©es (Smoothing & Interpolation)
Pour garantir un affichage fluide et professionnel :
- **Filtre de Lissage** : Application d'une moyenne glissante sur 3 points pour toutes les sÃ©ries (Vitesse, Angle, AccÃ©l).
- **Interpolation 50Hz** : Conversion des donnÃ©es brutes vers une base de temps fixe de 20ms (interpolation linÃ©aire).
- **LineSmoothness** : ParamÃ©trage de `LineSmoothness = 0.65` dans LiveCharts pour des courbes sans "pics".

### 2. Analyse de RÃ©gularitÃ© (Pilotage)
Calcul de l'Ã©cart-type ($\sigma$) pour aider le pilote Ã  identifier ses inconstances :
- **Par Secteur** : Calcul sur les tours sÃ©lectionnÃ©s pour comparaison.
- **Seuils Ajustables** : Configurables dans l'onglet PARAMÃˆTRES (Excellent/Moyen/Rouge).
- **Badges Visuels** : Retour visuel direct sous le tableau des tours.

---

## Architecture

L'application suit le pattern **MVVM** (Model-View-ViewModel) et utilise **WPF** avec le thÃ¨me **ModernWPF**.

### 1. ModÃ¨les (`Analyzer.Models`)

#### [TelemetryPoint.cs](file:///c:/dev/3DMS-CED/Models/TelemetryPoint.cs)
ReprÃ©sente un Ã©chantillon de donnÃ©e capturÃ© par le 3DMS Ã  un instant T.
- **Time** : Temps en ms (10 Hz).
- **Longitude / Latitude** : CoordonnÃ©es GPS.
- **Speed** : Vitesse en km/h.
- **LeanAngle** : Angle d'inclinaison calculÃ©.
- **Acceleration** : Force G longitudinale.

### 2. Services (`Analyzer.Services`)

#### [Ra1ReaderService.cs](file:///c:/dev/3DMS-CED/Services/Ra1ReaderService.cs)
Service de bas niveau pour l'extraction des donnÃ©es binaires.
- **MÃ©thode `ReadFile(string path)`** : Parse les fichiers `.ra1`.
    - Lit l'en-tÃªte de 16 octets.
    - ItÃ¨re sur les enregistrements de 28 octets.
    - GÃ¨re les types `uint32` et `float32` via `BinaryReader`.

### 3. ViewModels (`Analyzer.ViewModels`)

#### [MainViewModel.cs](file:///c:/dev/3DMS-CED/ViewModels/MainViewModel.cs)
CÅ“ur de l'application gÃ©rant l'Ã©tat de l'interface.
- **SpeedSeries / AngleSeries** : DonnÃ©es formatÃ©es pour LiveCharts2.
- **LoadSessionCommand** : Commande pour charger dynamiquement un fichier.
- **XAxes** : Configuration des axes temporels.

---

## Format de fichier .ra1 (SpÃ©cification brute)

| Offset | Grandeur | Type |
| :--- | :--- | :--- |
| 0x01 | "RA1" | ASCII (3 chars) |
| 0x05 | Version | ASCII (ex: "1.0.0.0") |
| 0x10 | Time | uint32 |
| 0x14 | Longitude | float32 |
| 0x18 | Latitude | float32 |
| 0x1C | Speed | float32 |
| 0x20 | LeanAngle | float32 |
| 0x24 | Acceleration | float32 |
| 0x28 | (Padding) | 4 bytes |

---

## Roadmap & Améliorations
Les idées d'améliorations pour l'analyse du pilotage (Delta Time, Écart-type, Vmin) sont répertoriées dans le fichier [TODO.md](file:///c:/dev/3DMS-CED/TODO.md).
