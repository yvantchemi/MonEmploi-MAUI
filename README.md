# MonEmploi Éducation

Application mobile **.NET MAUI** de construction d'emplois du temps pour un établissement d'enseignement.

## Problème traité

Dans beaucoup d'établissements, la planification des salles et des enseignants se fait encore sur fichier. Les conflits d'horaire (une salle réservée deux fois, un enseignant attendu dans deux classes à la même heure, un groupe convoqué à deux cours simultanés) ne sont découverts que tardivement, souvent le jour même. MonEmploi permet de construire l'emploi du temps séance par séance et refuse toute séance qui créerait un conflit de **salle**, d'**enseignant** ou de **groupe**, au moment même où on la saisit.

## Fonctionnalités

| Domaine | Ce que fait l'application |
|---|---|
| Ressources | Création, modification et suppression des salles (capacité, bâtiment), des enseignants (spécialité) et des groupes (filière, effectif). Une ressource utilisée par une séance ne peut pas être supprimée. |
| Création de séance | Matière, enseignant, groupe, salle, jour et horaires, avec validation des champs. |
| Détection de conflit | Vérification en temps réel pendant la saisie et blocage à l'enregistrement si la salle, l'enseignant ou le groupe est déjà pris sur un créneau qui se chevauche. Les séances en conflit sont signalées en rouge dans toutes les listes. |
| Vue par enseignant / par groupe | Emploi du temps filtré par enseignant, par groupe ou par salle, regroupé par jour, avec le volume horaire hebdomadaire. |
| Modification | Toute séance s'ouvre d'un simple toucher pour être modifiée (avec revérification des conflits) ou supprimée. |
| Export | Export de la vue affichée ou de l'emploi du temps complet en **CSV** (s'ouvre dans Excel) ou en **HTML** mis en forme (imprimable en PDF), puis partage via la feuille de partage du téléphone. |

### Fonctionnalités ajoutées (au-delà du cahier des charges)

| Ajout | Description |
|---|---|
| Tableau de bord | Chiffres clés, état global des conflits, séances du jour et taux d'occupation hebdomadaire de chaque salle. |
| Contrôle de capacité | Avertissement si l'effectif du groupe dépasse la capacité de la salle choisie, avec confirmation avant enregistrement. |
| Salle libre automatique | Le bouton « Salle libre ? » propose les salles disponibles sur le créneau et assez grandes pour le groupe, de la plus petite à la plus grande. |
| Recherche et filtre | Recherche par matière, enseignant, groupe ou salle, et filtre « conflits uniquement ». |
| Données de démonstration | Jeu de données réaliste chargé au premier lancement et rechargeable depuis l'accueil. |

## Règle de détection des conflits

Deux séances se chevauchent lorsqu'elles ont lieu le même jour et que `debutA < finB` et `debutB < finA` (une séance qui se termine à 10 h 00 ne bloque donc pas une séance qui commence à 10 h 00). Deux séances qui se chevauchent sont en conflit si elles partagent la même salle, le même enseignant ou le même groupe. La logique est centralisée dans `Services/ConflictService.cs`.

## Architecture

Le projet suit le modèle **MVVM** sans dépendance externe, avec injection de dépendances native de MAUI et navigation **Shell**.

```
MonEmploi/
├── Models/          Salle, Enseignant, Groupe, Seance, Jours, SeanceAffichage...
├── Services/        DataStore (données + persistance JSON), ConflictService, ExportService
├── ViewModels/      Un ViewModel par écran (BaseViewModel = INotifyPropertyChanged)
├── Views/           Pages XAML : Accueil, Séances, Édition de séance, Planning, Salles, Enseignants, Groupes
├── Converters/      InverseBoolConverter
├── Resources/       Styles (palette, styles), icônes SVG, splash
└── Platforms/       Android, Windows
```

Les données sont enregistrées localement dans `monemploi.json` (dossier de données de l'application), elles sont donc conservées entre deux lancements.

## Écrans

| Onglet | Contenu |
|---|---|
| Accueil | Tableau de bord |
| Séances | Liste de toutes les séances par jour, recherche, création |
| Planning | Vue par enseignant, par groupe ou par salle, export |
| Ressources | Trois sous-onglets : Salles, Enseignants, Groupes |

## Lancer le projet

Prérequis : Visual Studio 2026 (ou 2022 avec .NET 9) avec la charge de travail **Développement d'interface utilisateur d'application multiplateforme .NET (MAUI)**, et un émulateur Android configuré.

1. Ouvrir `MonEmploi.sln`.
2. Choisir la cible **Android Emulator** (ou **Windows Machine**).
3. Lancer avec F5.

Le projet cible .NET 10. Avec .NET 9, remplacer `net10.0` par `net9.0` dans les deux lignes `TargetFrameworks` de `MonEmploi.csproj`.

## Historique de réalisation

| Phase | Contenu | Repère Git |
|---|---|---|
| Phase A | Projet MAUI, charte graphique, modèles, interfaces des ressources et navigation | tag `phase-A` |
| Phase B | Persistance JSON, liste des séances, création de séance | tag `phase-B` |
| Phase C | Détection des conflits, modification/suppression, vues par enseignant/groupe, export | tag `phase-C` |
| Ajouts | Tableau de bord, capacité et salle libre, recherche et filtre | tag `v1.0` |

## Auteur

TCHEMI EUGENE YVAN ULRICK — Atelier de développement mobile (.NET MAUI).
