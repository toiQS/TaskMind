# Project resources

The project repository stores revision metadata and references to externally stored objects. TaskMind does not store file bytes.

## API

All routes are under `/api/v1`:

- `POST projects/{projectId}/repository` creates the project's single repository. The storage provider is currently `GoogleDrive`; `StorageRootRef` must be a non-secret provider object reference.
- `GET projects/{projectId}/repository` returns repository metadata and the current Dev, Test, and Product revision pointers.
- `GET projects/{projectId}/repository/revisions?page=1&pageSize=20` lists revision snapshots and review records.
- `POST projects/{projectId}/repository/workspaces` registers a caller-owned device in one environment.
- `POST projects/{projectId}/repository/workspaces/{workspaceId}/pull` returns the current manifest references for that environment.
- `POST projects/{projectId}/repository/revisions` pushes a Dev snapshot. Each file entry supplies a relative path, external `StorageObjectRef`, SHA-256 content hash, and size. The server computes the manifest hash.
- `POST projects/{projectId}/repository/revisions/{revisionId}/reviews` records an immutable review.
- `POST projects/{projectId}/repository/promote` promotes the current Dev revision to Test or current Test revision to Product, subject to role and review requirements.
- `GET projects/{projectId}/repository/promotions?page=1&pageSize=20` lists promotion history.
- `DELETE projects/{projectId}/repository/workspaces/{workspaceId}` revokes a workspace. Leaving/removing a member, deactivating an account, or completing/cancelling a project also revokes affected workspaces.
- `POST admin/projects/{projectId}/repository/revisions/{revisionId}/redact` hides the revision's file references in TaskMind and records an audit log. It does not delete objects from external storage.

Repository access requires an active project member or project manager/owner. Push is restricted to an active Dev workspace. A stale base revision is rejected, including concurrent pushes. Revisions and their file entries are immutable; a rejected review requires a new revision.

## Storage and database setup

Provider credentials, file transfer, external permission management, and deletion are not integrated. Clients must upload/manage external objects separately and provide stable, immutable object references; the API does not dereference submitted references. The stored SHA-256 hash can be used by the storage client to verify downloaded bytes.

No migration was generated. Before deploying this code, update the existing database schema manually for `ProjectRepositories`, `ProjectEnvironments`, `ProjectRevisions`, `RevisionFileEntries`, `RevisionReviews`, `LocalProjectWorkspaces`, and `RevisionPromotions`, including their foreign keys, unique indexes, and the concurrency check on `ProjectEnvironments.CurrentRevisionId`.
