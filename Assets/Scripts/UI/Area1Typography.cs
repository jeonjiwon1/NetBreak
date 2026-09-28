using TMPro;
using UnityEngine;

// The Area 1 short-form UI uses Galmuri. Longer text keeps its serialized font.
internal static class Area1Typography
{
    internal enum Role
    {
        Body,
        Title,
        Number,
        Key
    }

    private static TMP_FontAsset regular;
    private static TMP_FontAsset bold;

    internal static void Apply(TMP_Text text, Role role)
    {
        if (text == null) return;

        TMP_FontAsset font = role == Role.Title || role == Role.Key
            ? Bold
            : Regular;
        if (font == null) return;

        text.font = font;
        text.fontSharedMaterial = font.material;
    }

    private static TMP_FontAsset Regular
    {
        get
        {
            if (regular == null)
                regular = Resources.Load<TMP_FontAsset>("UI/Fonts/Galmuri11 SDF");
            return regular;
        }
    }

    private static TMP_FontAsset Bold
    {
        get
        {
            if (bold == null)
                bold = Resources.Load<TMP_FontAsset>("UI/Fonts/Galmuri11 Bold SDF");
            return bold;
        }
    }
}
