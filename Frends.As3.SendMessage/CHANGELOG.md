# Changelog

## [1.1.0] - 2026-09-25

### Added

- Added `AllowInvalidCertificate` option to skip FTPS server certificate validation.
- Added `TrustedCertificateBase64` option to trust a specific server certificate, supplied as a base64-encoded string in DER or PEM format (Base64-encode the PEM text).

### Fixed

- Upgraded `nsoftware.IPWorksEDI` from 24.0.9618 to 24.0.9716. Build 24.0.9618 had a regression where only the first FTPS (explicit TLS) data connection in a process succeeded and every following execution failed with `Connection closed.`.

## [1.0.0] - 2026-08-04

### Added

- Initial implementation
