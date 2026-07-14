# MVVM remaster roadmap

Each numbered stage is developed on a dedicated branch created from
`mvvm-remaster`. After verification, the stage is merged back into
`mvvm-remaster` before the next branch is created.

| Stage | Branch | Outcome |
| ---: | --- | --- |
| 00 | `codex/mvvm-00-baseline` | Record current behavior, data formats, and migration invariants |
| 01 | `codex/mvvm-01-foundation` | .NET 10, Generic Host, dependency injection, ReactiveUI, logging, and tests |
| 02 | `codex/mvvm-02-persistence` | Typed models, repositories, application paths, and legacy data adapters |
| 03 | `codex/mvvm-03-shell-navigation` | One shell window and ReactiveUI-based navigation |
| 04 | `codex/mvvm-04-simple-screens` | Migrate menu, settings, about, knowledge base, results, and mode selection |
| 05 | `codex/mvvm-05-auth-profile` | Reactive authentication, session, and profile |
| 06 | `codex/mvvm-06-store` | Reactive store commands and removal of polling |
| 07 | `codex/mvvm-07-battle-engine` | UI-independent and tested battle domain engine |
| 08 | `codex/mvvm-08-battle-ui` | Reactive battlefield view model and collection-based enemy UI |
| 09 | `codex/mvvm-09-async-lifecycle` | Cancellation-aware cooldowns and screen lifecycle |
| 10 | `codex/mvvm-10-modern-storage` | Versioned JSON or SQLite storage and safe migration |
| 11 | `codex/mvvm-11-quality` | Broader tests, error handling, localization, themes, and packaging |

ReactiveUI source generators are the default for new code. Fody weaving is not
introduced because ReactiveUI currently recommends source generators for new
projects and treats `ReactiveUI.Fody` as a legacy option.

