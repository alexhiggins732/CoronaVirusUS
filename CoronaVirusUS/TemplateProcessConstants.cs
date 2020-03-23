namespace CoronaVirusUS
{
    internal class TemplateProcessConstants
    {
        const string ht = "#";
        const string corona = nameof(corona);
        const string virus = nameof(virus);
        const string outbreak = nameof(outbreak);
        public static string cvTag = "#CoronaVirus"; 
        public static string[] cvTagCandidates = new[] { $"{corona}{virus}", $"{corona} {virus}", $"{virus}" };


        public const string cvIdTag = "#Covid19"; 
        public static string[] cvIdTagCandidates = new[] { "covid19", "covid-19", "virus" };

        public static string cOTag = "#CoronaVirusOutbreak"; 
        public static string[] cOTagCandidates = new[] { $"{corona} {virus} {outbreak}", $"{corona}{virus} {outbreak}", $"{virus} {outbreak}", $"{outbreak}" };
    }
}