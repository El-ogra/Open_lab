# Patient Module — Product Requirements Document
## Open Lab Laboratory Information System

### Overview
Desktop WPF application (.NET 8) for managing laboratory patients.
Architecture: MVVM. Database: SQL Server via Entity Framework Core 8.

### Module: Patient Registration (PatientRegistrationView)

#### Feature 1 — Add New Patient
- User fills: FullName (required), Gender (required), Age, Phone
- System auto-generates unique LabId
- System saves patient to database
- Validation: FullName and Gender are mandatory
- Success: patient appears in patient list

#### Feature 2 — Assign Lab Tests to Patient
- Available tests list loads from database (Tests table)
- User selects tests and adds them to patient's visit
- Each selected test creates a VisitTest record
- Price is calculated and displayed per test
- Total amount is shown

#### Feature 3 — Barcode Printing
- After saving patient, user can print barcode
- Barcode contains patient LabId
- Print dialog opens for barcode label

#### Feature 4 — Receipt Printing
- System generates receipt for patient visit
- Receipt shows: patient name, LabId, tests ordered, total price
- Print dialog opens

#### Feature 5 — Work Sheet Printing
- System generates worksheet for lab technician
- Shows patient info and ordered tests
- Print dialog opens

#### Feature 6 — Enter Test Results
- User selects a patient visit
- For single-component tests: enters one value
- For multi-component tests (CBC, Urine): enters value per parameter
- System compares result against reference ranges
- System flags result as High/Low/Normal

#### Feature 7 — Search Patient
- User searches by: LabId, Name, Phone, or NationalId
- Results displayed in list
- User can select patient to view or edit

### Business Rules
- LabId must be unique across all patients
- Gender values: Male or Female
- Test results flagged L=Low, H=High, N=Normal
- Reference ranges vary by patient gender and age