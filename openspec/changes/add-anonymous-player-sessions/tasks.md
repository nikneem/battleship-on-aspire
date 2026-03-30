## 1. Infrastructure setup

- [x] 1.1 Add Aspire AppHost configuration for a Dapr state store used by the API
- [x] 1.2 Add API configuration for JWT signing, token lifetime, and any Dapr client dependencies needed for anonymous sessions

## 2. Profiles module implementation

- [x] 2.1 Define Profiles abstractions for anonymous player session requests, responses, and persisted player records
- [x] 2.2 Implement Profiles services for creating temporary players in the Dapr state store with a one-hour TTL
- [x] 2.3 Implement Profiles services for validating persisted temporary players and renewing JWTs for active sessions

## 3. API integration

- [x] 3.1 Configure ASP.NET Core JWT bearer authentication in the API host
- [x] 3.2 Add an endpoint to create an anonymous player session from a submitted player name
- [x] 3.3 Add an authenticated endpoint to renew an anonymous player JWT near expiration

## 4. Verification

- [x] 4.1 Add automated tests for anonymous session creation, JWT claim contents, and renewal behavior
- [x] 4.2 Validate the OpenSpec change and run the relevant repository tests after implementation
