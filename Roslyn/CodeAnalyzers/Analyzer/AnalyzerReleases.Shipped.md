## Release 1.0
### New Rules
Rule ID    | Category      | Severity | Notes
-----------|---------------|----------|--------------------
SAMDATA01  | Naming        | Warning  | A class inheriting from EntityBase end with Entity
SAMDATA02  | Design        | Warning  | A class inheriting from EntityBase Must have a parameter less constructor 
SAMWEB01   | Naming        | Warning  | A class inheriting from BaseViewModel end with ViewModel
SAMTEST01  | Usage         | Warning  | A class inheriting from BaseTestClass should have a ExcludeFromCodeCoverage attribute