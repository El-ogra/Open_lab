# UI Navigation Specification
# Project: Open Lab System | WPF .NET 8 | MVVM
# Reference: Real Lab System (screenshots analysis)
# Status: Phase 1 — Main Navigation + Patient Module

---

## 1. STARTUP BEHAVIOR

### On Application Launch:
- The FIRST window to appear is: LoginView
- LoginView appears as a standalone centered Window
- No main window is visible at this point
- LoginView is NOT embedded inside any other window

### LoginView Contents:
- Title: "تسجيل الدخول" (large, bold, blue)
- Subtitle: "Open Lab System" (smaller, blue)
- Field: "اسم المستخدم" (username text input)
- Field: "كلمة المرور" (password input with show/hide eye icon)
- Checkbox: "تذكر بياناتي"
- Button: "دخول" (large, blue, full width)
- Layout direction: RTL (Right to Left)

### After Successful Login:
- LoginView closes and DISAPPEARS completely
- MainWindow opens and takes its place
- MainWindow shows the top navigation toolbar
- MainWindow shows the welcome content in the central area

### After Failed Login:
- LoginView remains open
- An error message appears inside the LoginView
- No navigation occurs

---

## 2. MAIN WINDOW STRUCTURE

### Layout: Three Horizontal Zones (Top to Bottom)

```
┌────────────────────────────────────────────────────┐
│                  TOP TOOLBAR                        │  ← Zone 1: Main Navigation
├────────────────────────────────────────────────────┤
│                                                    │
│               CENTRAL CONTENT AREA                 │  ← Zone 2: Dynamic Content
│                                                    │
├────────────────────────────────────────────────────┤
│                  STATUS BAR                         │  ← Zone 3: Status Info
└────────────────────────────────────────────────────┘
```

### IMPORTANT: Navigation Style
The reference system uses a HORIZONTAL TOP TOOLBAR,
NOT a vertical sidebar.
Main functions appear as icon+text buttons in the top toolbar.
This is the layout Open Lab System must replicate.

---

## 3. TOP TOOLBAR — MAIN FUNCTIONS

### Toolbar Direction: RTL (Right to Left)
### Style: Icon above text, each item is a clickable button

Items in order from RIGHT to LEFT:

| Position | Arabic Name       | English Name         | Icon Type        |
|----------|-------------------|----------------------|------------------|
| 1 (R)    | المرضى            | Patients             | Patient/Medical  |
| 2        | أدوات             | Tools                | Tools/Wrench     |
| 3        | ورقة عمل          | Worksheet            | Paper/Pencil     |
| 4        | حسابات            | Accounts/Finance     | Money/Chart      |
| 5        | احصاليات          | Statistics           | Bar Chart        |
| 6        | المستخدمين        | Users                | Person/Lock      |
| 7        | بيانات النظام     | System Data          | Database         |
| 8        | اعدادات           | Settings             | Gear/Wrench      |
| 9        | الموظفين          | Employees/HR         | Worker/Person    |
| 10       | هل تعلم           | Did You Know         | Info/Lightbulb   |
| 11       | نبذة              | About                | Leaf/Info        |
| 12 (L)   | خروج              | Exit/Logout          | Door/Arrow       |

### Toolbar Visual Style:
- Background: Dark navy/steel blue gradient
- Each button: Icon on top, Arabic text label below
- Active/selected button: highlighted (lighter or different color)
- Hover effect: slight color change
- Fixed height: approximately 80-90px

---

## 4. CENTRAL CONTENT AREA — DEFAULT STATE

### Shown When: No main function is selected (just after login)
### Content:
- White/light background panel
- Center logo area with decorative circles
- Three circles with text: "Open" | "Lab" | "Sys"
  (Reference uses "real" | "lab" | "sys" — replace with Open Lab branding)
- Optional: "MS SQL SERVER" text label in top-left of panel
- Optional: Company logo in bottom-right corner

---

## 5. NAVIGATION BEHAVIOR — STEP BY STEP

### Step 1: User clicks a main function in top toolbar
- The clicked toolbar button becomes visually ACTIVE (highlighted)
- The central content area CLEARS the default welcome content
- The central content area SHOWS the sub-function panel for this module
- NO new window opens at this stage
- The top toolbar remains fully visible

### Step 2: Sub-function panel appears in central area
- Panel has a description/info section at the top (text + illustration)
- Below the info section: large colored buttons for each sub-function
- Buttons are arranged in a grid (2 columns typical)
- Each button has: Arabic text label + icon on the right side
- Button color in reference: Yellow-green (#9DC718 approximate)

### Step 3: User clicks a sub-function button
- The sub-function window/view opens
- Behavior options (to be confirmed per function):
  Option A: Opens as a new child Window over MainWindow
  Option B: Replaces the central content area entirely
  Option C: Opens as a modal dialog
  NOTE: Reference system appears to use Option A or B — 
  to be specified per module as screenshots are reviewed

### Step 4: User returns from sub-function
- Sub-function closes
- MainWindow returns to the sub-function panel of the last selected module
  OR returns to the default welcome content
  (to be confirmed from screenshots)

---

## 6. STATUS BAR — BOTTOM OF MAIN WINDOW

### Always visible, never hidden
### Content (Left to Right in reference, RTL layout):
- "User Name" label + logged-in username value
- "Last Login" label + date and time value
- "SQL DATABASE" label (database connection indicator)
- Phone icon + number (optional)
- "Today" label + current date value

### For Open Lab System:
- Replace any "real lab system" branding with "Open Lab System"
- Same status bar structure applies

---

## 7. PATIENT MODULE — SUB-FUNCTIONS DETAIL
## (First Module: المرضى)

### Activation: Click "المرضى" in top toolbar

### Info Panel (top of central area):
- Illustration: Medical/patient image (doctor or patient graphic)
- Description text (RTL, right-aligned):
  "من خلال هذه النافذة يمكنك عمل الآتي:"
  - "إضافة مريض جديد وطباعة الباركود والإيصال الخاص به وإدخال النتائج وطباعتها"
  - "البحث عن مريض محدد أو عرض مرضى فترة معينة ومعرفة النتائج الغير منتهية"
  - "وكذلك الغير محققة والغير مطبوعة"
  - "تسليم نتائج المرضى وتصفية الحسابات الخاصة بهم وحصر النتائج الغير مسلمة"

### Sub-Function Buttons (2×2 grid):

| Position    | Arabic Label                  | English Reference Name          |
|-------------|-------------------------------|---------------------------------|
| Top-Right   | اضافة وتعديل بيانات المرضى   | Add & Edit Patient Data         |
| Top-Left    | ادخال نتائج التحاليل          | Enter Test Results              |
| Bottom-Right| تسليم نتائج المرضى            | Deliver Patient Results         |
| Bottom-Left | بحث عن مريض                   | Search for Patient              |

### Button Style:
- Background: Yellow-green color (#8DB600 approximate)
- Text: White or dark, Arabic, right-aligned
- Icon: On the right side of each button
- Size: Large, approximately equal width and height
- Corner radius: Slightly rounded

---

## 8. REMAINING MODULES — TO BE SPECIFIED

The following modules need screenshot review before specification:
Sub-function details will be added here progressively.

| # | Module Arabic Name | Status              |
|---|--------------------|---------------------|
| 1 | المرضى             | ✅ Specified above  |
| 2 | أدوات              | 🔄 Pending screenshots |
| 3 | ورقة عمل           | 🔄 Pending screenshots |
| 4 | حسابات             | 🔄 Pending screenshots |
| 5 | احصاليات           | 🔄 Pending screenshots |
| 6 | المستخدمين         | 🔄 Pending screenshots |
| 7 | بيانات النظام      | 🔄 Pending screenshots |
| 8 | اعدادات            | 🔄 Pending screenshots |
| 9 | الموظفين           | 🔄 Pending screenshots |
| 10| هل تعلم            | 🔄 Pending screenshots |
| 11| نبذة               | 🔄 Pending screenshots |
| 12| خروج               | ✅ Logout — no sub-functions |

---

## 9. BRANDING & IDENTITY

| Element           | Reference System        | Open Lab System         |
|-------------------|-------------------------|-------------------------|
| System Name       | Real Lab System         | Open Lab System         |
| Window Title      | real lab system >> [القائمة الرئيسية] | Open Lab System >> [القائمة الرئيسية] |
| Logo Circles Text | real / lab / sys        | Open / Lab / Sys        |
| Login Subtitle    | —                       | Open Lab System         |
| Color Theme       | Dark navy + green       | To be decided           |

---

## 10. IMMEDIATE PRIORITY — CRITICAL FIX NEEDED

### Problem Identified (Image 1):
When the project runs, it shows a blank white window displaying:
"Open_lab.ViewModels.LoginViewModel"

This means the DataContext is set correctly but no DataTemplate
or View is rendered. The LoginView.xaml exists and looks correct
(Image 2) but is not being displayed.

### Root Cause:
After dependency isolation was performed, the wiring between
LoginViewModel and LoginView was broken.
The application is showing the ViewModel's ToString() value
instead of rendering the View.

### Fix Required:
Restore the connection between LoginView and LoginViewModel
so that when the application starts, LoginView.xaml renders
correctly as the startup window.
This is the FIRST thing that must be fixed before any other
UI work proceeds.