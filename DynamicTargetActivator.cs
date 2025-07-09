using UnityEngine;

public class DynamicTargetActivator : MonoBehaviour
{
    [Header("ルートAで表示するオブジェクト")]
    public GameObject objectForPathA;

    [Header("ルートBで表示するオブジェクト")]
    public GameObject objectForPathB;

    void Awake()
    {
        // 開始時は、どちらのオブジェクトも非表示にしておく
        if (objectForPathA != null) objectForPathA.SetActive(false);
        if (objectForPathB != null) objectForPathB.SetActive(false);
    }

    // Image TargetのOnTargetFound()から呼び出されるメソッド
    public void ActivateObjectBasedOnChoice()
    {
        // SceneManagerが存在しない場合は何もしない
        if (SceneManager.Instance == null)
        {
            Debug.LogError("SceneManagerが見つかりません！");
            return;
        }

        // SceneManagerに保存された選択に応じて、対応するオブジェクトを有効化する
        if (SceneManager.Instance.PlayerChosePathA)
        {
            if (objectForPathA != null) objectForPathA.SetActive(true);
            if (objectForPathB != null) objectForPathB.SetActive(false);
        }
        else
        {
            if (objectForPathA != null) objectForPathA.SetActive(false);
            if (objectForPathB != null) objectForPathB.SetActive(true);
        }
    }
}