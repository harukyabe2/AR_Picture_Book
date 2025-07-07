using UnityEngine;
using Vuforia; // Vuforiaの名前空間を追加

public class ARObjectSwitcher : DefaultObserverEventHandler
{
    // 切り替えたいプレハブの配列
    public GameObject[] arPrefabs;
    private GameObject currentARObject; // 現在表示されているARオブジェクト

    protected override void Start()
    {
        base.Start(); // 親クラスのStartを呼び出す (Tracking lost/foundイベントのため)
        // 初期状態では何も表示しない、またはデフォルトのオブジェクトを表示する
        if (arPrefabs != null && arPrefabs.Length > 0)
        {
            // 最初は何も表示しない場合はコメントアウト
            // SwitchARObject(0); 
        }
    }

    // 外部から呼び出してARオブジェクトを切り替えるメソッド
    public void SwitchARObject(int prefabIndex)
    {
        if (prefabIndex >= 0 && prefabIndex < arPrefabs.Length)
        {
            // 既存のオブジェクトがあれば破棄する
            if (currentARObject != null)
            {
                Destroy(currentARObject);
            }

            // 新しいオブジェクトをインスタンス化し、ImageTargetの子にする
            currentARObject = Instantiate(arPrefabs[prefabIndex], transform);
            currentARObject.transform.localPosition = Vector3.zero; // 必要に応じて位置調整
            currentARObject.transform.localRotation = Quaternion.identity; // 必要に応じて回転調整
            currentARObject.transform.localScale = new Vector3(10f, 10f, 10f); // 必要に応じてスケール調整
            currentARObject.SetActive(true);
            Debug.Log("AR Object Switched to: " + arPrefabs[prefabIndex].name);
        }
        else
        {
            Debug.LogWarning("Invalid prefab index: " + prefabIndex);
        }
    }

    // オブジェクトを非表示にする（オプション）
    public void HideARObject()
    {
        if (currentARObject != null)
        {
            Destroy(currentARObject);
            currentARObject = null;
            Debug.Log("AR Object Hidden.");
        }
    }

    // マーカーが認識されたときに呼ばれる
    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        // マーカー認識時にデフォルトのオブジェクトを表示したい場合はここにロジックを追加
        // 例: SwitchARObject(0);
    }

    // マーカーが失われたときに呼ばれる
    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        // マーカーが失われたときにオブジェクトを非表示にする
        HideARObject();
    }
}