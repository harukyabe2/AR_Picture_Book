using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    [Header("UI Text References")]
    public TextMeshProUGUI uiText1; // 選択肢1
    public TextMeshProUGUI uiText2; // 選択肢2
    public TextMeshProUGUI uiText3; // ナレーション

    [Header("UI Effect References")]
    public Image glowImage1; // 選択肢1の背景画像
    public Image glowImage2; // 選択肢2の背景画像

    [Header("Effect Settings")]
    public float narrationInterval = 2.5f;
    public Color glowColor = Color.yellow; // 点滅時の色
    public float blinkInterval = 0.2f;
    public int blinkCount = 3;

    // 元の色を保存するための変数
    private Color originalGlowColor1;
    private Color originalGlowColor2;
    private bool colorsInitialized = false;

    // ★追加: 音声再生用のAudioSource
    [Header("Audio References")]
    public AudioSource audioSource;
    // ★追加: テキストと音声ファイルのパスをセットで管理するデータ構造
    private struct NarrationLine
    {
        public string Text;
        public string VoiceClipPath; // Resources/Voice/ フォルダからのパス
    }
    // ★この行を追加：実行中のコルーチンを保存しておくための変数
    private Coroutine narrationCoroutine;

    public enum UIScreenID
    {
        None, Scene1_Opening, Scene2a_SwordChoice, Scene2b_HeroineChoice,
        Scene3a_Alone, Scene3b_WithCompanion, Scene3c_Happy, Scene3d_Bad,
    }

    void Awake()
    {
        // 開始時はすべて非表示にする
        HideAllUI();
    }

    // --- SceneManagerから呼び出される公開メソッド ---

    // 指定されたIDの画面を表示する
    public void ShowScreen(UIScreenID screenId)
    {
        if (!colorsInitialized)
        {
            InitializeOriginalColors();
        }
        HideAllUI(); // 表示する前に一度すべてをリセット

        // ★ナレーション用のテキストオブジェクトは常に表示する
        if (uiText3 != null) uiText3.gameObject.SetActive(true);

        // ★変更: テキストのリストと音声パスのリストを両方取得する
        List<string> textLines = GetNarrationForScreen(screenId);
        List<string> voicePaths = GetVoiceClipPathsForScreen(screenId);

        // ★変更: 両方のリストをナレーション再生処理に渡す
        narrationCoroutine = StartCoroutine(ShowNarrationSequence(textLines, voicePaths));


        // ★選択肢や枠の表示は、必要なシーンの場合だけ実行する
        switch (screenId)
        {
            // --- 選択肢があるシーン ---
            case UIScreenID.Scene1_Opening:
            case UIScreenID.Scene2a_SwordChoice:
            case UIScreenID.Scene2b_HeroineChoice:
                // 選択肢UIのオブジェクトをすべて表示する
                if (uiText1 != null) uiText1.gameObject.SetActive(true);
                if (uiText2 != null) uiText2.gameObject.SetActive(true);
                if (glowImage1 != null) glowImage1.gameObject.SetActive(true);
                if (glowImage2 != null) glowImage2.gameObject.SetActive(true);

                // テキストを設定する
                uiText1.text = GetTextForChoice1(screenId);
                uiText2.text = GetTextForChoice2(screenId);
                break;

            // --- エンディングシーン (ナレーションのみ) ---
            case UIScreenID.Scene3a_Alone:
            case UIScreenID.Scene3b_WithCompanion:
            case UIScreenID.Scene3c_Happy:
            case UIScreenID.Scene3d_Bad:
                // 選択肢UIはHideAllUI()で非表示になったままなので、何もしない
                break;
        }
    }

    // すべてのUI要素を非表示にする
    public void HideAllUI()
    {
        StopAllCoroutines();

        // ★変更: テキストを空にするだけでなく、オブジェクト自体を非表示にする
        if (uiText1 != null) uiText1.gameObject.SetActive(false);
        if (uiText2 != null) uiText2.gameObject.SetActive(false);
        if (uiText3 != null) uiText3.gameObject.SetActive(false);
        if (glowImage1 != null) glowImage1.gameObject.SetActive(false);
        if (glowImage2 != null) glowImage2.gameObject.SetActive(false);

        // 色のリセット処理は残しておく
        if (colorsInitialized)
        {
            if (glowImage1 != null) glowImage1.color = originalGlowColor1;
            if (glowImage2 != null) glowImage2.color = originalGlowColor2;
        }
    }

    // --- 内部処理 ---

    private void InitializeOriginalColors()
    {
        if (glowImage1 != null) originalGlowColor1 = glowImage1.color;
        if (glowImage2 != null) originalGlowColor2 = glowImage2.color;
        colorsInitialized = true;
    }

    private void TriggerGlow(Image imageToGlow, Color originalColor)
    {
        StartCoroutine(GlowRoutine(imageToGlow, originalColor));
    }

    public void TriggerGlowEffect(bool isLeftChoice)
    {
        if (isLeftChoice)
        {
            if (glowImage1 != null) TriggerGlow(glowImage1, originalGlowColor1);
        }
        else
        {
            if (glowImage2 != null) TriggerGlow(glowImage2, originalGlowColor2);
        }
    }

    private IEnumerator GlowRoutine(Image imageToGlow, Color originalColor)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            imageToGlow.color = glowColor;
            yield return new WaitForSeconds(blinkInterval);
            imageToGlow.color = originalColor;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    // UIController.cs の ShowNarrationSequence メソッドを修正

    private IEnumerator ShowNarrationSequence(List<string> textLines, List<string> voiceClipPaths)
    {
        if (textLines == null || textLines.Count == 0) yield break;

        for (int i = 0; i < textLines.Count; i++)
        {
            // テキストリストからテキストを設定
            uiText3.text = textLines[i];

            // ★待機時間を計算するための変数を準備
            float waitTime = narrationInterval; // 音声がない場合のデフォルト待機時間

            // 音声パスのリストに対応する音声があるか確認
            if (i < voiceClipPaths.Count)
            {
                AudioClip clip = Resources.Load<AudioClip>(voiceClipPaths[i]);
                if (clip != null)
                {
                    audioSource.PlayOneShot(clip);
                    waitTime = clip.length; // ★待機時間を音声の長さに上書き
                }
            }

            // ★追加: 次の行がある場合のみ、計算した時間だけ待機する
            if (i < textLines.Count - 1)
            {
                yield return new WaitForSeconds(waitTime + 0.2f); // 音声の長さ + 少し余韻を持たせる
            }
        }
    }

    // --- テキストデータを返すメソッド群 ---

    private string GetTextForChoice1(UIScreenID screenId) // Left choice
    {
        switch (screenId)

        {
            case UIScreenID.Scene1_Opening: return "Draw the sword";
            case UIScreenID.Scene2a_SwordChoice: return "Alone";
            case UIScreenID.Scene2b_HeroineChoice: return "Save";
            default: return "";
        }
    }

    private string GetTextForChoice2(UIScreenID screenId) // Right choice
    {
        switch (screenId)
        {
            case UIScreenID.Scene1_Opening: return "Ignore the sword";
            case UIScreenID.Scene2a_SwordChoice: return "With companion";
            case UIScreenID.Scene2b_HeroineChoice: return "Run away";
            default: return "";
        }
    }

    private List<string> GetNarrationForScreen(UIScreenID screenId)
    {
        switch (screenId)
        {
            case UIScreenID.Scene1_Opening:
                return new List<string> {
        "This is a world threatened by the Demon King.",
        "Before a lone hero stands the legendary sword.",
        "Will he draw the blade and face the Demon King?",
        "He must make a choice—one that will shape the fate of all."
      };
            case UIScreenID.Scene2a_SwordChoice:
                return new List<string> {
        "The hero drew the sword and chose to fight.",
        "Ahead lies the battle that awaits in the Demon King's castle.",
        "Will he walk this path alone—",
        "or journey forward alongside companions?"
      };
            case UIScreenID.Scene2b_HeroineChoice:
                return new List<string> {
        "The hero chose not to draw the sword.",
        "He turned away from the legendary sword,",
        "After that, he came across a young girl attacked by mosnters.",
        "Should he help her, or run away?"
      };
            case UIScreenID.Scene3a_Alone:
                return new List<string> {
        "The hero set out alone to challenge the Demon King.",
        "Thanks to the power of the legendary sword,",
        "he finally reached the Demon King's throne.",
        "But before the overwhelming might of the Demon King,",
        "the hero stood no chance—and was defeated."
      };

            case UIScreenID.Scene3b_WithCompanion:
                return new List<string>
      {
        "Together with his companions, the hero overcame countless trials.",
        "At last, they reached the Demon King's stronghold.",
        "With the support of his allies," ,
        "the hero finally succeeded in defeating the Demon King.",
        "And so, peace returned to the world."
      };

            case UIScreenID.Scene3c_Happy:
                return new List<string>

      {
        "Summoning his courage, the hero finally defeated monsters",
        "The girl was saved and deeply grateful.",
        "In time, the hero and the girl became bound by love.",
        "They decided to live together,"
      };
            case UIScreenID.Scene3d_Bad:
                return new List<string>

      {
        "The hero couldn't find the courage and ran away.",
        "As a result, the girl was killed by the monsters.",
        "Consumed by hatred, she was reborn as a monster herself.",
        "In the end, the hero was killed by her hand."
      };
            default:
                return new List<string>();
        }

    }

    // ★音声ファイルのパスを返すメソッド
    private List<string> GetVoiceClipPathsForScreen(UIScreenID screenId)
    {
        switch (screenId)
        {
            case UIScreenID.Scene1_Opening:
                return new List<string> {
                    "Voice/Opening1",
                    "Voice/Opening2",
                    "Voice/Opening3",
                    "Voice/Opening4"
      };
            case UIScreenID.Scene2a_SwordChoice:
                return new List<string> {
                    "Voice/Sword1",
                    "Voice/Sword2",
                    "Voice/Sword3",
                    "Voice/Sword4"
      };
            case UIScreenID.Scene2b_HeroineChoice:
                return new List<string> {
                    "Voice/Heroine1",
                    "Voice/Heroine2",
                    "Voice/Heroine3",
                    "Voice/Heroine4"
      };
            case UIScreenID.Scene3a_Alone:
                return new List<string> {
                    "Voice/Alone1",
                    "Voice/Alone2",
                    "Voice/Alone3",
                    "Voice/Alone4",
                    "Voice/Alone5"
      };

            case UIScreenID.Scene3b_WithCompanion:
                return new List<string>
      {
                    "Voice/Friends1",
                    "Voice/Friends2",
                    "Voice/Friends3",
                    "Voice/Friends4",
                    "Voice/Friends5"
      };

            case UIScreenID.Scene3c_Happy:
                return new List<string>
      {
                    "Voice/Help1",
                    "Voice/Help2",
                    "Voice/Help3",
                    "Voice/Help4"
      };
            case UIScreenID.Scene3d_Bad:
                return new List<string>
      {
                    "Voice/NotHelp1",
                    "Voice/NotHelp2",
                    "Voice/NotHelp3",
                    "Voice/NotHelp4"
      };
            default:
                return new List<string>();
        }
    }
}