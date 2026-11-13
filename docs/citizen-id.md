# CitizenId handling

`CitizenId` is the person's national identity number (for example, a Vietnamese citizen identification number). It is retained as sensitive profile data and is not an account credential.

At present:

- Registration accepts exactly 12 ASCII digits and stores the value as a string, preserving leading zeroes.
- The format check only checks the input shape. It does not validate the number with an authority, prove ownership, or verify the person's identity.
- The value is encrypted by the EF Core value converter using ASP.NET Core Data Protection before persistence and excluded from audit-trail value snapshots. No CitizenId encryption key is configured in `appsettings`.
- It is not used to sign in, issue tokens, or verify accounts. Email verification remains a separate account-verification step.
- It is not included in public profile responses.
- Duplicate-person matching and multi-account linking are not enabled yet. They require the trusted identity provider and server-side linking flow planned for later; a matching CitizenId alone will not be treated as proof that two registrations belong to the same person.

Any future identity-verification flow should be designed separately and must not treat a format-valid stored `CitizenId` as proof of identity.

## Data Protection keys

ASP.NET Core manages the encryption key ring outside application settings. Keep the key ring persistent across restarts and shared by every TaskMind instance that reads the same database; losing or changing the key ring makes previously protected CitizenIds unreadable. Container deployments must persist/mount the key-ring storage. The application name is fixed to `TaskMind` so the protection purpose remains stable across deployments.
