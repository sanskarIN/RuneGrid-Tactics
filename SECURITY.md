# Security Policy

RuneGrid Tactics is a client-side, local-first game. Security issues may still arise in dependency handling, import parsing, generated build output, cross-site behavior, or the local export/import boundary.

Please do **not** publish a suspected vulnerability in a public issue before it is assessed. Send a concise private report to **sanskarin@outlook.in** with the affected version, reproduction steps, potential impact, and any suggested mitigation. Avoid sending real personal data or credential material.

The project validates imported save records before replacing primary local state and keeps a rolling local backup. Nevertheless, players should keep their own export before importing records received from untrusted sources. Security fixes should include a regression test whenever the affected code is testable.
