namespace CoronaVirusUS
{
    public static class StringExentsions
    {
        public static int ParseAsInt(this string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            value = (value ?? "").Trim().TrimStart('+');
            if (string.IsNullOrEmpty(value)) value = "0";
            return int.Parse(value, System.Globalization.NumberStyles.AllowThousands);

        }
    }
}
