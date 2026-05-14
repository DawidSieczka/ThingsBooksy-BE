# 009 — Resources UI Handoff (Claude Design)

## 1. Model dziedziny (skrót)

**Dwupoziomowy model EAV:**

```
ManagementGroup (z innego modułu)
   └── ResourceType (np. "Car", "Meeting Room")     ← SCHEMA (template)
         ├── ResourcePropertyDefinition[]            ← pola schematu
         │      • Name (string)
         │      • DataType: Text | Number | Boolean
         │      • IsRequired (bool)
         └── ResourceInstance[]                     ← konkretne sztuki
                └── ResourcePropertyValue[]          ← wartości EAV (zawsze string, walidowane przy zapisie)
```

**Reguły biznesowe istotne dla UI:**
- Tylko **owner grupy** może tworzyć/edytować/usuwać typy i instancje. Każdy członek grupy może tylko *czytać*.
- **Usunięcie typu** jest możliwe wyłącznie gdy nie ma instancji (400 jeśli są).
- **Usunięcie instancji** = soft delete. Listing pokazuje skasowane tylko z `?includeDeleted=true`.
- Wszystkie zapisy wymagają wcześniej istniejącej `groupId` w lokalnym read-modelu — UI musi mieć kontekst „aktywnej grupy".
- Wartości Number są przechowywane jako string — UI musi walidować po stronie klienta i pokazywać sensowne błędy.
- Po **zmianie schematu** istniejące instancje pozostają nietknięte (ich wartości nie są re-walidowane).

## 2. Decyzja architektoniczna UI — DWA osobne widoki

Akceptuję propozycję i jasno rekomenduję rozdział:

| Widok                    | Odbiorca   | Częstotliwość | Złożoność | Mental model |
|--------------------------|------------|---------------|-----------|--------------|
| **Schema Designer**      | Owner      | Rzadko        | Wysoka    | "Projektant formularza" |
| **Resource Catalog**     | Wszyscy członkowie + Owner do CRUD instancji | Często | Niska | "Lista + edytowalna karta" |

**Dlaczego rozdzielić:**
- Inne persony — nietechniczny członek grupy nigdy nie wejdzie w schemę.
- Rzadkość operacji — schema raz na miesiąc, instancje codziennie.
- Złożoność stylu — drag-and-drop pól vs prosty formularz uzupełniania.
- Łatwiej egzekwować role (catalog tylko-do-odczytu dla non-ownerów; designer w ogóle ukryty).

## 3. Mapa widoków i nawigacji

```
Sidebar Workspace > Active Group: "Acme Fleet ▾"
   │
   ├── Resources           ← główny entry point
   │     ├── List view: kafelki/listę ResourceType ("Cars 7", "Meeting Rooms 3")
   │     │
   │     ├── /resources/types/:id   → Resource Catalog (instancje danego typu)
   │     │     ├── lista instancji (tabela z dynamic columns z PropertyDefinition)
   │     │     ├── action "+ Add resource"  → drawer/modal: dynamiczny formularz
   │     │     └── action "Edit schema"     → Schema Designer (tylko owner)
   │     │
   │     └── action "+ New resource type"   → Schema Designer (create mode)
   │
   └── (reszta modułów)
```

## 4. Wireframe — Schema Designer (Create/Edit ResourceType)

```
┌────────────────────────────────────────────────────────────────────────────┐
│ ← Back to Resources          DRAFT • unsaved        [Cancel] [Save schema] │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│   Resource type name *                                                     │
│   ┌──────────────────────────────────────────────┐                         │
│   │ Car                                          │                         │
│   └──────────────────────────────────────────────┘                         │
│                                                                            │
│   Description (optional)                                                   │
│   ┌──────────────────────────────────────────────┐                         │
│   │ Company fleet vehicle                        │                         │
│   └──────────────────────────────────────────────┘                         │
│                                                                            │
│   ─────────────────────────────────────────────────────────────────────    │
│                                                                            │
│   Fields                                          [+ Add field ▾]          │
│   These are the questions every car will need to answer.                   │
│                                                                            │
│   ┌──────────────────────────────────────────────────────────────┐         │
│   │ ⋮⋮  Power (HP)                  [Number ▾]  ☑ Required   🗑  │         │
│   ├──────────────────────────────────────────────────────────────┤         │
│   │ ⋮⋮  Avg fuel consumption        [Text   ▾]  ☐ Required   🗑  │         │
│   ├──────────────────────────────────────────────────────────────┤         │
│   │ ⋮⋮  Has roof rack               [Yes/No ▾]  ☑ Required   🗑  │         │
│   └──────────────────────────────────────────────────────────────┘         │
│                                                                            │
│   ╭── Live preview ──────────────────────────────────────────────╮         │
│   │ How members will see the form when adding a "Car":           │         │
│   │                                                              │         │
│   │   Power (HP) *           [        ]                          │         │
│   │   Avg fuel consumption   [        ]                          │         │
│   │   Has roof rack *        ( ) Yes  ( ) No                     │         │
│   ╰──────────────────────────────────────────────────────────────╯         │
│                                                                            │
│   ⚠ Editing a schema does NOT change existing resources.                   │
│      6 existing "Car" resources will keep their current data.              │
└────────────────────────────────────────────────────────────────────────────┘
```

**Kluczowe interakcje:**
- `[+ Add field ▾]` rozwija dropdown z 3 typami → klik dodaje wiersz na końcu listy (chip z typem na początku gotowy).
- Pola są **przeciągane** za uchwyt `⋮⋮` (drag-and-drop, choć kolejność nie wpływa na backend — wpływa na renderowanie w Catalog).
- **Inline rename** — kliknięcie nazwy aktywuje edycję.
- **Live preview** po prawej (na desktopie) lub poniżej (na mobile) — pokazuje formularz jaki zobaczy nietechniczny user. To kluczowe dla "easy".
- **Banner ostrzegawczy** w trybie edycji jeśli `instanceCount > 0` — informuje że zmiana DataType nie przewaliduje istniejących wartości.
- **Validation chips** pod każdym wierszem (np. „Name required" / „Two fields share this name").
- Klik `Save schema` → POST/PUT, toast „Schema saved", powrót do Resource Catalog.

**Empty state (przed pierwszym polem):**
```
   Add the first field
   A field is something every "Car" will need to fill in —
   like "Power (HP)" or "Has roof rack".

   [+ Add field ▾]
```

## 5. Wireframe — Resource Catalog (lista + dodawanie instancji)

```
┌────────────────────────────────────────────────────────────────────────────┐
│ Resources / Cars                                                           │
│ Company fleet vehicles · 7 active · 2 archived          🔧 Edit schema     │
├────────────────────────────────────────────────────────────────────────────┤
│ 🔎 Search by name…              ☐ Show archived              [+ Add car]   │
├────────┬────────────┬──────────────────────┬─────────────┬─────────────────┤
│ Name   │ Power (HP) │ Avg fuel consumption │ Roof rack   │  Added       ⋮  │
├────────┼────────────┼──────────────────────┼─────────────┼─────────────────┤
│ Car 1  │ 116        │ 3.8–4.4 l/100 km     │ Yes ✓       │ 2 days ago   ⋮  │
│ Car 2  │ 130        │ 5.5–6.0 l/100 km     │ No  ✗       │ 5 days ago   ⋮  │
│ Car 3  │ 95         │ —                    │ Yes ✓       │ 1 week ago   ⋮  │
│ Car 4  │ 180        │ 6.1 l/100 km         │ No  ✗       │ 2 weeks ago  ⋮  │
│ ⋯                                                                          │
│ ─────────────────────────────────────────────────────────────────────────  │
│ ▒ Car 6  (archived 2026-05-02)                                          ⋮  │
└────────────────────────────────────────────────────────────────────────────┘
                                            wiersz „⋮" → View / Edit / Archive
```

**Drawer „+ Add car" (slide-in z prawej):**

```
┌──────────────────────────────────────────┐
│ Add new car                          ✕   │
├──────────────────────────────────────────┤
│ Name *                                   │
│ [ Car 8                              ]   │
│                                          │
│ Description (optional)                   │
│ [                                    ]   │
│                                          │
│ ─── Properties ───                       │
│                                          │
│ Power (HP) *                             │
│ [ 142                                ]   │
│                                          │
│ Avg fuel consumption                     │
│ [                                    ]   │
│ Leave empty if unknown.                  │
│                                          │
│ Has roof rack *                          │
│ ( ) Yes      (•) No                      │
│                                          │
├──────────────────────────────────────────┤
│             [Cancel]   [Save resource]   │
└──────────────────────────────────────────┘
```

**Edit mode** używa tego samego drawera, po prostu z `PUT` zamiast `POST`. **View mode** (read-only dla non-ownera) renderuje to samo, ale wszystkie inputy `disabled` + brak przycisku Save; dla ownera „View" przełącza się w „Edit" jednym CTA.

**Detail page** (klik w wiersz tabeli) — alternatywa do drawera, dla wygodnego linkowania (`/resources/instances/:id`). Pokazuje header z nazwą, badge typu, audytową stopkę (owner, createdAt, deletedAt jeśli archived) i pełną listę properties w grupach.

## 6. Mapowanie pól → typ kontrolki

| `DataType` (BE) | Kontrolka w Schema Designer | Kontrolka w Catalog form | Walidacja FE |
|-----------------|----------------------------|--------------------------|--------------|
| `Text`          | brak dodatkowych pól       | `<input type="text">`    | `required` jeśli flag |
| `Number`        | brak dodatkowych pól       | `<input type="number" inputmode="decimal">` | `required` + regex numeryczny |
| `Boolean`       | brak dodatkowych pól       | Radio Yes/No (nie checkbox — wymagane pola muszą być świadomie wybrane) | jeśli `Required` — żadna opcja niezaznaczona = błąd |

Wartość zawsze idzie do BE jako `string` (`"true"` / `"false"` dla bool, `"142"` dla number) — to musi obsłużyć FE serializer (mapper `PropertyValueInputDto`).

## 7. API mapping (frontend hook → endpoint)

| Akcja UI                                     | Endpoint                                |
|---------------------------------------------|-----------------------------------------|
| Lista typów w grupie                        | `GET /resources/types?groupId={id}`     |
| Otwarcie typu (przed wejściem do Catalog)   | `GET /resources/types/{id}`             |
| Save schemy (create)                        | `POST /resources/types`                 |
| Save schemy (edit)                          | `PUT /resources/types/{id}`             |
| Usuń typ                                    | `DELETE /resources/types/{id}` (UI musi obsłużyć 400 = "has instances") |
| Lista instancji                             | `GET /resources/instances?resourceTypeId={id}&includeDeleted={bool}` |
| Detail instancji                            | `GET /resources/instances/{id}`         |
| Add resource                                | `POST /resources/instances`             |
| Edit resource                               | `PUT /resources/instances/{id}`         |
| Archive resource                            | `DELETE /resources/instances/{id}`      |

## 8. Stany, błędy, edge cases (do pokazania w Claude Design)

- **403 (non-owner próbuje pisać)** — przyciski edycji są ukryte, ale w razie wyścigu pokaż toast „Only the group owner can do that".
- **400 przy usuwaniu typu z instancjami** — modal: „Cars has 7 resources. Archive or move them before deleting the type."
- **400 przy create instance** z brakującą required property — error chip pod konkretnym polem.
- **Soft-deleted listing** — wiersze z 50% opacity, badge „Archived", action „Restore" (jeśli backend doda; obecnie brak — UI tylko wyświetla).
- **Pusta grupa** — full-page empty state „No resource types yet. What does your group own? Start by adding a type like 'Cars' or 'Meeting rooms'." + CTA.
- **Zmiana DataType pola po stronie schemy** — confirmation modal, bo istniejące wartości zostają nietknięte (potential garbage).
- **Loader skeletons** dla tabeli i drawerów.

## 9. Accessibility / interakcje

- Cała tabela nawigowalna klawiaturą, sortowalne kolumny (po Name + Added; properties bez sortu w MVP).
- Drawer trapy fokus, `Esc` zamyka, `Enter` w polu nie submituje (tylko Tab → przycisk).
- Wszystkie radio/checkboxy mają `<label for>` (`Has roof rack` to oddzielny `fieldset` + `legend`).
- Stany walidacji `aria-invalid` + `aria-describedby` na error chipie.
- Drag-and-drop polami w designerze ma **alternatywę klawiaturową** (`↑/↓` na uchwycie po fokusie).

## 10. Tokeny / styling

Trzymać się już istniejących `_tokens.scss` z konwencji `angular-styling.md`. Niczego nie wymyślać — używać:
- `--color-primary` na CTA,
- `--color-danger` na delete,
- `--color-surface` / `--color-surface-elevated` (drawer, preview card),
- `--space-{2,3,4,6,8}` (8px grid),
- `--radius-md` dla kart i inputów.

Mobile-first — Schema Designer na <md ma live-preview pod listą pól, drawer staje się full-screen sheet.

## 11. PROMPT GOTOWY DO WKLEJENIA W CLAUDE DESIGN

```
Design a two-screen UI for a SaaS "Resources" module aimed at NON-TECHNICAL users
who manage bookable items inside a workgroup.

CONTEXT
- Two-level data model:
    ResourceType (schema template) → ResourceInstance (concrete bookable item)
- Each ResourceType has a list of PropertyDefinitions (Name, DataType ∈ {Text,
  Number, Boolean}, IsRequired flag).
- Each ResourceInstance carries PropertyValues matching its type's schema.
- Only the workgroup OWNER edits schema and instances; other members can only
  view.

DELIVER TWO SCREENS

SCREEN 1 — "Schema Designer" (/resources/types/:id/edit)
Goal: a non-technical owner builds a clickable schema like "Car has: Power
(Number, required), Fuel consumption (Text, optional), Has roof rack (Boolean,
required)".
- Top: editable Type name + optional description.
- Center: a vertical list of property rows; each row = drag handle + Name input
  + DataType pill selector (Text / Number / Yes-No) + Required toggle + delete.
- "+ Add field" button at the bottom of the list (dropdown picks DataType so the
  row appears pre-typed).
- Right-side LIVE PREVIEW pane showing exactly the form a member will fill in
  when creating an instance (label, control kind, required asterisk). Updates in
  real time.
- Warning banner in edit mode: "Editing this schema does NOT change existing
  resources" + a count of how many instances will keep old data.
- Save / Cancel sticky footer.
- Mobile: preview collapses below the form.

SCREEN 2 — "Resource Catalog" (/resources/types/:id)
Goal: members of the workgroup browse and fill in resources for a given type.
- Header with type name, description, "Edit schema" CTA (owner-only), counts
  (active, archived).
- Toolbar: search by name, "Show archived" toggle, "+ Add <type-name>" primary
  CTA (owner-only).
- Table where COLUMNS are the type's PropertyDefinitions in order. Boolean
  values render as ✓ / ✗ pills, Numbers right-aligned, Text wraps.
- Row action menu: View / Edit / Archive (owner-only for the latter two).
- Archived rows shown dimmed with an "Archived" badge when the toggle is on.
- "+ Add" opens a right-side DRAWER with a dynamic form generated from the
  schema: Text → text input, Number → numeric input with inputmode=decimal,
  Boolean → Yes/No radio fieldset. Required fields marked with *.
- Drawer footer: Cancel / Save.
- Empty state when no instances exist: friendly illustration + "No <type-name>s
  yet. Add the first one." CTA.

STYLE
- Modern SaaS, generous whitespace, 8px spacing scale, rounded-md corners.
- Calm neutral palette + one primary accent for CTAs, red only for destructive
  actions.
- Strong accessibility: keyboard-navigable table, focusable drag handles,
  labeled fieldsets for radios, visible focus rings.
- Mobile-first responsive layout: drawer becomes full-screen sheet under md.

NON-GOALS
- Booking / reservation flow — out of scope.
- Permissions UI — handled elsewhere.
- Multi-group switching — assume a single active group is set globally.
```
