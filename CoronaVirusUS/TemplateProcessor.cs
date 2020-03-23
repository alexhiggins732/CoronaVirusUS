using System;

namespace CoronaVirusUS
{
    internal class TemplateProcessor
    {

        static string[] cvTagCandidates = TemplateProcessConstants.cvTagCandidates;

        private string rawText;
        const int MaxLength = 280;
        bool insertedNewLine = false;

        public TemplateProcessor(string text)
        {
            this.rawText = (text ?? "").Trim();
        }

        internal string GetProcessedText()
        {
            AssureCVTag();
            return outputText();
        }
        private string outputText() => rawText;

        bool contains(string match) => rawText.IndexOf(match, StringComparison.OrdinalIgnoreCase) > -1;

        bool processNext()
        {
            return rawText.Length < MaxLength;
        }

        bool replace(string tag, string candidate)
        {
            var idx = rawText.IndexOf(candidate, StringComparison.OrdinalIgnoreCase);
            if (idx > -1 && notAHashTag(idx))
            {
                var temp = $"{rawText.Substring(0, idx)}{tag}{rawText.Substring(idx + candidate.Length)}";
                rawText = temp;
                return true;
            }
            return false;
        }

        private bool notAHashTag(int idx)
        {
            var result = true;
            try
            {
                //var spaceIdx = (idx > 0 ? rawText.Substring(0, idx - 1) : rawText).LastIndexOf(' ');
                var spaceIdx = rawText.LastIndexOf(' ', idx - 1, idx);
                var tail = rawText.Substring(spaceIdx + 1);
                var c = rawText[spaceIdx + 1];
                result = rawText[spaceIdx + 1] != '#';

                if (result)
                {
                    var lnIdx = rawText.LastIndexOf('\n', idx - 1, idx);
                    result = rawText[lnIdx + 1] != '#';
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        void assureTag(string tag, string[] candidates)
        {

            if (!contains(tag))
            {
                foreach (var candidate in candidates)
                {
                    if (replace(tag, candidate))
                    {
                        return;
                    }
                }

                if (!insertedNewLine)
                {
                    if ((rawText.Length + "\n\n").Length < MaxLength)
                        rawText += "\n\n";
                    insertedNewLine = true;
                    if (rawText.Length + tag.Length + 1 < 280)
                        rawText += "" + tag;
                }
                else
                {
                    if (rawText.Length + tag.Length + 1 < 280)
                        rawText += " " + tag;
                }
            }

        }

        void AssureCVTag()
        {
            assureTag(TemplateProcessConstants.cvTag, TemplateProcessConstants.cvTagCandidates);
            if (processNext())
            {
                AssureCVIdTag();
            }
        }


        void AssureCVIdTag()
        {
            assureTag(TemplateProcessConstants.cvIdTag, TemplateProcessConstants.cvIdTagCandidates);
            if (processNext())
            {
                AssureCOTag();
            }
        }

        void AssureCOTag()
        {
            assureTag(TemplateProcessConstants.cOTag, TemplateProcessConstants.cOTagCandidates);
        }
    }
}