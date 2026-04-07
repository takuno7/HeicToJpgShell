using System;
using System.Collections.Generic;
using System.Globalization;

namespace HeicToJpgShell
{
    public static class L10n
    {
        private static bool IsKo = CultureInfo.CurrentUICulture.Name.StartsWith("ko", StringComparison.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            ["MenuText"] = "HEIC to JPG",
            ["FormTitle"] = "HEIC to JPG Converter",
            ["ColFileName"] = "File Name",
            ["ColStatus"] = "Status",
            ["ColTime"] = "Time",
            ["StatusWaiting"] = "Waiting...",
            ["StatusConverting"] = "Converting...",
            ["StatusSuccess"] = "Success",
            ["StatusFailed"] = "Failed",
            ["StatusSkipped"] = "Skipped",
            ["NoFilesMsg"] = "No HEIC files found.",
            ["TaskComplete"] = "Task Complete",
            ["SummaryBody"] = "All conversions finished.\n\n- Total: {0}\n- Success: {1}\n- Failure: {2}\n- Skipped: {3}\n- Duration: {4}s",
            ["ProgressLabel"] = "Progress: {0} / {1} (S: {2}, F: {3}, K: {4})",
            ["OverwriteTitle"] = "File Already Exists",
            ["OverwriteMsg"] = "The file '{0}' already exists.\nWould you like to overwrite it?",
            ["ApplyToAll"] = "Apply to all (Don't ask again)",
            ["Yes"] = "Yes",
            ["No"] = "No",
            ["Close"] = "Close",
            ["ErrorTitle"] = "Error",
            ["ErrorMsg"] = "Error occurred: {0}"
        };

        private static readonly Dictionary<string, string> Ko = new Dictionary<string, string>
        {
            ["MenuText"] = "HEIC to JPG 변환",
            ["FormTitle"] = "HEIC to JPG 변환기",
            ["ColFileName"] = "파일명",
            ["ColStatus"] = "상태",
            ["ColTime"] = "소요 시간",
            ["StatusWaiting"] = "대기 중...",
            ["StatusConverting"] = "변환 중...",
            ["StatusSuccess"] = "성공",
            ["StatusFailed"] = "실패",
            ["StatusSkipped"] = "건너뜀",
            ["NoFilesMsg"] = "변환할 HEIC 파일이 없습니다.",
            ["TaskComplete"] = "작업 완료",
            ["SummaryBody"] = "모든 변환 작업이 완료되었습니다.\n\n- 총 파일: {0}\n- 성공: {1}\n- 실패: {2}\n- 건너뜀: {3}\n- 총 소요 시간: {4}초",
            ["ProgressLabel"] = "진행 상태: {0} / {1} (성공: {2}, 실패: {3}, 건너뜀: {4})",
            ["OverwriteTitle"] = "파일 존재 확인",
            ["OverwriteMsg"] = "이미 '{0}' 파일이 존재합니다.\n덮어쓰시겠습니까?",
            ["ApplyToAll"] = "모든 파일에 적용(다시 묻지 않기)",
            ["Yes"] = "예",
            ["No"] = "아니오",
            ["Close"] = "닫기",
            ["ErrorTitle"] = "오류 발생",
            ["ErrorMsg"] = "오류 발생: {0}"
        };

        public static string Get(string key)
        {
            var dict = IsKo ? Ko : En;
            if (dict.ContainsKey(key)) return dict[key];
            return En.ContainsKey(key) ? En[key] : key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }
    }
}
