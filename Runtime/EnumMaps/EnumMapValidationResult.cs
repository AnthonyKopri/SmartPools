using System.Collections.Generic;
using System.Text;

namespace SmartPools.EnumMaps
{
    public enum EnumMapIssueSeverity
    {
        Warning,
        Error
    }

    public sealed class EnumMapValidationIssue
    {
        public EnumMapIssueSeverity Severity { get; }
        public int EntryIndex { get; }
        public string KeyText { get; }
        public string Message { get; }

        public EnumMapValidationIssue(
            EnumMapIssueSeverity severity,
            int entryIndex,
            string keyText,
            string message)
        {
            Severity = severity;
            EntryIndex = entryIndex;
            KeyText = keyText;
            Message = message;
        }
    }

    public sealed class EnumMapValidationResult
    {
        private readonly List<EnumMapValidationIssue> _issues = new List<EnumMapValidationIssue>();

        public IReadOnlyList<EnumMapValidationIssue> Issues => _issues;

        public bool IsValid => !HasErrors;

        public bool HasErrors
        {
            get
            {
                for (int i = 0; i < _issues.Count; i++)
                {
                    if (_issues[i].Severity == EnumMapIssueSeverity.Error)
                        return true;
                }

                return false;
            }
        }

        public bool HasWarnings
        {
            get
            {
                for (int i = 0; i < _issues.Count; i++)
                {
                    if (_issues[i].Severity == EnumMapIssueSeverity.Warning)
                        return true;
                }

                return false;
            }
        }

        public void AddError(int entryIndex, string keyText, string message)
        {
            _issues.Add(new EnumMapValidationIssue(
                EnumMapIssueSeverity.Error,
                entryIndex,
                keyText,
                message));
        }

        public void AddWarning(int entryIndex, string keyText, string message)
        {
            _issues.Add(new EnumMapValidationIssue(
                EnumMapIssueSeverity.Warning,
                entryIndex,
                keyText,
                message));
        }

        public string ToMessage(string header = null)
        {
            StringBuilder builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(header))
                builder.AppendLine(header);

            for (int i = 0; i < _issues.Count; i++)
            {
                EnumMapValidationIssue issue = _issues[i];

                string prefix = issue.Severity == EnumMapIssueSeverity.Error
                    ? "Error"
                    : "Warning";

                if (issue.EntryIndex >= 0)
                {
                    builder.AppendLine(
                        $"{prefix} at entry {issue.EntryIndex}: {issue.Message}");
                }
                else
                {
                    builder.AppendLine($"{prefix}: {issue.Message}");
                }
            }

            return builder.ToString().TrimEnd();
        }
    }
}
