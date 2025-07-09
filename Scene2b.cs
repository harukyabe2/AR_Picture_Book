using UnityEngine;
// 音声認識に必要なライブラリを追加
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

public class FinalMovementControllerV2 : MonoBehaviour
{
    // オブジェクトの状態を管理
    private enum MovementState
    {
        TurningToWaitPoint,
        MovingToWaitPoint,
        TurningToPreviewA,
        WaitingForInput, // この状態の時に音声入力を待つ
        MovingToA,
        TurningToB,
        MovingToB,
        Finished
    }

    private MovementState currentState;

    [Header("Settings")]
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 180.0f;

    // 各目標地点 (ローカル座標)
    private Vector3 waitPoint = new Vector3(0f, 0f, 20f);
    private Vector3 targetA = new Vector3(-11f, 0f, 28f);
    private Vector3 targetB = new Vector3(30f, 0f, 20f);

    private Quaternion targetRotation;

    // 各オブジェクトへの参照
    public GameObject heroin;
    public GameObject turtle1;
    public GameObject turtle2;
    public GameObject turtle3;
    public GameObject HeroinLoseEffect;

    // 各アニメーターへの参照
    private Animator HeroMovement;
    private Animator HeroinMovement;
    private Animator Turtle1Movement;
    private Animator Turtle2Movement;
    private Animator Turtle3Movement;

    // --- 音声認識用の変数を追加 ---
    private KeywordRecognizer recognizer;
    private Dictionary<string, System.Action> voiceCommands = new Dictionary<string, System.Action>();

    void Start()
    {
        currentState = MovementState.TurningToWaitPoint;
        HeroMovement = GetComponent<Animator>();
        HeroinMovement = heroin.GetComponent<Animator>();
        Turtle1Movement = turtle1.GetComponent<Animator>();
        Turtle2Movement = turtle2.GetComponent<Animator>();
        Turtle3Movement = turtle3.GetComponent<Animator>();
        // ★ローカル座標で動作するように初期位置を設定
        transform.localPosition = Vector3.zero;
    }

    void Update()
    {
        switch (currentState)
        {
            case MovementState.TurningToWaitPoint:
                // ★ローカル座標で計算
                Vector3 directionToWait = (waitPoint - transform.localPosition).normalized;
                targetRotation = Quaternion.LookRotation(directionToWait);
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotateSpeed * Time.deltaTime);
                if (Quaternion.Angle(transform.localRotation, targetRotation) < 1.0f)
                {
                    currentState = MovementState.MovingToWaitPoint;
                }
                break;

            case MovementState.MovingToWaitPoint:
                // ★ローカル座標で移動
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, waitPoint, moveSpeed * Time.deltaTime);
                if (transform.localPosition == waitPoint)
                {
                    currentState = MovementState.TurningToPreviewA;
                }
                break;

            case MovementState.TurningToPreviewA:
                // ★ローカル座標で計算
                Vector3 directionToA_Preview = (targetA - transform.localPosition).normalized;
                targetRotation = Quaternion.LookRotation(directionToA_Preview);
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotateSpeed * Time.deltaTime);
                if (Quaternion.Angle(transform.localRotation, targetRotation) < 1.0f)
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

            case MovementState.MovingToA:
                // ★ローカル座標で移動
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetA, moveSpeed * Time.deltaTime);
                if (transform.localPosition == targetA)
                {
                    currentState = MovementState.Finished;
                    HeroMovement.SetTrigger("IdleTrigger");
                    HeroMovement.SetTrigger("AttackTrigger");
                    Invoke(nameof(HeroVictoryTriggerOn), 3f);
                    Invoke(nameof(HeroinVictoryTriggerOn), 3f);
                    Invoke(nameof(Turtle1DieTriggerOn), 0.4f);
                    Invoke(nameof(Turtle2DieTriggerOn), 0.933f);
                    Invoke(nameof(Turtle3DieTriggerOn), 1.466f);
                }
                break;

            case MovementState.TurningToB:
                // ★ローカル座標で回転
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotateSpeed * Time.deltaTime);
                if (Quaternion.Angle(transform.localRotation, targetRotation) < 1.0f)
                {
                    currentState = MovementState.MovingToB;
                }
                break;

            case MovementState.MovingToB:
                // ★ローカル座標で移動
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetB, moveSpeed * Time.deltaTime);
                if (transform.localPosition == targetB)
                {
                    currentState = MovementState.Finished;
                    gameObject.SetActive(false);
                    Invoke(nameof(HeroinLose), 1f);
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
        Debug.Log("音声入力を待っています... コマンド: 「助ける」または「去る」");
    }

    private void SetupVoiceCommands()
    {
        voiceCommands.Clear();

        // 「助ける」と発声された時の処理
        voiceCommands.Add("助ける", () => {
            currentState = MovementState.MovingToA;
            HeroMovement.SetTrigger("MoveTrigger");
            SceneManager.Instance.SetFinalOutcome(SceneManager.FinalOutcome.Result_From_2b_PathA);
        });

        // 「去る」と発声された時の処理
        voiceCommands.Add("去る", () => {
            HeroMovement.SetTrigger("MoveTrigger");
            Vector3 directionToB = (targetB - transform.localPosition).normalized;
            targetRotation = Quaternion.LookRotation(directionToB);
            currentState = MovementState.TurningToB;
            SceneManager.Instance.SetFinalOutcome(SceneManager.FinalOutcome.Result_From_2b_PathB);
        });
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("認識されたコマンド: " + args.text);
        if (voiceCommands.ContainsKey(args.text))
        {
            voiceCommands[args.text].Invoke();
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

    // --- 以下、元からあったメソッド群 (変更なし) ---
    void HeroVictoryTriggerOn() { HeroMovement.SetTrigger("VictoryTrigger"); }
    void HeroinVictoryTriggerOn() { HeroinMovement.SetTrigger("VictoryTrigger"); }
    void Turtle1DieTriggerOn() { Turtle1Movement.SetTrigger("DieTrigger"); Invoke(nameof(DestroyTurtle1), 2f); }
    void Turtle2DieTriggerOn() { Turtle2Movement.SetTrigger("DieTrigger"); Invoke(nameof(DestroyTurtle2), 2f); }
    void Turtle3DieTriggerOn() { Turtle3Movement.SetTrigger("DieTrigger"); Invoke(nameof(DestroyTurtle3), 2f); }
    void DestroyTurtle1() { Destroy(turtle1); }
    void DestroyTurtle2() { Destroy(turtle2); }
    void DestroyTurtle3() { Destroy(turtle3); }
    void HeroinLose() { HeroinMovement.SetTrigger("LoseTrigger"); Turtle1Movement.SetBool("AttackBool", false); Turtle2Movement.SetBool("AttackBool", false); Turtle3Movement.SetBool("AttackBool", false); Invoke(nameof(HeroinDark), 1f); }
    void HeroinDark() { HeroinLoseEffect.SetActive(true); }
}