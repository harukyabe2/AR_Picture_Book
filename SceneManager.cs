using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    // --- 1から2への分岐管理用 (以前のbool値を残します) ---
    public bool PlayerChosePathA { get; private set; }

    // --- 2から3への分岐管理用 (新しいenumを追加します) ---
    public enum FinalOutcome
    {
        None,
        Result_From_2a_PathA,
        Result_From_2a_PathB,
        Result_From_2b_PathA,
        Result_From_2b_PathB
    }
    public FinalOutcome CurrentFinalOutcome { get; private set; }

    public UIController uiController;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (uiController == null)
        {
            uiController = FindObjectOfType<UIController>();
        }
    }

    // --- それぞれの値を設定するためのメソッド ---

    // 1から2への分岐で使うメソッド (以前のまま)
    public void SetNextSceneChoice(bool chosePathA)
    {
        PlayerChosePathA = chosePathA;
        Debug.Log("SceneManager: 次の分岐を " + (chosePathA ? "A" : "B") + " に設定しました。");
    }

    // 2から3への分岐で使うメソッド (新しく追加)
    public void SetFinalOutcome(FinalOutcome outcome)
    {
        CurrentFinalOutcome = outcome;
        Debug.Log("SceneManager: 最終結果が「" + outcome.ToString() + "」に設定されました。");
    }

    public void ShowUIPrompt(UIController.UIScreenID screenId)
    {
        if (uiController != null)
        {
            uiController.ShowScreen(screenId);
        }
        else
        {
            Debug.LogError("UIManagerがSceneManagerに設定されていません！");
        }
    }

    public void HideUIPrompt()
    {
        if (uiController != null)
        {
            uiController.HideAllUI();
        }
    }

    // SceneManager.cs の末尾に以下のメソッドを追加

    public void TriggerChoiceGlow(bool isLeftChoice)
    {
        if (uiController != null)
        {
            uiController.TriggerGlowEffect(isLeftChoice);
        }
    }
}