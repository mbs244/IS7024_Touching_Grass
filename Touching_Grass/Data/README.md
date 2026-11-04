# ZIP Location Dataset

`zip-locations.json` is the local source for USA ZIP-code resolution. The production
file MUST contain every supported five-digit USA ZIP code with its ZIP code, latitude,
longitude, and IANA timezone identifier.

The checked-in JSON currently contains representative records for development and tests.
Replace or regenerate it from the approved nationwide ZIP dataset before enabling
nationwide production coverage. The application intentionally does not fabricate
coordinates for ZIP codes absent from this file.
