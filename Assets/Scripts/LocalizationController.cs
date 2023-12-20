using GamePush;
using Lean.Localization;
using UnityEngine;

public class LocalizationController : MonoBehaviour
{
    [SerializeField] LeanLocalization _local;

    private void Start()
    {
        var language = GP_Language.Current();
        // language = Language.Turkish;

        if (language == Language.English)
            _local.CurrentLanguage = "English";

        if (language == Language.Russian)
            _local.CurrentLanguage = "Russian";

        if (language == Language.Turkish)
            _local.CurrentLanguage = "Turkish";

        if (language == Language.Spanish)
            _local.CurrentLanguage = "Spanish";

        if (language == Language.German)
            _local.CurrentLanguage = "German";

        if (language == Language.Chineese)
            _local.CurrentLanguage = "English";

        if (language == Language.Korean)
            _local.CurrentLanguage = "English";

        if (language == Language.Japanese)
            _local.CurrentLanguage = "English";

        if (language == Language.Arab)
            _local.CurrentLanguage = "English";

        if (language == Language.Hindi)
            _local.CurrentLanguage = "English";

        if (language == Language.French)
            _local.CurrentLanguage = "English";

        if (language == Language.Italian)
            _local.CurrentLanguage = "English";

        if (language == Language.Italian)
            _local.CurrentLanguage = "English";

        if (language == Language.Portuguese)
            _local.CurrentLanguage = "English";

        if (language == Language.Indonesian)
            _local.CurrentLanguage = "English";

    }
}
