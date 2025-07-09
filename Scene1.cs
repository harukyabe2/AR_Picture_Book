using System;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

public class Scene1 : MonoBehaviour
{
    // === 状態管理の定義 ===
    public enum MovementState
    {
        AwaitingTracking, // マーカーが認識されるのを待つ状態
        MovingToCenter, RotatingAtCenter, WaitingForInput,
        MovingToPositionA, RotatingForB, MovingToPositionB, Finished
    }
    public enum VoiceState { Opening, Sword, Heroine }

    [Header("State Control")]
    public MovementState currentMovementState;
    public VoiceState currentVoiceState;

    // (以下、変数の定義は変更なし)
    [Header("Movement Settings")]
    public float moveSpeed = 8.0f;
    public float rotateSpeed = 90.0f;
    [Header("Object References")]
    public GameObject hero_havingSword;
    public GameObject sword_onRock;
    private Vector3 startPosition = new Vector3(-25f, 0f, 13f);
    private Vector3 centerPosition = new Vector3(0f, 0f, 13f);
    private Vector3 targetPositionA = new Vector3(0f, 0f, 22f);
    private Vector3 targetPositionB = new Vector3(30f, 0f, 13f);
    private Quaternion targetRotation1 = Quaternion.Euler(0, 0, 0);
    private Quaternion targetRotation2 = Quaternion.Euler(0, 90, 0);
    private float lookSwordTime = 1.0f;
    private Animator HeroFindingTheSword;
    private KeywordRecognizer recognizer;
    private Dictionary<string, System.Action> voiceCommands = new Dictionary<string, System.Action>();

    void Start()
    {
        // すぐに開始せず、待機状態にする
        transform.localPosition = startPosition;
        currentMovementState = MovementState.AwaitingTracking; // マーカー認識待機状態から開始
        HeroFindingTheSword = GetComponent<Animator>();
    }

    void Update()
    {
        if (currentMovementState == MovementState.AwaitingTracking)
        {
            return;
        }

        switch (currentMovementState)
        {
            case MovementState.MovingToCenter:
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, centerPosition, moveSpeed * Time.deltaTime);
                if (transform.localPosition == centerPosition)
                {
                    HeroFindingTheSword.SetTrigger("IdleTrigger");
                    currentMovementState = MovementState.RotatingAtCenter;
                }
                break;

            case MovementState.RotatingAtCenter:
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation1, rotateSpeed * Time.deltaTime);
                if (transform.localRotation == targetRotation1)
                {
                    // 回転が完了したら、再度Idleトリガーを送り、確実に待機状態にする
                    HeroFindingTheSword.SetTrigger("IdleTrigger");

                    // 音声入力を待つ状態へ移行
                    currentMovementState = MovementState.WaitingForInput;
                    InitializeAndStartRecognizer();
                }
                break;

            case MovementState.WaitingForInput:
                // 音声入力待ち
                break;

            case MovementState.MovingToPositionA:
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPositionA, moveSpeed * Time.deltaTime);
                if (transform.localPosition == targetPositionA)
                {
                    HeroFindingTheSword.SetTrigger("IdleTrigger");
                    gameObject.SetActive(false);
                    Invoke(nameof(SwordSetActiveFalse), lookSwordTime);
                    Invoke(nameof(HeroSetActiveTrue), lookSwordTime);
                    Invoke(nameof(StatementToFinish), lookSwordTime);
                }
                break;

            case MovementState.RotatingForB:
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation2, rotateSpeed * Time.deltaTime);
                if (transform.localRotation == targetRotation2)
                {
                    HeroFindingTheSword.SetTrigger("MoveTrigger");
                    currentMovementState = MovementState.MovingToPositionB;
                }
                break;

            case MovementState.MovingToPositionB:
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPositionB, moveSpeed * Time.deltaTime);
                if (transform.localPosition == targetPositionB)
                {
                    HeroFindingTheSword.SetTrigger("IdleTrigger");
                    currentMovementState = MovementState.Finished;
                }
                break;

            case MovementState.Finished:
                Destroy(gameObject, 2f);
                break;
        }
    }

    // --- ARマーカーに認識された時に呼び出される公開メソッド ---
    public void StartSequence()
    {
        // 待機状態の時だけ処理を開始する
        if (currentMovementState == MovementState.AwaitingTracking)
        {
            Debug.Log("ARマーカーを認識しました。シーケンスを開始します。");

            // ★追加: アニメーション再生のトリガーを引く
            HeroFindingTheSword.SetTrigger("MoveTrigger");

            // 毎回必ず初期状態から始まるように、位置と向きをリセット
            transform.localPosition = startPosition;
            transform.localRotation = Quaternion.Euler(0, 90, 0);

            // アニメーションシーケンスを開始
            currentMovementState = MovementState.MovingToCenter;
            SetVoiceState(VoiceState.Opening);
        }
    }

    // --- ARマーカーが見失われた時に呼び出される公開メソッド (推奨) ---
    public void ResetSequence()
    {
        Debug.Log("ARマーカーを見失いました。シーケンスをリセットします。");

        // 進行中の処理をすべて停止
        CleanupRecognizer();
        CancelInvoke(); // Invokeで実行待機中のメソッドをキャンセル

        // 最初の待機状態に戻す
        currentMovementState = MovementState.AwaitingTracking;

        // オブジェクトの状態も初期に戻す
        if (sword_onRock != null) sword_onRock.SetActive(true);
        if (hero_havingSword != null) hero_havingSword.SetActive(false);
    }

    // --- (以下、音声認識関連のコードは変更なし) ---
    public void SetVoiceState(VoiceState newState)
    {
        currentVoiceState = newState;
        if (recognizer != null && recognizer.IsRunning) { InitializeAndStartRecognizer(); }
    }
    private void InitializeAndStartRecognizer()
    {
        CleanupRecognizer();
        UpdateVoiceCommandsForState(currentVoiceState);
        if (voiceCommands.Count > 0)
        {
            recognizer = new KeywordRecognizer(voiceCommands.Keys.ToArray());
            recognizer.OnPhraseRecognized += OnPhraseRecognized;
            recognizer.Start();
            Debug.Log("音声入力を待っています... コマンド: " + string.Join(", ", voiceCommands.Keys.ToArray()));
        }
    }
    private void UpdateVoiceCommandsForState(VoiceState state)
    {
        voiceCommands.Clear();
        switch (state)
        {
            case VoiceState.Opening:
                voiceCommands.Add("抜く", () => { HeroFindingTheSword.SetTrigger("MoveTrigger"); currentMovementState = MovementState.MovingToPositionA; SceneManager.Instance.SetNextSceneChoice(true); });
                voiceCommands.Add("抜かない", () => { HeroFindingTheSword.SetTrigger("MoveTrigger"); currentMovementState = MovementState.RotatingForB; SceneManager.Instance.SetNextSceneChoice(false); });
                break;
            case VoiceState.Sword: voiceCommands.Add("一人で", () => CleanupRecognizer()); voiceCommands.Add("仲間と", () => CleanupRecognizer()); break;
            case VoiceState.Heroine: voiceCommands.Add("助ける", () => CleanupRecognizer()); voiceCommands.Add("助けない", () => CleanupRecognizer()); break;
        }
    }
    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (voiceCommands.ContainsKey(args.text)) { voiceCommands[args.text].Invoke(); CleanupRecognizer(); }
    }
    private void CleanupRecognizer()
    {
        if (recognizer != null) { recognizer.Stop(); recognizer.OnPhraseRecognized -= OnPhraseRecognized; recognizer.Dispose(); recognizer = null; }
    }
    void OnDestroy() { CleanupRecognizer(); }
    void SwordSetActiveFalse() { if (sword_onRock != null) sword_onRock.SetActive(false); }
    void HeroSetActiveTrue() { if (hero_havingSword != null) hero_havingSword.SetActive(true); }
    void StatementToFinish() { currentMovementState = MovementState.Finished; }
}