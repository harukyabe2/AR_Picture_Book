using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.XR;
using System.IO;
using System.Threading;
using static UnityEditor.PlayerSettings;
using static UnityEngine.ParticleSystem;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEditor.PackageManager;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI uiText1;
    public TextMeshProUGUI uiText2;
    public TextMeshProUGUI uiText3; // 地の文用
    public float narrationInterval = 2.5f;

    public Image glowImage1; // 外枠用Image（UI上の枠など）
    public Image glowImage2;

    public Color emissionOnColor = Color.yellow;
    public Color emissionOffColor = Color.black;
    public float blinkInterval = 0.2f;
    public int blinkCount = 3;


    private Material glowMat1;
    private Material glowMat2;

    void Start()
    {
        // 複製マテリアルで他UIと共有しないようにする
        glowMat1 = Instantiate(glowImage1.material);
        glowMat2 = Instantiate(glowImage2.material);
        glowImage1.material = glowMat1;
        glowImage2.material = glowMat2;

        // 初期は発光オフ
        glowMat1.SetColor("_EmissionColor", emissionOffColor);
        glowMat2.SetColor("_EmissionColor", emissionOffColor);
        StartCoroutine(ShowNarrationSequenceOpening(GetTextForState3(VoiceCommand.State.Opening)));
    }

    public void PlayEffectAndUpdateText(VoiceCommand.State newState)
    {
        switch(newState){
            case VoiceCommand.State.Opening:
                TriggerGlow(glowMat1);
                break;
            case VoiceCommand.State.Sword:
                TriggerGlow(glowMat1);
                break;
            case VoiceCommand.State.Heroine:
                TriggerGlow(glowMat2);
                break;
            case VoiceCommand.State.Alone:
                TriggerGlow(glowMat1);
                break;
            case VoiceCommand.State.Friends:
                TriggerGlow(glowMat2);
                break;
            case VoiceCommand.State.Help:
                TriggerGlow(glowMat1);
                break;
            case VoiceCommand.State.NotHelp:
                TriggerGlow(glowMat2);
                break;
            default:
                break;
            }
        //状態ごとに光らせるものをcase文で定義することによって片方だけ光らせることはできる。
        // 一瞬光らせる
        // テキスト変更はディレイ後に
        var lines = GetTextForState3(newState);
        if(lines.Count > 0)
        {
            uiText3.text = lines[0]; // 最初の行を表示
        }
        StartCoroutine(ChangeTextWithDelay(newState, 4.0f));
    }

    private void TriggerGlow(Material mat)
    {
        StartCoroutine(GlowRoutine(mat));
    }

    private System.Collections.IEnumerator GlowRoutine(Material mat)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            mat.SetColor("_EmissionColor", emissionOnColor);
            yield return new WaitForSeconds(blinkInterval);
            mat.SetColor("_EmissionColor", emissionOffColor);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private System.Collections.IEnumerator ChangeTextWithDelay(VoiceCommand.State newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        uiText1.text = GetTextForState1(newState);
        uiText2.text = GetTextForState2(newState);
        StartCoroutine(ShowNarrationSequence(GetTextForState3(newState)));

    }

    private string GetTextForState1(VoiceCommand.State state)
    {
        switch (state)
        {
            case VoiceCommand.State.Opening: return "Draw the sword";
            case VoiceCommand.State.Sword: return "Alone";
            case VoiceCommand.State.Heroine: return "Save";
            default: return "";
        }
    }

    private string GetTextForState2(VoiceCommand.State state)
    {
        switch (state)
        {
            case VoiceCommand.State.Opening: return "Ignore the sword";
            case VoiceCommand.State.Sword: return "With companion";
            case VoiceCommand.State.Heroine: return "Run away";
            default: return "";
        }
    }
    private List<string> GetTextForState3(VoiceCommand.State state)
    {
        switch (state)
        {
            case VoiceCommand.State.Opening:
                return new List<string>
                {
                    "This is a world threatened by the Demon King.",
                    "Before a lone hero stands the legendary sword.",
                    "Will he draw the blade and face the Demon King?",
                    "He must make a choice—one that will shape the fate of all."
                };
            case VoiceCommand.State.Sword:
                return new List<string>
                {
                    "The hero drew the sword and chose to fight.",
                    "Ahead lies the battle that awaits in the Demon King's castle.",
                    "Will he walk this path alone—",
                    "or journey forward alongside companions?",
                };
            case VoiceCommand.State.Heroine:
                return new List<string>
                {
                    "The hero chose not to draw the sword.",
                    "He turned away from the legendary sword,",
                    "After that, he came across a young girl attacked by mosnters.",
                    "Should he help her, or run away?"
                };
            case VoiceCommand.State.Alone:
                return new List<string>
                {
                    "The hero set out alone to challenge the Demon King.",
                    "Thanks to the power of the legendary sword,",
                    "he finally reached the Demon King's throne.",
                    "But before the overwhelming might of the Demon King,",
                    "the hero stood no chance—and was defeated."

                };
            case VoiceCommand.State.Friends:
                return new List<string>
                {
                    "Together with his companions, the hero overcame countless trials.",
                    "At last, they reached the Demon King's stronghold.",
                    "With the support of his allies," ,
                    "the hero finally succeeded in defeating the Demon King.",
                    "And so, peace returned to the world."


                };
            case VoiceCommand.State.Help:
                return new List<string>
                {
                    "Summoning his courage, the hero finally defeated monsters",
                    "The girl was saved and deeply grateful.",
                    "In time, the hero and the girl became bound by love.",
                    "They decided to live together,"

                };
            case VoiceCommand.State.NotHelp:
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

    private IEnumerator ShowNarrationSequenceOpening(List<string> lines)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            uiText3.text = lines[i];

            // 必要であればここで VOICEVOX 音声再生処理も追加
            // audioSource.PlayOneShot(...)

            if (i < lines.Count - 1)
                yield return new WaitForSeconds(narrationInterval);
        }
    }

    private IEnumerator ShowNarrationSequence(List<string> lines)
    {
        for (int i = 1; i < lines.Count; i++)
        {
            uiText3.text = lines[i];

            // 必要であればここで VOICEVOX 音声再生処理も追加
            // audioSource.PlayOneShot(...)

            if (i < lines.Count - 1)
                yield return new WaitForSeconds(narrationInterval);
        }
    }
}