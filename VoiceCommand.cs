using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

public class VoiceCommand : MonoBehaviour
{
    private KeywordRecognizer recognizer;
    private Dictionary<string, System.Action> currentCommands = new Dictionary<string, System.Action>();

    public enum State { Opening, Sword, Heroine, Alone, Friends, Help, NotHelp }//シーンごとの状態変数の設定
    public State currentState;

    void Start()
    {
        UpdateCommandsForState(currentState);
        //SetState(State.Opening);
        SetState(State.Sword);
        //SetState(State.Heroine);
    }

    public void SetState(State newState)
    {
        if (newState != currentState)
        {
            currentState = newState;
            UpdateCommandsForState(currentState);
        }
    }

    private void UpdateCommandsForState(State state)
    {
        // 以前のRecognizerを止めて破棄
        if (recognizer != null && recognizer.IsRunning)
            recognizer.Stop();

        if (recognizer != null)
        {
            recognizer.OnPhraseRecognized -= OnPhraseRecognized;
            recognizer.Dispose();
        }
        Debug.Log("音声を入力して");
        // 状態に応じたコマンドを設定
        currentCommands.Clear();
        switch (state)
        {
            case State.Opening:
                currentCommands.Add("抜く", () => Debug.Log("剣を抜く！"));
                currentCommands.Add("抜かない", () => Debug.Log("剣を抜かない"));
                break;

            case State.Sword:
                currentCommands.Add("一人で", () => Debug.Log("一人で戦う"));
                currentCommands.Add("仲間と", () => Debug.Log("仲間と戦う"));
                break;

            case State.Heroine:
                currentCommands.Add("助ける", () => Debug.Log("助ける"));
                currentCommands.Add("助けない", () => Debug.Log("助ける"));
                break;
        }

        // 新しいRecognizerを作成
        if (currentCommands.Count > 0)
        {
            recognizer = new KeywordRecognizer(currentCommands.Keys.ToArray());
            recognizer.OnPhraseRecognized += OnPhraseRecognized;
            recognizer.Start();
        }
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("認識されたコマンド: " + args.text);
        if (currentCommands.ContainsKey(args.text))
            currentCommands[args.text].Invoke();
    }
}
