# HEIC to JPG Shell Extension (Windows)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Framework](https://img.shields.io/badge/.NET-4.8-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net48)

Windows 탐색기에서 우클릭만으로 다수의 HEIC 이미지를 고성능(병렬 처리)으로 JPG로 변환해주는 쉘 확장 프로그램입니다.

---

## 주요 기능
- 탐색기 우클릭 메뉴에서 바로 변환 가능
- 여러 파일 동시 변환 (최대 4개 스레드로 병렬 처리)
- 시스템 언어에 따라 한국어/영어 UI 자동 전환
- 중복 파일 발생 시 덮어쓸지 여부를 선택할 수 있고, "전체 적용" 옵션도 있음
- 진행 상황과 성공/실패/건너뜀 상태를 목록으로 확인 가능

---

## 사전 요구 사항

이 프로젝트를 빌드하고 실행하려면 다음이 필요합니다:

- **OS**: Windows 10/11 (64-bit 권장)
- **런타임**: [.NET Framework 4.8 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- **빌드 도구**: [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) (또는 [Build Tools for Visual Studio](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022))
  - **.NET 4.8 개발 도구** 및 **MSBuild**가 설치되어 있어야 합니다.

---

## 빌드 방법

종속성의 강력한 이름(Strong Name) 서명 처리가 필요하기 때문에 빌드 스크립트를 사용하는 걸 권장합니다.

1. 이 저장소를 로컬로 복제(Clone)합니다.
2. 터미널(PowerShell)을 열고 프로젝트 폴더로 이동합니다.
3. 빌드 스크립트를 실행합니다:
   ```powershell
   .\build.ps1
   ```
   *참고: 빌드 완료 시 `bin\Release\net48` 폴더에 결과물이 생성됩니다.*

---

## 설치 및 등록

빌드 후 생성된 폴더(`bin\Debug\net48` 또는 `bin\Release\net48`)로 이동한 다음, 해당 폴더 안에 있는 스크립트를 실행합니다.

등록 스크립트는 두 가지 방법으로 실행할 수 있습니다.

**방법 1: 배치 파일로 실행**

`register_extension.bat` 파일을 더블클릭하면 UAC 승격을 거쳐 자동으로 등록이 진행됩니다.

**방법 2: PowerShell에서 직접 실행**

관리자 권한으로 PowerShell을 열고, 출력 폴더로 이동한 후 실행합니다:
```powershell
cd bin\Release\net48
.\register_extension.ps1
```

등록이 완료되면 탐색기에서 `.heic` 파일을 우클릭해서 메뉴가 나타나는지 확인하면 됩니다.

메뉴가 바로 안 보이면 탐색기를 재시작하거나(`taskkill /f /im explorer.exe` 후 `start explorer.exe`), 그냥 로그아웃 후 다시 로그인해도 됩니다.

---

## 설치 제거

등록할 때와 마찬가지로 출력 폴더(`bin\Debug\net48` 또는 `bin\Release\net48`)에서 실행합니다.

**방법 1: 배치 파일로 실행**

`unregister_extension.bat` 파일을 더블클릭합니다.

**방법 2: PowerShell에서 직접 실행**

```powershell
cd bin\Release\net48
.\unregister_extension.ps1
```

---

## 사용법

1. 탐색기에서 변환할 `.heic` 파일을 하나 또는 여러 개 선택합니다.
2. 우클릭 후 **[HEIC to JPG]** 메뉴를 클릭합니다.
3. 진행 창이 열리면서 변환이 시작됩니다.
4. 완료 후 결과를 확인하고 [Close]를 눌러 닫습니다.

> **Windows 11 사용자 참고**: Windows 11에서는 우클릭 시 기본 메뉴에 [HEIC to JPG] 항목이 표시되지 않을 수 있습니다.
> 메뉴 하단의 **"추가 옵션 보기"** 를 클릭하거나 `Shift + F10`을 누르면 항목을 확인할 수 있습니다.

---

## 참고 사항

- 등록/해제 작업은 HKEY_CLASSES_ROOT 레지스트리를 수정하기 때문에 반드시 관리자 권한이 필요합니다.
- 스크립트는 반드시 DLL이 있는 폴더(빌드 출력 폴더) 안에서 실행해야 합니다. 다른 위치에서 실행하면 DLL을 찾지 못해 오류가 납니다.
- 쉘 확장은 격리된 환경에서 로드되기 때문에 종속성 로드 오류를 방지하는 리졸버 로직이 포함되어 있습니다.

---

## 사용한 라이브러리

- [SharpShell](https://github.com/dwmkerr/sharpshell): Windows Shell Extension 개발용 라이브러리
- [Openize.HEIC](https://github.com/openize/heic-dotnet): .NET용 HEIC 디코더
- [StrongNamer](https://github.com/dsplaisted/strongnamer): 종속성 Strong Name 자동 서명 도구

---

## 라이선스

MIT 라이선스로 자유롭게 사용할 수 있습니다.
