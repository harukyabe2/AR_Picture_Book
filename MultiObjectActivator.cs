using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiObjectActivator : MonoBehaviour
{
    [Header("表示する4つのオブジェクト")]
    public GameObject object_2a_PathA;
    public GameObject object_2a_PathB;
    public GameObject object_2b_PathA;
    public GameObject object_2b_PathB;

    void Awake()
    {
        // 開始時は全てのオブジェクトを非表示に
        object_2a_PathA.SetActive(false);
        object_2a_PathB.SetActive(false);
        object_2b_PathA.SetActive(false);
        object_2b_PathB.SetActive(false);
    }

    // Image TargetのOnTargetFound()から呼び出されるメソッド
    public void ActivateObjectBasedOnFinalOutcome()
    {
        if (SceneManager.Instance == null)
        {
            Debug.LogError("SceneManagerが見つかりません！");
            return;
        }

        // SceneManagerに保存された最終結果に応じて、表示するオブジェクトを切り替える
        switch (SceneManager.Instance.CurrentFinalOutcome)
        {
            case SceneManager.FinalOutcome.Result_From_2a_PathA:
                object_2a_PathA.SetActive(true);
                break;
            case SceneManager.FinalOutcome.Result_From_2a_PathB:
                object_2a_PathB.SetActive(true);
                break;
            case SceneManager.FinalOutcome.Result_From_2b_PathA:
                object_2b_PathA.SetActive(true);
                break;
            case SceneManager.FinalOutcome.Result_From_2b_PathB:
                object_2b_PathB.SetActive(true);
                break;
            case SceneManager.FinalOutcome.None:
            default:
                Debug.LogWarning("最終結果が設定されていません。");
                break;
        }
    }
}