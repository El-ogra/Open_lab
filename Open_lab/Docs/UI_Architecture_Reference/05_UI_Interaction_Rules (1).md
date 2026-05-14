# 05 — UI Interaction Rules — Open lab system (Final Independent Audit)

> قواعد التفاعل الكاملة لكل شاشة. مأخوذة حرفياً من الكود + المرجعين `real lab system help.pdf` و `RLS_Learn.pdf`.

---

## 0) القواعد المعيارية الموحدة (مؤكَّدة من الكود)

### نقر واحد (Single-Click) — السلوك العام في WPF
- **الأزرار:** تنفّذ `Command` المرتبط فوراً عبر `ICommand.Execute`
- **RadioButton:** تحدث `IsChecked=true` ⇒ تنفِّذ Command + تشغّل Trigger BG=#4F7590 BorderBrush=#B7D6E8
- **CheckBox:** تبديل قيمة `IsChecked`
- **TextBox / PasswordBox:** التركيز ووضع المؤشر في موضع النقر
- **ContentControl:** لا استجابة (مجرد حاوية)

### نقر مزدوج (Double-Click)
- **لا يوجد أي ربط `MouseDoubleClick` في XAML الفعلي.** هذا الادعاء في الملف 05 القديم خاطئ.
- في PDFs المرجعية: ضغط مزدوج على اسم تحليل (في PatientTestsSelection) لإضافته (يصير سلوك مطلوب في التنفيذ المستقبلي للـ View).

### نقر يمين (Right-Click)
- **لا توجد أي ContextMenu في XAML الفعلي.** القائمة السياقية مفقودة كلياً من جميع الـ 14 ملف XAML.

### التركيز والاختيار (Selection/Focus)
- **افتراض WPF:** التركيز الأولي يقع على أول عنصر قابل للتركيز في Tab order
- **في LoginView:** بسبب `IsDefault=True` على زر «دخول»، الضغط على Enter في أي حقل ينفّذ `LoginCommand` تلقائياً
- **في باقي Hubs:** كل الأزرار لها `Cursor=Hand` (مؤشر يد عند المرور)
- **TextBox الـ ModernInputField عند التركيز:** Trigger IsFocused=True ⇒ BorderBrush=#106EBE, BorderThickness=2
- **TextBox عند Hover:** Trigger IsMouseOver=True ⇒ BorderBrush=#106EBE

### Hover Effects (Trigger IsMouseOver=True)
| العنصر | الـ Effect |
|---|---|
| LoginView TextBox / PasswordBox | BorderBrush=#106EBE |
| LoginView Button (LoginButtonStyle) | Background=#005A9E |
| MainWindow Toolbar RadioButton | Background=#244F70 |
| MainWindow Toolbar Exit Button | Background=#244F70 |
| كل Hub Button (ModuleButtonStyle) | Opacity=0.88 |
| PlaceholderView زر «رجوع» | (لا Trigger مخصص — السلوك الافتراضي) |

### Pressed Effects (Trigger IsPressed=True)
| العنصر | الـ Effect |
|---|---|
| LoginButton | Background=#004578 |
| MainWindow Toolbar Exit Button | Background=#4F7590 |
| كل Hub Button | Opacity=0.75 |

### Checked Effects (Trigger IsChecked=True — RadioButton فقط)
| العنصر | الـ Effect |
|---|---|
| MainWindow Toolbar RadioButton | Background=#4F7590, BorderBrush=#B7D6E8, FontWeight=Bold |

### Disabled Effects (Trigger IsEnabled=False)
| العنصر | الـ Effect |
|---|---|
| LoginButton | Background=#C8C8C8 |

---

## 0.1 LoginView — قواعد التفاعل

### التفاعلات الأساسية
| المُحفِّز | الشرط | النتيجة |
|---|---|---|
| Click في TextBox Username | — | تركيز + إظهار حدود زرقاء #106EBE |
| Click في PasswordBox | IsPasswordVisible=False | تركيز PasswordBox الآمن |
| Click في PasswordBox | IsPasswordVisible=True | يُخفى PasswordBox + يظهر TextBox عادي |
| Click على Eye Button | — | `TogglePasswordVisibilityCommand` ⇒ تبديل IsPasswordVisible + الأيقونة `&#xE723;` ⇄ `&#xE7B3;` + Foreground=#106EBE |
| Click على CheckBox «تذكر بياناتي» | — | RememberMe ⇄ true/false |
| Enter | في أي حقل | IsDefault=True ⇒ LoginCommand.Execute |
| Click على زر «دخول» | !IsBusy | LoginCommand.Execute |
| Click خارج النموذج | — | لا تأثير (Border بكامل النافذة) |

### دورة تنفيذ LoginCommand
1. تحقق فارغ: `if IsNullOrWhiteSpace(Username) || IsNullOrWhiteSpace(Password)` ⇒ StatusMessage = «يرجى إدخال اسم المستخدم وكلمة المرور.»
2. IsBusy = true (تعطيل الزر)
3. `await _authService.ValidateCredentialsAsync(Username, Password)`
   - if null ⇒ StatusMessage = «بيانات الدخول غير صحيحة.»
4. if isAdmin ⇒ `await _adminSetupService.EnsureAdminAccessAsync(user.UserId)`
5. `await _authorizationService.GetPermissionCodesAsync(user.UserId)` ⇒ AppSession.SetPermissions
6. AppSession.UserId / Username / IsAdmin set
7. if RememberMe ⇒ `_userPreferenceService.SetRememberedUsername(user.Username)` else null
8. `await _attendanceService.CreateLoginAsync(user.UserId, "تسجيل دخول")` ⇒ AppSession.AttendanceLogId
9. StatusMessage = «تم تسجيل الدخول بنجاح.» + _onLoginSuccess()
10. catch Exception ⇒ StatusMessage = «خطأ: {ex.Message}»
11. finally ⇒ IsBusy = false

### قواعد التحقق (Validation)
- **Username:** يجب ألا يكون فارغاً أو يحوي مسافات فقط
- **Password:** يجب ألا يكون فارغاً أو يحوي مسافات فقط
- **لا تحقق طول** (لا حد أدنى أو أقصى ظاهري)

### Tab Order (الترتيب التلقائي من XAML)
1. TextBox Username
2. PasswordBox (أو TextBox Password حسب IsPasswordVisible)
3. Eye Button
4. CheckBox RememberMe
5. Button «دخول»

### حالات Open Lab System الخاصة بـ LoginView
| الحالة | المؤشر | السلوك |
|---|---|---|
| Idle | IsBusy=False, StatusMessage=null | الزر «دخول» متاح |
| Validating | IsBusy=True | الزر «دخول» معطَّل (IsEnabled=False ⇒ Background=#C8C8C8) |
| Error | StatusMessage=«…خطأ…» | TextBlock بلون أحمر #D13438 |
| Success (لحظي) | StatusMessage=«تم تسجيل الدخول بنجاح.» | لحظة قبل إغلاق النافذة |
| RememberedUsername | Username مملوء + RememberMe=True | في constructor يتم تعبئة من `_userPreferenceService.GetRememberedUsername()` |

---

## 0.2 MainWindow — قواعد التفاعل

### Toolbar (Row 0) — 12 خانة
| الزر | الـ Command | السلوك |
|---|---|---|
| «المرضى» | NavigateToPatientsCommand | يضبط CurrentView إلى PatientModuleViewModel + ActiveModule="المرضى" |
| «أدوات» | NavigateToToolsCommand | … ToolsModuleViewModel + "أدوات" |
| «ورقة عمل» | NavigateToWorksheetCommand | … WorksheetModuleViewModel + "ورقة عمل" |
| «حسابات» | NavigateToAccountsCommand | … AccountsModuleViewModel + "حسابات" |
| «احصاليات» | NavigateToStatisticsCommand | … StatisticsModuleViewModel + "احصاليات" |
| «المستخدمين» | NavigateToUsersCommand | … UsersModuleViewModel + "المستخدمين" |
| «بيانات النظام» | NavigateToSystemDataCommand | … SystemDataModuleViewModel + "بيانات النظام" |
| «اعدادات» | NavigateToSettingsCommand | … SettingsModuleViewModel + "اعدادات" |
| «الموظفين» | NavigateToEmployeesCommand | NavigateTopModule("الموظفين") ⇒ CurrentView = new WelcomeViewModel("الموظفين") |
| «هل تعلم» | NavigateToDidYouKnowCommand | … "هل تعلم" |
| «نبذة» | NavigateToAboutCommand | … "نبذة" |
| «خروج» | LogoutCommand | LogoutAsync ⇒ CloseAttendance ⇒ _logoutRequested.Invoke ⇒ App.ReturnToLogin |

### قواعد إخفاء Toolbar
- **عند فتح PlaceholderView:** `OpenPlaceholder` يضبط `IsToolbarVisible = false` ⇒ Border Row 0 يصير Collapsed
- **عند الرجوع من PlaceholderView:** `ReturnToActiveModule` يضبط `IsToolbarVisible = true`
- **بعد Logout:** `IsToolbarVisible = true` (لأن MainWindow يُغلق بعد ذلك)

### الـ RadioButton GroupName="TopModules"
- ضمن نفس المجموعة، الضغط على واحدة يلغي الباقي تلقائياً
- لا توجد ميكانيكية لـ "checked initial" — كل الأزرار تبدأ unchecked عند فتح Window
- بعد ضغط زر علوي، يبقى مظللاً (Background=#4F7590) حتى الضغط على آخر

### CanExecute لكل Command
- جميع `Navigate*Command` لها CanExecute بناءً على permission code: `IsLoggedIn && AppSession.HasPermission(code)`
- AppSession.HasPermission: لو IsAdmin=true ⇒ يعيد دائماً true، وإلا يفحص في GrantedPermissions HashSet
- `LogoutCommand` ـ CanExecute = `IsLoggedIn`
- `NavigateDashboardCommand` ـ CanExecute = `IsLoggedIn` فقط (بدون permission)
- `NavigateTo*Module` (الـ 8 Hubs) و `NavigateToEmployees/DidYouKnow/About` ـ **بدون CanExecute** (متاحة دائماً)

### StatusBar Row 2
- TextBlock «متصل» مكتوب حرفياً كنص ثابت في XAML — **ليس Binding**. لا يعكس الحالة الفعلية للـ DB.
- `CurrentDate` يُضبط مرة واحدة في constructor + مرة في `InitializeAfterLogin`. لا يتحدث ذاتياً بدون إعادة تنفيذ ذلك.

---

## 0.3 WelcomeView — قواعد التفاعل

- **لا أي حقل قابل للتفاعل**
- مجرد Canvas وعناصر زخرفية
- ضغطة على أي مكان لا تنتج عنه أي حدث
- `ModuleName` في WelcomeViewModel غير مربوطة بأي عنصر في XAML — لذا فإن قيمة `new WelcomeViewModel("الموظفين")` (مثلاً) لن تُعرض

---

## 0.4 PlaceholderView — قواعد التفاعل

| المُحفِّز | السلوك |
|---|---|
| Click في أي مكان | لا تأثير (لا Triggers) |
| Click على زر «رجوع» | تنفيذ `BackCommand` ⇒ `MainViewModel.ReturnToActiveModule` ⇒ إعادة بناء Hub المناسب وفقاً لـ ActiveModule |
| Esc | لا ربط (KeyBinding مفقود) |
| Enter | لا ربط — زر «رجوع» ليس IsDefault |

### قواعد العنوان
- `FunctionTitle` يُمرَّر في constructor من `OpenPlaceholder(string functionTitle)` ⇒ يُعرض في TextBlock العنوان
- النص الثابت «نافذة مؤقتة للوظيفة، وسيتم استكمال محتواها لاحقاً.» مكتوب حرفياً (ليس Binding)

---

## 1.1 PatientModuleView — قواعد التفاعل

| الزر | الـ Command | السلوك |
|---|---|---|
| «اضافة وتعديل بيانات المرضى» | OpenAddPatientCommand | `_openPlaceholder("اضافة وتعديل بيانات المرضى")` ⇒ PlaceholderView |
| «ادخال نتائج التحاليل» | OpenEnterResultsCommand | `_openPlaceholder("ادخال نتائج التحاليل")` |
| «تسليم نتائج المرضى» | OpenDeliverResultsCommand | `_openPlaceholder("تسليم نتائج المرضى")` |
| «بحث عن مريض» | OpenSearchPatientCommand | `_openPlaceholder("بحث عن مريض")` |

كل ضغطة على زر ⇒ `MainViewModel.OpenPlaceholder` ⇒ `IsToolbarVisible=false` + `CurrentView=new PlaceholderView(...)`.

---

## 2.1 SystemDataModuleView — قواعد التفاعل
نفس النمط: كل زر ⇒ `_openPlaceholder(title)`. 14 وظيفة موصوفة في الملف 03.

## 3.1 AccountsModuleView / 4.1 WorksheetModuleView / 5.1 StatisticsModuleView / 6.1 SettingsModuleView / 7.1 ToolsModuleView / 8.1 UsersModuleView
نفس النمط: كل زر يفتح PlaceholderView. لا توجد:
- Tab switching داخل Hub
- شاشات frame داخلية
- Modal Dialog

---

## قواعد التفاعل المتوقعة للشاشات المفقودة (مأخوذة من PDFs)

### PatientRegistration (المرجع: real lab system help.pdf)
| المُحفِّز | الشرط | النتيجة |
|---|---|---|
| F1 | داخل النموذج | NewCommand (إنشاء سجل جديد) |
| F9 | داخل النموذج | SaveCommand (حفظ) |
| F10 | بعد حفظ | طباعة باركود |
| F11 | بعد حفظ | طباعة إيصال |
| F12 | بعد حفظ | DeleteCommand |
| Enter من LabId | الزر "تحميل" | LoadByLabIdCommand |
| Click على صف DataGrid Results | — | SelectedPatient ⇒ تلقائياً يُحمَّل بيانات المريض في النموذج (LoadFromPatientAsync) |
| تغيير في SelectedReferral | — | يحدّث ReferralId الجديد ولا يحفظ تلقائياً |

### PatientTestsSelection (المرجع: real lab system help.pdf)
| المُحفِّز | الشرط | النتيجة |
|---|---|---|
| ضغط مزدوج على Test من AvailableTests | VisitId>0 | إضافة تلقائية إلى SelectedTests |
| Enter في SearchText | — | فلترة FilteredAvailableTests |
| تغيير في AccountType | — | يعيد حساب الأسعار حسب Cash أو Referral |
| تغيير في SelectedReferral | AccountType=Referral | يعيد جلب الأسعار من PriceList المرتبطة |

### ResultsEntry (المرجع: RLS_Learn.pdf)
| المُحفِّز | الشرط | النتيجة |
|---|---|---|
| F4 | في MainWindow | فتح ResultsEntry |
| Enter داخل قيمة نتيجة | — | الانتقال للحقل التالي |
| F8 | نتيجة مختارة | Toggle Reviewed |
| F9 | نتيجة مختارة | SaveResultsCommand / Toggle Complete |
| F12 | بعد حفظ | معاينة طباعة |
| Click على VisitTest في القائمة | — | LoadResultsAsync لـ ResultItems |

### PatientBilling (المرجع: real lab system help.pdf)
| المُحفِّز | الشرط | النتيجة |
|---|---|---|
| Enter Enter (مرتين) | بعد إدخال المدفوع | تطبيق الحساب وحفظ (موافق) |
| Click على Payment في DataGrid | — | SelectedPayment يُحمَّل قيمته في EditPaymentAmount |
| تغيير Discount | VisitId>0 | إعادة حساب NetTotal و Balance تلقائياً |

### PatientSearch (المرجع: real lab system help.pdf + RLS_Learn.pdf)
| المُحفِّز | الشرط | النتيجة |
|---|---|---|
| F3 | في MainWindow | فتح PatientSearch |
| F5 | في PatientSearch | RefreshSearch |
| تغيير في Name / Phone / LabId | — | لا فلترة تلقائية، يجب الضغط على SearchCommand |
| Click على صف نتيجة | — | SelectedPatient + LoadVisitsAsync (يحمّل Visits للمريض) |

---

## قواعد سياق Open Lab System عبر كل الشاشات

### قواعد الـ Layout الديناميكي
- عند Login: ApplyLoginLayout ⇒ 400×550 NoResize
- بعد Login: ApplyAppLayout ⇒ 1100×700 CanResize
- يدوياً في MainWindow.xaml: Height=720 Width=1180 (تتجاوزها ApplyAppLayout)
- في PlaceholderView: IsToolbarVisible=false (Toolbar يختفي)

### قواعد المستخدم والصلاحيات
- AppSession.IsAdmin=true ⇒ كل HasPermission() يُرجع true بدون فحص HashSet
- PermissionCodes.FullAccess="ALL" ⇒ نفس الأثر (يُمنح كصلاحية واحدة)
- 22 صلاحية محددة في `Open_lab/Services/PermissionCodes.cs`

### قواعد الجلسة (AppSession)
- AppSession static — يبقى عبر كل ViewModels
- يُمسح عند Logout (AppSession.Clear)
- AttendanceLogId يُحفظ لإغلاق سجل الحضور عند Logout

### قواعد الأخطاء وعرضها
- كل ViewModel يستخدم `StatusMessage` كحقل دلالي للأخطاء
- في LoginView: TextBlock في Row 2 بلون #D13438
- باقي الـ ViewModels لا توجد لها View لذا StatusMessage لا تُعرض حالياً

### قواعد العمليات الـ async
- كل أوامر VM تستخدم `async _ => await SomethingAsync()` داخل RelayCommand
- لا يوجد IsBusy عام على مستوى التطبيق — كل VM يحدد IsBusy خاصة به (إن وُجدت)
- RaiseCanExecuteChanged يُستدعى يدوياً عند تغيير IsBusy

### قواعد الـ ObservableCollection
- كل الـ Collections في VMs هي ObservableCollection ⇒ تحديث تلقائي للـ Bindings عند Add/Remove
- Refresh في معظم VMs = Clear ثم Add في حلقة (لا InsertRange، لأن .NET ObservableCollection لا تدعمه افتراضياً)

---

## قواعد التفاعل المفقودة في كل الكود

### قواعد التحقق (Validation Rules) المفقودة
- لا IDataErrorInfo، لا INotifyDataErrorInfo
- لا ValidationRules في Bindings
- التحقق يتم برمجياً في Command Execute (مثل LoginAsync: `if IsNullOrWhiteSpace(...)`)

### قواعد التراجع (Undo/Redo) المفقودة
- لا توجد إطلاقاً

### قواعد التراجع عن العمليات (Confirmation Dialogs) المفقودة
- لا توجد MessageBox.Show في الكود (تحقق بـ grep يُرجع 0 نتائج خارج Services التي قد تستخدمها)
- لا توجد Custom Dialog Service

### قواعد الـ Drag & Drop المفقودة
- لا AllowDrop، لا Drop event handlers

### قواعد Tab Navigation الخاصة المفقودة
- لا TabIndex مُعرَّفة يدوياً ⇒ يعتمد على ترتيب XAML

---

## ملخص قواعد التفاعل الفعلية vs المُتوقَّعة

| النوع | في الكود الحالي | مُتوقَّع من PDF |
|---|:---:|:---:|
| Single-Click على Buttons | ✅ | ✅ |
| Hover Effects | ✅ | ✅ |
| RadioButton GroupName | ✅ | ✅ |
| Toggle PasswordVisibility | ✅ | غير محدد |
| KeyBinding (اختصارات F-Keys) | ❌ | ✅ مطلوب (F1-F12) |
| Enter ⇒ IsDefault (Login فقط) | ✅ | ✅ |
| Double-Click | ❌ | ✅ مطلوب (Tests Selection, DataGrid) |
| Context Menu | ❌ | ✅ مطلوب |
| Validation | ❌ Bindings | ⚠️ يدوي فقط |
| Confirmation Dialogs | ❌ | ✅ مطلوب (Delete, Logout) |
| Drag & Drop | ❌ | ❌ |
| Async Progress UI | ⚠️ IsBusy فقط | ✅ مطلوب |

---
