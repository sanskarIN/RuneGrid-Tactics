# Security Policy

RuneGrid Tactics is a native, local-first Godot game. Security-sensitive areas include C# dependencies, JSON import parsing, exported packages, local save integrity, Android signing, and platform credentials.

Do **not** publish a suspected vulnerability in a public issue before it is assessed. Send a private report to **sanskarin@outlook.in** with the affected version, reproduction steps, potential impact, and a suggested mitigation if available. Do not send real private save records, keystores, passwords, tokens, or personal data.

Imported local records are validated before replacement, and the application stores a rolling local backup. Players should still export their own record before importing an untrusted file. Keep `export_credentials.cfg`, Android keystores, signing keys, and CI secrets out of Git.
