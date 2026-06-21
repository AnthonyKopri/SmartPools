#if UNITY_EDITOR

using System.Collections.Generic;

namespace SmartPools.EnumMaps.Editor
{
    internal sealed class EnumMapEditorAnalysis
    {
        private readonly HashSet<int> _errorRows = new HashSet<int>();
        private readonly HashSet<int> _warningRows = new HashSet<int>();

        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();

        public bool HasIssues => Errors.Count > 0 || Warnings.Count > 0;
        public bool HasErrors => Errors.Count > 0;

        public bool RowHasError(int index)
        {
            return _errorRows.Contains(index);
        }

        public bool RowHasWarning(int index)
        {
            return _warningRows.Contains(index);
        }

        public void AddError(int rowIndex, string message)
        {
            if (rowIndex >= 0)
                _errorRows.Add(rowIndex);

            if (!Errors.Contains(message))
                Errors.Add(message);
        }

        public void AddWarning(int rowIndex, string message)
        {
            if (rowIndex >= 0)
                _warningRows.Add(rowIndex);

            if (!Warnings.Contains(message))
                Warnings.Add(message);
        }

        public string ToSummaryMessage()
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < Errors.Count; i++)
                lines.Add("Error: " + Errors[i]);

            for (int i = 0; i < Warnings.Count; i++)
                lines.Add("Warning: " + Warnings[i]);

            return string.Join("\n", lines);
        }
    }
}

#endif
