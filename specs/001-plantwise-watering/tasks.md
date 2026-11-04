# Tasks: PlantWise Watering Recommendation

## Phase 1: Setup

**Purpose**: Establish the first vertical-slice structure without external API calls.

- [x] T001 Create the application folder structure for `Models`, `Services`, `Repositories`, `Data`, and `Pages/Shared` under `Touching_Grass/`.
- [x] T002 [P] Create the xUnit test project files under `tests/Touching_Grass.UnitTests/` and add a project reference to `Touching_Grass/Touching_Grass.csproj`.
- [x] T003 [P] Add JSON sample-data files under `Touching_Grass/Data/` and configure them to copy to the output directory.
- [x] T004 [P] Verify `.gitignore` excludes `bin/`, `obj/`, `.vs/`, `*.user`, `*.suo`, and local secret/configuration files.

## Phase 2: Foundational

**Purpose**: Define shared contracts, configuration, and deterministic local-data loading before the user stories.

- [x] T005 Create `Touching_Grass/Models/Plant.cs` with required `Id`, `CommonName`, `ScientificName`, and `WaterFactor` properties; enforce `WaterFactor` as a decimal from 0 to 1.
- [x] T006 Create `Touching_Grass/Models/ZipLocation.cs` with required five-digit `ZipCode`, `Latitude`, `Longitude`, and `TimeZoneId` properties.
- [x] T007 Create `Touching_Grass/Models/WaterRecommendation.cs` with ZIP code, plant display name, weekly evapotranspiration inches, weekly rainfall inches, plant factor, gross need, net need, and explanation properties.
- [x] T008 Create `Touching_Grass/Services/IZipLookupService.cs`, `Touching_Grass/Repositories/IPlantRepository.cs`, and `Touching_Grass/Services/IWateringCalculatorService.cs` with asynchronous, cancellation-aware contracts.
- [ ] T009 Replace representative ZIP records with the approved nationwide USA ZIP dataset, including coordinates and IANA timezone values for every supported ZIP.
- [x] T010 Create `Touching_Grass/Data/plants.json` with representative plant records including common names, scientific names, and decimal water factors.
- [x] T011 Create `Touching_Grass/Services/JsonDataLoader.cs` to load and deserialize local JSON data without network access, returning actionable initialization errors for missing or malformed files.
- [x] T012 Register JSON data loading, `IZipLookupService`, `IPlantRepository`, and `IWateringCalculatorService` in `Touching_Grass/Program.cs` using scoped or singleton lifetimes appropriate to immutable local data.

## Phase 3: User Story 1 - Submit a watering lookup (P1)

**Goal**: A user can submit a supported ZIP code and plant name and receive a mock-weather recommendation.

**Independent Test**: Submit a supported ZIP and a known plant; verify a recommendation is rendered with weekly inches and the resolved location and plant.

- [x] T013 [P] [US1] Write ZIP lookup tests in `tests/Touching_Grass.UnitTests/ZipLookupServiceTests.cs` for trimming, exact five-digit matching, supported locations, and missing locations.
- [x] T014 [P] [US1] Write plant repository tests in `tests/Touching_Grass.UnitTests/PlantRepositoryTests.cs` for case-insensitive matching, whitespace trimming, known plants, and missing plants.
- [x] T015 [US1] Implement `Touching_Grass/Services/ZipLookupService.cs` to normalize ZIP input and return a matching `ZipLocation` or an unsupported-location result without network calls.
- [x] T016 [US1] Implement `Touching_Grass/Repositories/PlantRepository.cs` to load local plant JSON and return a unique case-insensitive plant match or a missing/ambiguous result.
- [x] T017 [US1] Implement `Touching_Grass/Services/WateringCalculatorService.cs` using temporary values `ET = 0.60` inches and `Rainfall = 0.10` inches and return the `WaterRecommendation` contract from T007.
- [x] T018 [US1] Write calculator tests in `tests/Touching_Grass.UnitTests/WateringCalculatorServiceTests.cs` for decimal factors, zero factor, rainfall offset, exact zero, and clamping negative need to zero.
- [x] T019 [US1] Create `Touching_Grass/Pages/Index.cshtml.cs` with bound ZIP and plant inputs, server-side validation, service orchestration, and a request-scoped `WaterRecommendation` result.
- [x] T020 [US1] Create `Touching_Grass/Pages/Index.cshtml` with Bootstrap-labeled ZIP and plant fields, validation summary, submit button, and accessible result/error regions.
- [x] T021 [US1] Add `Touching_Grass/Pages/Shared/_ValidationScriptsPartial.cshtml` or verify the existing partial supports the form's unobtrusive validation rules.
- [x] T022 [US1] Add `Touching_Grass/wwwroot/js/site.js` behavior to prevent duplicate submissions while preserving keyboard and screen-reader feedback.
- [x] T023 [US1] Add `Touching_Grass/Pages/Results.cshtml` and `Touching_Grass/Pages/Results.cshtml.cs` to render the recommendation, formula explanation, input values, units, and a return-to-lookup action.

## Phase 4: User Story 2 - Understand the calculation (P1)

**Goal**: The result explains exactly how the mock-weather recommendation was calculated.

**Independent Test**: Use a known plant factor and verify the result equals `(0.60 × factor) - 0.10`, clamped to zero, with all inputs displayed in inches.

- [x] T024 [P] [US2] Add calculation acceptance tests in `tests/Touching_Grass.UnitTests/WaterRecommendationPresentationTests.cs` for formula inputs, gross need, net need, units, and explanation text.
- [x] T025 [US2] Update `Touching_Grass/Models/WaterRecommendation.cs` to retain unrounded decimal values and a display-ready explanation generated from the same calculation inputs.
- [x] T026 [US2] Update `Touching_Grass/Pages/Results.cshtml` to display weekly evapotranspiration `0.60 in`, rainfall `0.10 in`, plant factor, gross need, net watering need, and the formula explanation.
- [x] T027 [US2] Add result formatting tests in `tests/Touching_Grass.UnitTests/WaterRecommendationFormattingTests.cs` to verify consistent decimal precision and that negative recommendations are never displayed.

## Phase 5: User Story 3 - Recover from invalid input or local-data failure (P1)

**Goal**: Invalid input and local-data errors produce clear, accessible messages without a recommendation.

**Independent Test**: Submit invalid ZIP/plant inputs and simulate missing or malformed JSON; verify field errors or a safe application error state is shown.

- [x] T028 [P] [US3] Write PageModel validation tests in `tests/Touching_Grass.UnitTests/IndexPageModelTests.cs` for empty ZIP, malformed ZIP, empty plant, unknown ZIP, and unknown plant.
- [x] T029 [P] [US3] Write data-load failure tests in `tests/Touching_Grass.UnitTests/JsonDataLoaderTests.cs` for missing files, malformed JSON, duplicate identifiers, and invalid factor ranges.
- [x] T030 [US3] Add data validation and typed failure results in `Touching_Grass/Services/JsonDataLoader.cs`, `Touching_Grass/Services/ZipLookupService.cs`, and `Touching_Grass/Repositories/PlantRepository.cs` so invalid local data never produces a recommendation.
- [x] T031 [US3] Update `Touching_Grass/Pages/Index.cshtml.cs` to map validation, unsupported-location, missing-plant, and local-data failures to user-safe messages without exposing file paths or stack traces.
- [x] T032 [US3] Update `Touching_Grass/Pages/Index.cshtml` and `Touching_Grass/Pages/Error.cshtml` to provide consistent Bootstrap error states, validation summaries, retry guidance, and accessible focus behavior.

## Phase 6: Polish and Cross-Cutting Validation

**Purpose**: Verify the complete first slice against the constitution and quickstart.

- [x] T033 [P] Run `dotnet format` or the repository-equivalent formatter on changed C# files and resolve introduced analyzer warnings.
- [x] T034 [P] Add `tests/Touching_Grass.UnitTests/AccessibilityAndValidationTests.cs` for labels, validation summaries, safe error messages, and no recommendation on failure.
- [x] T035 Run `dotnet test` for all unit tests and confirm normal, boundary, invalid-input, and local-data-failure cases pass.
- [x] T036 Run `dotnet build` for `Touching_Grass/Touching_Grass.csproj` and resolve all errors and new warnings.
- [x] T037 Run the application locally and validate the flow in `specs/001-plantwise-watering/quickstart.md` using a supported ZIP and known plant.
- [x] T038 Confirm the first slice makes no external HTTP calls and document the mock weather limitation in `specs/001-plantwise-watering/quickstart.md` or the implementation notes.

## Phase 7: Deferred Full-Feature Integration

**Purpose**: Follow-up backlog for the target EF Core and external-weather architecture;
do not execute this phase as part of the JSON/mock first vertical slice.

- [ ] T039 Replace JSON reference-data loading with EF Core SQL Server LocalDB entities, migrations, constraints, and repeatable seed/import in `Touching_Grass/Data/`, `Touching_Grass/Models/`, and `Touching_Grass/Migrations/`.
- [ ] T040 [P] Implement `Touching_Grass/Services/OpenMeteoService.cs` and its interface with seven local calendar days, unit normalization to inches, bounded timeout, and cancellation.
- [ ] T041 [P] Implement `Touching_Grass/Services/VisualCrossingService.cs` and its interface with secure API-key configuration, seven local calendar days, unit normalization to inches, bounded timeout, and cancellation.
- [ ] T042 [P] Add mocked HTTP contract tests in `tests/Touching_Grass.ContractTests/` for successful Open-Meteo and Visual Crossing responses, request date ranges, timezone, and unit conversion.
- [ ] T043 Add provider failure tests in `tests/Touching_Grass.ContractTests/` for timeout, cancellation, rate limiting, non-success responses, malformed payloads, missing values, invalid credentials, and partial-provider failure.
- [ ] T044 Integrate both weather services into `Touching_Grass/Services/WateringCalculatorService.cs` and ensure no partial or fabricated recommendation is returned when either required provider fails.
- [ ] T045 Add structured provider-failure logging in `Touching_Grass/Services/` and `Touching_Grass/Program.cs` without logging API keys, raw response bodies, or unnecessary user data.
- [ ] T046 Add a manual usability validation task in `specs/001-plantwise-watering/quickstart.md` to measure the first-attempt completion target for representative users.
- [ ] T047 Add a local performance measurement or benchmark in `tests/Touching_Grass.UnitTests/` for JSON lookup, calculation, and the full provider-integrated lookup against the ten-second target.

## Dependencies and Execution Order

### Phase Dependencies

- Phase 1 has no dependencies.
- Phase 2 depends on Phase 1 and blocks all user stories.
- Phase 3 depends on Phase 2 and delivers the MVP.
- Phase 4 depends on Phase 3 result contracts and UI.
- Phase 5 depends on Phase 3 service and PageModel boundaries.
- Phase 6 depends on completed desired user-story phases.
- Phase 7 is deferred until the first slice is accepted and depends on Phase 6; it is not
  part of the first-slice implementation checkpoint.

### User Story Dependencies

- US1 starts after Phase 2 and is the MVP vertical slice.
- US2 depends on US1's recommendation model and result page.
- US3 depends on US1's validation and local-data boundaries but can be implemented in parallel with US2 after US1 contracts exist.

### Parallel Opportunities

- T002-T004 can run in parallel during setup.
- T005-T007 and T009-T010 can run in parallel before service implementation.
- T013-T014 and T018 can run in parallel because they target separate test files.
- T024 and T027 can run in parallel after the recommendation contract exists.
- T028-T029 can run in parallel because they target separate test files.
- T033-T034 can run in parallel after feature code stabilizes.
- T040-T043 can run in parallel after the EF Core and configuration foundation in T039.

## Implementation Strategy

### MVP First

1. Complete Phases 1 and 2.
2. Complete Phase 3, including the form, local JSON lookup, mock calculation, and result page.
3. Run T035-T037 and validate the supported ZIP/plant flow.
4. Stop for a working demo before adding broader error and presentation coverage.

### Incremental Delivery

1. Phase 3 provides the complete mock-weather lookup.
2. Phase 4 makes the calculation auditable and consistently formatted.
3. Phase 5 hardens validation and local-data failures.
4. Phase 6 verifies quality, accessibility, build, and the no-external-API constraint.

All tasks use the required checklist format with sequential IDs, explicit file paths, and
story labels for user-story phases.
