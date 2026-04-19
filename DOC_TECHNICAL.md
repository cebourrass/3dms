# Documentation Technique - 3DMS Analyzer

Cette documentation résume la structure des classes et les responsabilités du projet.

## Architecture

L'application suit le pattern **MVVM** (Model-View-ViewModel) et utilise **WPF** avec le thème **ModernWPF**.

### 1. Modèles (`Analyzer.Models`)

#### [TelemetryPoint.cs](file:///c:/dev/3DMS-CED/Models/TelemetryPoint.cs)
Représente un échantillon de donnée capturé par le 3DMS à un instant T.
- **Time** : Temps en ms (10 Hz).
- **Longitude / Latitude** : Coordonnées GPS.
- **Speed** : Vitesse en km/h.
- **LeanAngle** : Angle d'inclinaison calculé.
- **Acceleration** : Force G longitudinale.

### 2. Services (`Analyzer.Services`)

#### [Ra1ReaderService.cs](file:///c:/dev/3DMS-CED/Services/Ra1ReaderService.cs)
Service de bas niveau pour l'extraction des données binaires.
- **Méthode `ReadFile(string path)`** : Parse les fichiers `.ra1`.
    - Lit l'en-tête de 16 octets.
    - Itère sur les enregistrements de 28 octets.
    - Gère les types `uint32` et `float32` via `BinaryReader`.

### 3. ViewModels (`Analyzer.ViewModels`)

#### [MainViewModel.cs](file:///c:/dev/3DMS-CED/ViewModels/MainViewModel.cs)
Cœur de l'application gérant l'état de l'interface.
- **SpeedSeries / AngleSeries** : Données formatées pour LiveCharts2.
- **LoadSessionCommand** : Commande pour charger dynamiquement un fichier.
- **XAxes** : Configuration des axes temporels.

---

## Format de fichier .ra1 (Spécification brute)

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
