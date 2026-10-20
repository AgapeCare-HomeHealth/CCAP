# Referral PDF extraction — MRN / PCP / insurance safety

**Scope:** Only PDF field recognition and referral-intake mapping. No database migration or referral workflow rewrite.

## MRN

- Recognize only clearly labelled `MRN` / `Medical Record Number` values; return the first validated identifier token after the label (never the rest of the OCR row).
- Reject malformed values and candidate tokens containing too few digits.
- Deduplicate repeated identical identifiers across pages.
- Return each distinct candidate's page number and document section to the Web layer.
- Exactly one validated MRN: populate a previously empty MRN field.
- More than one validated MRN: **do not auto-select**. At Upload, a Care Coordinator must choose a candidate or explicitly choose manual entry and click **Confirm MRN review** before continuing.
- The MRN remains editable at Referral Information. No match: leave blank, not guessed. Facility-specific MRNs are not assumed to be interchangeable.

## Primary Care Physician (PCP)

- Only explicitly labelled PCP / Primary Care Physician / Primary Care Provider / Primary Physician text can be used.
- Require a plausible personal name. Reject signatures, signatures timestamps, provider addresses, phone numbers, pharmacy, health centers and facilities.
- If multiple distinct plausible names are present, do not auto-populate. Leave the existing field available for manual verification.
- A `Referring Physician` or `Ordering Physician` is **not automatically treated as the PCP**.

## Primary Insurance

- Reject OCR fragments that are numeric, too short, too long, or include other field labels.
- Even a plausible insurer name is auto-selected **only** if it matches an existing **active** Insurance lookup option, ignoring case and leading/trailing whitespace.
- No partial or fuzzy matching; no new insurance dropdown options are created by PDFs.

## Ordered Services

- Never auto-select, remove, or replace service checkboxes based on PDF text. The Care Coordinator chooses these explicitly.

## QA scenarios (synthetic data only)

1. `MRN: 12345 Gender: F DOB: 01/01/1950` -> one candidate, MRN `12345` (without neighbor text).
2. `MRN: 12345` on one page and `MRN: 98765432` on another -> two candidates; blocking review panel; no automatic MRN selection.
3. Repeated `MRN: 12345` on multiple pages -> one candidate, no conflict.
4. `MRN: Gender` or `Medical Record #: Signature` -> no MRN.
5. `PCP: Dr. Ellen Gray, MD` -> plausible PCP candidate; `PCP: County Health Center` / `Signature: Dr. Gray` -> blank.
6. Two conflicting PCP names -> blank, no silent selection.
7. Insurance text `73)` / `Subscriber ID:` -> blank; exact active lookup name -> selected; inactive/partial match -> blank.
8. Navigate Back/Next with unresolved MRN conflict, upload another PDF, and retry OCR errors. Verify the form never silently carries an unconfirmed candidate into MRN.
9. Upload a de-identified scanned multi-facility packet and confirm the MRN page numbers correspond to actual document pages.

## Verification status

The source and archive are statically checked. **Full .NET build, deployed OCR integration, and clinical user acceptance tests remain required**. The real patient PDF is not included in this package.
