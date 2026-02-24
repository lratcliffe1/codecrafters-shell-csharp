# C# Unix-like Shell

This repository contains my solution to the
["Build Your Own Shell" challenge](https://app.codecrafters.io/courses/shell/overview).

Built a Unix-like shell with composable internal and external commands, history,
and autocompletion.

Modelled command I/O as streams to support redirection, pipelines, and
asynchronous composition.

## What I have done

- Implemented a REPL shell loop in [`src/main.cs`](src/main.cs) that:
  - reads interactive input,
  - parses commands and pipelines,
  - executes builtins and external programs,
  - supports async output handling and process waiting.
- Added builtin commands:
  - `exit`, `echo`, `pwd`, `cd`, `type`, `history`.
- Added external command execution using `ProcessStartInfo` with redirected
  `stdin`, `stdout`, and `stderr`.
- Implemented redirection support:
  - stdout: `>`, `1>`, `>>`, `1>>`
  - stderr: `2>`, `2>>`
- Implemented pipeline support (`|`) by passing stream output from one command
  into the next command's stdin.
- Built parser logic for shell-like tokenization with:
  - single quote handling,
  - double quote handling,
  - backslash escaping rules.
- Added interactive line editing via a custom readline engine:
  - character input/backspace,
  - arrow-key history navigation,
  - tab completion.
- Built autocompletion that supports:
  - command + PATH executable completion for first token,
  - file/directory completion for path tokens,
  - longest-common-prefix completion,
  - double-tab candidate listing.
- Added history features:
  - load on startup (`HISTFILE` or `~/.bash_history`),
  - append on exit,
  - `history`, `history N`, `history -r`, `history -w`, `history -a`.

## Architecture choices

- Used a `ShellContext` object to carry state (`working directory`, `history`,
  `running processes`, and pipeline streams) through each command cycle.
- Normalized internal command output into streams (via
  `InternalCommand.CreateStream`) so builtin and external commands can be
  composed with the same redirection/pipeline flow.
- Used async stream copy tasks to avoid blocking and to handle multi-process
  output safely.
- Added defensive handling for common shell edge-cases like broken pipes and
  unknown commands.

## What I have learnt

- Stream-first design makes shell composition much simpler:
  if every command exposes output as a stream, redirection and pipelines become
  wiring problems instead of special-case logic.
- Process orchestration is mostly about lifecycle management:
  closing stdin at the right time, waiting for exit, and cleaning up streams is
  critical to avoid hangs.
- Shell parsing is deceptively tricky:
  quote/escape behavior needs an explicit state machine to stay predictable.
- Supporting `stderr` separately from `stdout` is essential for correct shell
  semantics and realistic command behavior.
- UX features (history navigation + autocomplete) significantly improve
  usability but require careful terminal redraw and key handling.
- Stateful features like history persistence are easier to reason about when
  tracked with explicit counters (`HistoryLoaded`, `HistoryAppended`) rather
  than inferred from file size or timestamps.

## Run locally

1. Ensure `.NET 9` is installed.
1. Run `./your_program.sh`.
