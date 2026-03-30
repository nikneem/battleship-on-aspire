## Context

The repository currently contains an empty `Profiles` module and a minimal API host without authentication, Dapr, or player session behavior. This change introduces temporary anonymous player identity across multiple projects: the API host, the Profiles module, and the Aspire AppHost that must provide a Dapr state store for short-lived session persistence.

The desired identity model is intentionally ephemeral. A browser visit creates a temporary player record from a submitted name, receives a JWT, and renews that JWT while the session remains active. A later browser visit may reuse the stored name to create a brand-new player record and JWT, even if the old record has already expired from the state store.

## Goals / Non-Goals

**Goals:**
- Create anonymous player sessions without integrating an external identity provider.
- Persist temporary player records in Dapr state with a one-hour lifetime.
- Encode the temporary player identifier and player name into JWT claims for authenticated requests.
- Support token renewal before expiry so active browser sessions continue without prompting for the player name again.
- Keep the design simple enough to use as the first identity boundary for gameplay APIs.

**Non-Goals:**
- Providing durable player identity across browsers or long periods of inactivity.
- Supporting account recovery, multi-device sign-in, or name ownership guarantees.
- Implementing player rename flows in this change.
- Defining gameplay authorization rules beyond requiring a valid anonymous player token.

## Decisions

### Use ephemeral player records in Dapr state

The server will create a new player record whenever the client starts a session, whether this is the first visit or a later visit using the locally stored name. Each record will receive a new player identifier and be written to the Dapr state store with a one-hour TTL.

This matches the desired product behavior: player identity is temporary and intentionally disposable. Using Dapr state avoids introducing a database-specific persistence layer in the first iteration and keeps expiry aligned with the state store itself.

**Alternatives considered:**
- Reissuing tokens from a stable persisted player record keyed by name or a local secret. Rejected because the intended behavior explicitly allows a new player identity on later visits.
- Avoiding persistence entirely and relying only on JWT claims. Rejected because the server still needs a short-lived source of truth for temporary player metadata and future gameplay lookups.

### Use JWT bearer authentication with player identifier and name claims

The API will issue short-lived JWT access tokens for anonymous sessions. Tokens will carry at least a subject claim for the temporary player identifier and a name claim for the submitted player name. Gameplay endpoints can trust the token as the authenticated identity boundary and use the claims to load any needed session data.

This keeps authentication standard for ASP.NET Core and makes it easy for later modules to authorize requests using normal bearer token middleware.

**Alternatives considered:**
- Using opaque server session IDs in cookies. Rejected because the desired client flow already assumes JWT-based requests and renewal.
- Including only the player identifier in the token. Rejected because the requested behavior explicitly wants both identifier and player name included.

### Provide a dedicated token renewal endpoint

The server will expose a renewal path that accepts the current valid token and returns a replacement token when the current token is nearing expiration. Renewal will keep the same temporary player identifier as long as the backing Dapr record still exists and remains within its TTL.

This separates initial player creation from session continuation. It also allows the client to auto-renew without re-sending the player name during an active visit.

**Alternatives considered:**
- Forcing the client to create a new player whenever a token expires. Rejected because it would rotate identity mid-session and break continuity for active gameplay.
- Returning very long-lived JWTs. Rejected because shorter token lifetimes reduce the impact of token leakage and better match temporary sessions.

### Configure Aspire to supply the Dapr state store dependency

The Aspire AppHost will define the state store resource needed by the API, and the API will use the Dapr client or HTTP API to persist and retrieve temporary player session records. JWT signing configuration will remain in the API host configuration.

This keeps infrastructure wiring in the AppHost where the rest of the distributed application is expected to be described.

**Alternatives considered:**
- Hard-coding direct state store access without Aspire wiring. Rejected because it would bypass the project’s orchestration boundary.

## Risks / Trade-offs

- [Name collisions between players] -> Allow duplicate names and treat them as presentation-only data rather than unique identity.
- [Expired Dapr records during renewal] -> Reject renewal once the record is missing and require the client to create a new anonymous session from the stored name.
- [Player identity changes between visits] -> Document that identities are visit-scoped and avoid binding long-lived external concepts to the temporary player identifier.
- [JWT signing key misconfiguration] -> Require explicit configuration for signing credentials and fail startup or request processing clearly when missing.

## Migration Plan

1. Add Aspire wiring for a Dapr state store and make the API aware of the state store dependency.
2. Implement the Profiles contracts and server-side services for creating, loading, and renewing anonymous player sessions.
3. Configure JWT bearer authentication in the API and expose anonymous session creation and renewal endpoints.
4. Update downstream gameplay endpoints to read player identity from JWT claims once those endpoints exist.

Rollback consists of removing the new endpoints and authentication middleware, then disabling the Dapr state store resource from the AppHost.

## Open Questions

- What JWT lifetime should be used relative to the one-hour Dapr TTL?
- Should renewal extend the Dapr record TTL, or should the underlying session always expire one hour after creation regardless of renewals?
- What exact API surface should be exposed for the client: a single creation endpoint plus renewal endpoint, or a combined bootstrap endpoint that behaves differently based on authentication state?
