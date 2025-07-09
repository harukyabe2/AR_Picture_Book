using UnityEngine;
// 音声認識に必要なライブラリを追加
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

// クラス名を元のものに合わせます
public class Scene2a_HeroSelectingWay : MonoBehaviour
{
    // オブジェクトの状態を管理
    private enum MovementState
    {
        MovingToWaitPoint, // 1. 待機場所へ移動中
        WaitingForInput,   // 2. 音声入力待ち
        TurningToA,        // 3a. A地点へ旋回中
        MovingToA,         // 4a. A地点へ移動中
        TurningToB,        // 3b. B地点へ旋回中
        MovingToB,         // 4b. B地点へ移動中
        ResettingRotation, // 5. 回転をリセット中
        Finished           // 6. 全て完了
    }

    private MovementState currentState;

    [Header("Settings")]
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 180.0f;

    // 各目標地点 (ローカル座標)
    private Vector3 waitPoint = new Vector3(0f, 0f, 15f);
    private Vector3 targetA = new Vector3(-16f, 0f, 25f);
    private Vector3 targetB = new Vector3(14f, 0f, 25f);

    private Quaternion targetRotation;
    private Animator HeroMovement;

    // --- 音声認識用の変数を追加 ---
    private KeywordRecognizer recognizer;
    private Dictionary<string, System.Action> voiceCommands = new Dictionary<string, System.Action>();

    void Start()
    {
        currentState = MovementState.MovingToWaitPoint;
        HeroMovement = GetComponent<Animator>();
        // ★ローカル座標で動作するように初期位置を設定
        transform.localPosition = Vector3.zero;
    }

    void Update()
    {
        switch (currentState)
        {
            case MovementState.MovingToWaitPoint:
                // ★ローカル座標で移動
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, waitPoint, moveSpeed * Time.deltaTime);
                if (transform.localPosition == waitPoint)
                {
                    currentState = MovementState.WaitingForInput;
                    HeroMovement.SetTrigger("IdleTrigger");
                    // ★音声認識を開始
                    InitializeAndStartRecognizer();
                }
                break;

            case MovementState.WaitingForInput:
                // 音声認識イベントで処理されるため、Update内では何もしない
                break;

            case MovementState.TurningToA:
            case MovementState.TurningToB:
                // ★ローカル座標で回転 (AとBで処理は共通)
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotateSpeed * Time.deltaTime);
                if (Quaternion.Angle(transform.localRotation, targetRotation) < 1.0f)
                {
                    // 状態に応じて次の移動先へ
                    currentState = (currentState == MovementState.TurningToA) ? MovementState.MovingToA : MovementState.MovingToB;
                }
                break;

            case MovementState.MovingToA:
                // ★ローカル座標で移動
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetA, moveSpeed * Time.deltaTime);
                if (transform.localPosition == targetA)
                {
                    currentState = MovementState.ResettingRotation;
                }
                break;

            case MovementState.MovingToB:
                // ★ローカル座標で移動
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetB, moveSpeed * Time.deltaTime);
                if (transform.localPosition == targetB)
                {
                    currentState = MovementState.ResettingRotation;
                }
                break;

            case MovementState.ResettingRotation:
                // ★ローカル座標で回転
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.identity, rotateSpeed * Time.deltaTime);
                if (transform.localRotation == Quaternion.identity)
                {
                    currentState = MovementState.Finished;
                    HeroMovement.SetTrigger("IdleTrigger");
                }
                break;

            case MovementState.Finished:
                // 何もしない
                break;
        }
    }

    // --- ここから下は音声認識関連のメソッド ---

    private void InitializeAndStartRecognizer()
    {
        CleanupRecognizer();
        SetupVoiceCommands();

        recognizer = new KeywordRecognizer(voiceCommands.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnPhraseRecognized;
        recognizer.Start();
        Debug.Log("音声入力を待っています... コマンド: 「左」または「右」");
    }

    private void SetupVoiceCommands()
    {
        voiceCommands.Clear();

        // 「左」と発声された時の処理
        voiceCommands.Add("一人で", () => {
            HeroMovement.SetTrigger("MoveTrigger");
            // A地点(左)の方向を計算
            Vector3 directionToA = (targetA - transform.localPosition).normalized;
            targetRotation = Quaternion.LookRotation(directionToA);
            currentState = MovementState.TurningToA;
            SceneManager.Instance.SetFinalOutcome(SceneManager.FinalOutcome.Result_From_2a_PathA);
        });

        // 「右」と発声された時の処理
        voiceCommands.Add("仲間と", () => {
            HeroMovement.SetTrigger("MoveTrigger");
            // B地点(右)の方向を計算
            Vector3 directionToB = (targetB - transform.localPosition).normalized;
            targetRotation = Quaternion.LookRotation(directionToB);
            currentState = MovementState.TurningToB;
            SceneManager.Instance.SetFinalOutcome(SceneManager.FinalOutcome.Result_From_2a_PathB);
        });
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("認識されたコマンド: " + args.text);
        if (voiceCommands.ContainsKey(args.text))
        {
            voiceCommands[args.text].Invoke();
            // コマンドを認識したら、リソース解放
            CleanupRecognizer();
        }
    }

    private void CleanupRecognizer()
    {
        if (recognizer != null)
        {
            recognizer.Stop();
            recognizer.OnPhraseRecognized -= OnPhraseRecognized;
            recognizer.Dispose();
            recognizer = null;
        }
    }

    void OnDestroy()
    {
        CleanupRecognizer();
    }
}