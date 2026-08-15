namespace SylviaNG.Prescription.Application.Common.StaticData
{
    /// <summary>
    /// US-043's known-phrase dictionary: English phrase (trimmed/lowercased key) → Bangla
    /// translation. Deliberately a small hardcoded list, never a live translation API call —
    /// feature.md §13 explicitly scopes bilingual support to "auto-fill from a known-phrase
    /// dictionary," not machine translation. Shares its literal English/Bangla pairs with
    /// QuickAddPresetSeeder's Advice/FollowUp starters (one source list, not two that could
    /// drift). GetAdviceFollowUpPhraseDictionaryHandler merges this with the calling doctor's
    /// own previously-saved Advice/FollowUp presets, whose pairs take precedence.
    /// </summary>
    public static class AdviceFollowUpPhraseDictionary
    {
        public static readonly IReadOnlyDictionary<string, string> Entries = new Dictionary<string, string>
        {
            ["drink plenty of fluids."] = "প্রচুর পরিমাণে তরল পান করুন।",
            ["take adequate rest."] = "পর্যাপ্ত বিশ্রাম নিন।",
            ["avoid oily and spicy food."] = "তৈলাক্ত ও মশলাযুক্ত খাবার এড়িয়ে চলুন।",
            ["review after 7 days."] = "৭ দিন পর পুনরায় দেখাবেন।",
            ["follow up if symptoms don't improve."] = "উপসর্গ না কমলে পুনরায় দেখাবেন।",
        };
    }
}
