# Cahier des charges — AgriActifs

> Projet technique : `AgriActifs.Web`  
> Famille produit : **FermeActifs** (écosystème GISEBS)  
> Type : **Identity-Based Application** · **RBAC** · multi-exploitation  
> Socle : fork de [`GISERBAC-TEMPLATE`](../../GISERBAC-TEMPLATE/docs/CAHIER_DES_CHARGES.md)  
> Références métier : isolation type ComptaDoc (`Company` → `Exploitation`), inventaire / équipements

---

## 1. Objectif

Fournir une application **ASP.NET Core MVC** complète pour la **gestion des actifs d’une ferme agricole** (cultures, parcelles, matériel, stocks, bâtiments, maintenance), avec :

- authentification Identity + RBAC GISEBS ;
- isolation des données par **exploitation** (ferme) ;
- registre d’actifs robuste (équipements, bâtiments, parcelles, stocks) ;
- cycle de vie (acquisition → utilisation → maintenance → réforme) ;
- tableaux de bord opérationnels et rapports ;
- comptes de démonstration prêts à l’emploi ;
- intégration optionnelle SecureMail / Pay Gateway / Support Hub.

```
Utilisateur → Rôles → Permissions → Ressources (scopées ExploitationId)
```

---

## 2. Périmètre

### 2.1 Inclus (MVP + V1)

| Domaine | Description |
|---------|-------------|
| Exploitation | Multi-fermes, membres, rôles métier |
| Parcelles & cultures | Cadastre opérationnel, assolement, campagnes |
| Actifs matériels | Tracteurs, outils, irrigation, bâtiments |
| Stocks agricoles | Semences, engrais, phytosanitaires, récoltes |
| Maintenance | Interventions, planning, pièces, coûts |
| Documents & pièces jointes | Factures, manuels, photos, certificats |
| Dashboard & rapports | KPI actifs, alertes, exports |
| Admin RBAC | Utilisateurs, rôles, audit, paramètres |

### 2.2 Hors périmètre (évolutions)

- Comptabilité générale complète (renvoi vers ComptaDoc)
- Capteurs IoT / télémétrie temps réel
- Cartographie GIS avancée (hors fond cadastral simple)
- Marketplace d’équipements d’occasion

---

## 3. Stack technique (alignée écosystème)

| Composant | Technologie |
|-----------|-------------|
| Framework | ASP.NET Core **10** MVC |
| Authentification | ASP.NET Core Identity (cookies) |
| Autorisation | Rôles + policies dynamiques `Permission:{code}` |
| ORM | Entity Framework Core |
| Base de données | **PostgreSQL** (Npgsql), schéma `agriactifs` |
| UI Auth | Identity Razor Pages (`Areas/Identity`) |
| UI métier | Bootstrap **5.3** + Bootstrap Icons + thème `--gise-*` |
| Localisation | FR (défaut) / EN |
| Jobs (option V1.1) | Hangfire pour rappels maintenance / alertes stocks |
| Tests | Projet `AgriActifs.Tests` (xUnit) |
| Déploiement | `deploy/` (systemd / GHA), `project.config.json` |

**AppCode** intégrations : `AGRIACTIFS`

---

## 4. Acteurs et rôles

### 4.1 Rôles système (Identity — template GISEBS)

| Rôle | Usage |
|------|--------|
| SuperAdmin | Plateforme entière, seed, paramétrage global |
| Admin | Administration app + toutes exploitations |
| Manager | Pilotage multi-modules |
| User | Opérateur métier |
| Auditor | Lecture + audit |
| ReportViewer | Rapports / exports uniquement |

### 4.2 Rôles métier par exploitation

| Rôle exploitation | Responsabilités |
|-------------------|-----------------|
| Proprietaire | Tous droits sur l’exploitation |
| Gerant | Gestion opérationnelle, validation |
| Technicien | Saisie actifs, maintenance, stocks |
| Ouvrier | Consultation + saisie limitée (interventions, mouvements) |
| Observateur | Lecture seule |

Isolation : toutes les entités métier portent `ExploitationId` (équivalent ComptaDoc `CompanyId`).

---

## 5. Modules fonctionnels

### 5.0 Socle RBAC (hérité du template)

- Authentification (login, reset MDP, 2FA, lockout)
- CRUD utilisateurs / rôles / assignations
- Profil utilisateur
- Area `Admin` (dashboard, audit, `SystemSettings`)
- Permissions dynamiques + `AuditLog`

### 5.1 Exploitations

- CRUD exploitation (nom, adresse, superficie totale, type de production, NEQ / identifiants)
- Multi-appartenance utilisateur ↔ exploitations
- Sélection du contexte courant (cookie / claim `ExploitationId`)
- Paramètres : unités (ha, acres), devise, fuseau, saison culturale

**Area** : `Ferme`  
**Permissions** : `Exploitations.View`, `Exploitations.Manage`, `Exploitations.Members`

### 5.2 Parcelles

- Parcelle : code, nom, superficie, type de sol, irrigation, statut
- Historique d’assolement (culture / saison / rendement)
- Campagnes culturales (année / saison)
- Lien parcelle ↔ actifs (ex. système d’irrigation rattaché)

**Permissions** : `Parcelles.View`, `Parcelles.Manage`

### 5.3 Registre des actifs (cœur métier)

Catégories d’actifs agricoles :

| Catégorie | Exemples |
|-----------|----------|
| MaterielRoulant | Tracteurs, moissonneuses, remorques |
| Outillage | Cultivateurs, semoirs, pulvérisateurs |
| Irrigation | Pompes, pivots, tuyauterie |
| Batiment | Hangars, silos, serres |
| Infrastructure | Clôtures, chemins, drainage |
| Autre | Actifs divers inventoriés |

Fiche actif :

- Identifiant interne + numéro de série / plaque
- Marque, modèle, année, valeur d’acquisition, date d’acquisition
- Statut : `EnService`, `EnMaintenance`, `HorsService`, `Reforme`, `Loue`
- Localisation (parcelle / bâtiment)
- Amortissement simple (durée, valeur résiduelle) — hint comptable, pas GL
- Documents attachés, photos
- Historique des mouvements / affectations

**Permissions** : `Actifs.View`, `Actifs.Create`, `Actifs.Edit`, `Actifs.Delete`, `Actifs.Export`

### 5.4 Stocks agricoles

- Articles : SKU, nom, unité, catégorie (semences, engrais, phyto, récolte, pièces)
- Quantité, seuil de réapprovisionnement, coût unitaire
- Mouvements : entrée, sortie, ajustement, transfert
- Alertes stock bas
- Traçabilité lot / date d’expiration (phyto, semences)

**Permissions** : `Stocks.View`, `Stocks.Manage`, `Stocks.Adjust`

### 5.5 Maintenance & interventions

- Ordres de travail (préventif / correctif)
- Planning (échéance, récurrence km/heures/mois)
- Pièces consommées (lien stock)
- Coût main-d’œuvre + pièces
- Clôture avec rapport d’intervention

**Permissions** : `Maintenance.View`, `Maintenance.Manage`, `Maintenance.Close`

### 5.6 Fournisseurs & acquisitions

- Annuaire fournisseurs
- Enregistrement d’acquisition / location d’actif
- Lien facture (fichier) sans écriture comptable

**Permissions** : `Fournisseurs.View`, `Fournisseurs.Manage`

### 5.7 Dashboard & alertes

KPI :

- Nombre d’actifs par statut / catégorie
- Valeur brute du parc matériel
- Interventions ouvertes / en retard
- Stocks sous seuil
- Campagnes en cours (superficie emblavée)

### 5.8 Rapports

| Rapport | Description |
|---------|-------------|
| Inventaire actifs | Liste filtrable + export Excel/PDF |
| Valeur du parc | Synthèse acquisition / résiduelle |
| Maintenance | Interventions période |
| Stocks | Niveaux + mouvements |
| Assolement | Parcelles × cultures × saison |

**Permissions** : `Reports.View`, `Reports.Export`

### 5.9 Intégrations écosystème GISEBS

| Service | Usage | Identifiant |
|---------|-------|-------------|
| SecureMailGateway | Notifications (maintenance, stock bas, invitation membre) | `AGRIACTIFS` |
| GiseBsPayGateway | Abonnement SaaS (option) | `X-App-Code: AGRIACTIFS` |
| GiseSupportHub | Widget support / tickets | API Key client |

---

## 6. Modèle de données (simplifié)

```
ApplicationUser / ApplicationRole / UserProfile   (Identity + RBAC)
AuditLog / SystemSettings / ReportDefinition

Exploitation
  ├── ExploitationUser (UserId, Role)
  ├── Parcelle
  │     └── Assolement (Culture, Saison, Rendement)
  ├── ActifAgricole
  │     ├── ActifDocument
  │     ├── ActifAffectation
  │     └── InterventionMaintenance
  ├── StockArticle
  │     └── StockMouvement
  ├── Fournisseur
  └── CampagneCulturale
```

Contraintes :

- Soft-delete ou statut `IsActive` sur entités majeures
- Horodatage `CreatedAt` / `UpdatedAt` / `CreatedBy`
- Index sur `ExploitationId` + codes métier uniques par exploitation

---

## 7. Matrice permissions métier (extrait)

| Permission | SuperAdmin | Admin | Proprietaire* | Gerant* | Technicien* | Ouvrier* | Observateur* |
|------------|:----------:|:-----:|:-------------:|:-------:|:-----------:|:--------:|:------------:|
| Exploitations.View | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Exploitations.Manage | ✅ | ✅ | ✅ | | | | |
| Actifs.View | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Actifs.Create / Edit | ✅ | ✅ | ✅ | ✅ | ✅ | | |
| Actifs.Delete | ✅ | ✅ | ✅ | | | | |
| Stocks.Manage | ✅ | ✅ | ✅ | ✅ | ✅ | | |
| Stocks.Adjust | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | |
| Maintenance.Manage | ✅ | ✅ | ✅ | ✅ | ✅ | | |
| Maintenance.Close | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | |
| Reports.Export | ✅ | ✅ | ✅ | ✅ | | | ✅ |

\*Rôles scopés à l’exploitation courante (grants type ComptaDoc).

---

## 8. Comptes de démonstration (seed)

Seed au démarrage après `MigrateAsync` (idempotent).

### 8.1 Comptes plateforme

| Email | Mot de passe | Rôle |
|-------|--------------|------|
| `superadmin@agriactifs.local` | `Agri@Secure2026!` | SuperAdmin |
| `admin@agriactifs.local` | `Agri@Admin2026!` | Admin |

### 8.2 Exploitation démo — « Ferme des Érables »

| Email | Mot de passe | Rôle exploitation |
|-------|--------------|-------------------|
| `gerant@fermedeserables.demo` | `Demo@Agri2026!` | Gerant |
| `tech@fermedeserables.demo` | `Demo@Agri2026!` | Technicien |
| `ouvrier@fermedeserables.demo` | `Demo@Agri2026!` | Ouvrier |
| `lecture@fermedeserables.demo` | `Demo@Agri2026!` | Observateur |

### 8.3 Données démo minimales

- 1 exploitation (80 ha, grandes cultures)
- 5 parcelles + assolement saison courante
- 8 actifs (2 tracteurs, 1 moissonneuse, outillage, hangar, pivot)
- 12 articles de stock + 2 alertes seuil
- 4 interventions (2 ouvertes, 2 clôturées)
- 2 fournisseurs

Flag : `Seed:IncludeDemoData=true` (désactivable en prod).

---

## 9. Structure projet

```
AgriActifs/
├── AgriActifs.slnx
├── README.md
├── docs/
│   ├── CAHIER_DES_CHARGES.md      ← ce document
│   ├── ARCHITECTURE_RBAC.md       (copie adaptée template)
│   └── SECUREMAIL_INTEGRATION.md
├── src/AgriActifs.Web/
│   ├── Areas/
│   │   ├── Admin/
│   │   ├── Identity/
│   │   └── Ferme/                 # modules métier
│   ├── Authorization/
│   ├── Constants/
│   ├── Controllers/
│   ├── Data/                      # DbContext, Seed, Migrations
│   ├── Extensions/
│   ├── Models/
│   ├── Services/
│   └── Views/
├── tests/AgriActifs.Tests/
└── deploy/
```

---

## 10. Exigences non fonctionnelles

| Critère | Cible |
|---------|-------|
| Sécurité | CSRF, HTTPS, cookies Secure, MDP 12+, lockout, audit |
| Performance | Listes paginées (< 200 ms locales typiques) |
| Disponibilité | Déploiement standard GISEBS |
| Accessibilité | Contrastes Bootstrap, labels formulaires |
| Observabilité | Logs structurés + `AuditLog` |
| Secrets | Hors code en prod (User Secrets / env) |
| Qualité | Build + tests unitaires services critiques |

---

## 11. Critères d’acceptation (MVP)

- [ ] Projet compilable .NET 10, schéma PostgreSQL `agriactifs`
- [ ] Seed rôles + SuperAdmin + comptes démo
- [ ] Contexte exploitation + isolation des données
- [ ] CRUD parcelles, actifs, stocks, interventions
- [ ] Dashboard KPI + alertes stock / maintenance
- [ ] Area Admin RBAC fonctionnelle
- [ ] Export inventaire Excel (minimum)
- [ ] UI Bootstrap 5 FR, responsive mobile
- [ ] Tests unitaires seed + scoping ExploitationId

---

## 12. Phasage

| Phase | Contenu | Durée indicative |
|-------|---------|------------------|
| **P0** | Fork template, Identity, seed, Exploitation | 1 itération |
| **P1** | Actifs + Parcelles + Dashboard | 1–2 itérations |
| **P2** | Stocks + Maintenance + Fournisseurs | 1–2 itérations |
| **P3** | Rapports, SecureMail, polish UX, tests | 1 itération |
| **P4** | Pay Gateway / Support Hub / Hangfire | optionnel |

---

## 13. Commandes utiles (cible)

```powershell
dotnet ef migrations add InitialCreate --project src/AgriActifs.Web
dotnet ef database update --project src/AgriActifs.Web
dotnet run --project src/AgriActifs.Web
```

---

## 14. Évolutions prévues

1. Lien bidirectionnel inventaire / immobilisations **ComptaDoc**
2. Cartographie parcelles (GeoJSON)
3. Application mobile lecture/écriture interventions
4. API REST (mêmes policies) pour partenaires
5. Module assurance / sinistres sur actifs
