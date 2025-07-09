using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI uiText1;
    public TextMeshProUGUI uiText2;

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
        StartCoroutine(ChangeTextWithDelay(newState, 1.0f));
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
    }

    private string GetTextForState1(VoiceCommand.State state)
    {
        switch (state)
        {
            case VoiceCommand.State.Opening: return "Draw a sword";
            case VoiceCommand.State.Sword: return "Alone";
            case VoiceCommand.State.Heroine: return "Save";
            default: return "";
        }
    }

    private string GetTextForState2(VoiceCommand.State state)
    {
        switch (state)
        {
            case VoiceCommand.State.Opening: return "Don't draw a sword";
            case VoiceCommand.State.Sword: return "battle With your companion";
            case VoiceCommand.State.Heroine: return "Don't save";
            default: return "";
        }
    }
}
