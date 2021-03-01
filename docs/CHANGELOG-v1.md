# Change log

See [upgrade notes][upgrade-notes] for helpful information when upgrading from previous versions.

[upgrade-notes]: upgrade-notes.md

## Unreleased

## v1.1.0

What's changed since v1.0.3:

- Engine features:
  - Added assertion helpers. [#640](https://github.com/microsoft/PSRule/issues/640)
    - Added `NotHasField` to check object does not have any of the specified fields.
    - Added `Null` to check field value is null.
    - Added `NotNull` to check field value is not null.
  - Added type assertion helpers. [#635](https://github.com/microsoft/PSRule/issues/635)
    - Added `IsNumeric` to check field value is a numeric types.
    - Added `IsInteger` to check field value is an integer types.
    - Added `IsBoolean` to check field value is a boolean.
    - Added `IsArray` to check field value is an array.
    - Added `IsString` to check field value is a string.
    - Added `TypeOf` to check field value is a specified type.
  - Added content helpers. [#637](https://github.com/microsoft/PSRule/issues/637)
    - Added `$PSRule.GetContentFirstOrDefault` to get content and return the first object.
    - Added `$PSRule.GetContentField` to get the field from content objects.
- General improvements:
  - Updated `HasJsonSchema` assertion helper. [#636](https://github.com/microsoft/PSRule/issues/636)
    - The URI scheme can optionally be ignored for `http://` or `https://` URIs.
    - The fragment `#` is ignored.
  - Added support for `-Outcome` and `-As` to produce filtered output from `Assert-PSRule`. [#643](https://github.com/microsoft/PSRule/issues/643)
    - Configure `Output.As` with `Summary` to produce summarized results per object.
    - Configure `Output.Outcome` to limit output to `Fail` or `Error`.

What's changed since pre-release v1.1.0-B2102029:

- No additional changes.

## v1.1.0-B2102029 (pre-release)

What's changed since pre-release v1.1.0-B2102024:

- General improvements:
  - Added support for `-Outcome` and `-As` to produce filtered output from `Assert-PSRule`. [#643](https://github.com/microsoft/PSRule/issues/643)
    - Configure `Output.As` with `Summary` to produce summarized results per object.
    - Configure `Output.Outcome` to limit output to `Fail` or `Error`.

## v1.1.0-B2102024 (pre-release)

What's changed since pre-release v1.1.0-B2102019:

- Engine features:
  - Added assertion helpers. [#640](https://github.com/microsoft/PSRule/issues/640)
    - Added `NotHasField` to check object does not have any of the specified fields.
    - Added `Null` to check field value is null.
    - Added `NotNull` to check field value is not null.

## v1.1.0-B2102019 (pre-release)

What's changed since v1.0.3:

- Engine features:
  - Added type assertion helpers. [#635](https://github.com/microsoft/PSRule/issues/635)
    - Added `IsNumeric` to check field value is a numeric types.
    - Added `IsInteger` to check field value is an integer types.
    - Added `IsBoolean` to check field value is a boolean.
    - Added `IsArray` to check field value is an array.
    - Added `IsString` to check field value is a string.
    - Added `TypeOf` to check field value is a specified type.
  - Added content helpers. [#637](https://github.com/microsoft/PSRule/issues/637)
    - Added `$PSRule.GetContentFirstOrDefault` to get content and return the first object.
    - Added `$PSRule.GetContentField` to get the field from content objects.
- General improvements:
  - Updated `HasJsonSchema` assertion helper. [#636](https://github.com/microsoft/PSRule/issues/636)
    - The URI scheme can optionally be ignored for `http://` or `https://` URIs.
    - The fragment `#` is ignored.

## v1.0.3

What's changed since v1.0.2:

- Bug fixes:
  - Fixed reason reported fields for `HasField` and `HasFields` assertion helpers. [#632](https://github.com/microsoft/PSRule/issues/632)

## v1.0.2

What's changed since v1.0.1:

- Engineering:
  - Bump Manatee.Json from 13.0.4 to 13.0.5. [#619](https://github.com/microsoft/PSRule/pull/619)
- Bug fixes:
  - Fixed `GetContent` processing of `InputFileInfo`. [#625](https://github.com/microsoft/PSRule/issues/625)
  - Fixed null reference of rule reason with wide output. [#626](https://github.com/microsoft/PSRule/issues/626)
  - Fixed markdown help handling of inline code blocks with `[`. [#627](https://github.com/microsoft/PSRule/issues/627)
  - Fixed markdown help inclusion of fenced code blocks in notes and description. [#628](https://github.com/microsoft/PSRule/issues/628)

## v1.0.1

What's changed since v1.0.0:

- Bug fixes:
  - Fixed module source key has already been added. [#608](https://github.com/microsoft/PSRule/issues/608)

## v1.0.0

What's changed since v0.22.0:

- General improvements:
  - Added rule help link in failed `Assert-PSRule` output. [#595](https://github.com/microsoft/PSRule/issues/595)
- Engineering:
  - **Breaking change**: Removed deprecated `$Rule` properties. [#495](https://github.com/microsoft/PSRule/pull/495)
  - Bump Manatee.Json from 13.0.3 to 13.0.4. [#591](https://github.com/microsoft/PSRule/pull/591)

What's changed since pre-release v1.0.0-B2011028:

- No additional changes.

## v1.0.0-B2011028 (pre-release)

What's changed since v0.22.0:

- General improvements:
  - Added rule help link in failed `Assert-PSRule` output. [#595](https://github.com/microsoft/PSRule/issues/595)
- Engineering:
  - **Breaking change**: Removed deprecated `$Rule` properties. [#495](https://github.com/microsoft/PSRule/pull/495)
  - Bump Manatee.Json from 13.0.3 to 13.0.4. [#591](https://github.com/microsoft/PSRule/pull/591)
