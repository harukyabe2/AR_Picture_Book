using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

public class VoiceCommand : MonoBehaviour
{
    private KeywordRecognizer recognizer;
    private Dictionary<string, System.Action> currentCommands = new Dictionary<string, System.Action>();

    public enum State { Opening, Sword, Heroine, Alone, Friends, Help, NotHelp }//�V�[�����Ƃ̏�ԕϐ��̐ݒ�
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
        // �ȑO��Recognizer���~�߂Ĕj��
        if (recognizer != null && recognizer.IsRunning)
            recognizer.Stop();

        if (recognizer != null)
        {
            recognizer.OnPhraseRecognized -= OnPhraseRecognized;
            recognizer.Dispose();
        }
        Debug.Log("��������͂���");
        // ��Ԃɉ������R�}���h��ݒ�
        currentCommands.Clear();
        switch (state)
        {
            case State.Opening:
                currentCommands.Add("����", () => Debug.Log("���𔲂��I"));
                currentCommands.Add("�����Ȃ�", () => Debug.Log("���𔲂��Ȃ�"));
                break;

            case State.Sword:
                currentCommands.Add("��l��", () => Debug.Log("��l�Ő키"));
                currentCommands.Add("���Ԃ�", () => Debug.Log("���ԂƐ키"));
                break;

            case State.Heroine:
                currentCommands.Add("������", () => Debug.Log("������"));
                currentCommands.Add("�����Ȃ�", () => Debug.Log("������"));
                break;
        }

        // �V����Recognizer���쐬
        if (currentCommands.Count > 0)
        {
            recognizer = new KeywordRecognizer(currentCommands.Keys.ToArray());
            recognizer.OnPhraseRecognized += OnPhraseRecognized;
            recognizer.Start();
        }
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("�F�����ꂽ�R�}���h: " + args.text);
        if (currentCommands.ContainsKey(args.text))
            currentCommands[args.text].Invoke();
    }
}
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

public class VoiceCommand : MonoBehaviour
{
    private KeywordRecognizer recognizer;
    private Dictionary<string, System.Action> currentCommands = new Dictionary<string, System.Action>();

    public enum State { Opening, Sword, Heroine, Alone, Friends, Help, NotHelp }
    public State currentState;

    // ARObjectSwitcherへの参照を追加
    public ARObjectSwitcher arObjectSwitcher;

    void Start()
    {
        // ARObjectSwitcherの参照が設定されているか確認
        if (arObjectSwitcher == null)
        {
            Debug.LogError("ARObjectSwitcher is not assigned in the Inspector!");
            // もしImage Targetにアタッチされているなら、GameObject.Findなどで取得することも可能
            // arObjectSwitcher = FindObjectOfType<ARObjectSwitcher>(); // 注意: シーンに一つしかない場合に限る
        }

        UpdateCommandsForState(currentState);
        // SetState(State.Opening); 
        SetState(State.Sword); // テスト用にSword状態から開始
        // SetState(State.Heroine);
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
                currentCommands.Add("抜く", () => { Debug.Log("剣を抜く！"); if (arObjectSwitcher != null) arObjectSwitcher.SwitchARObject(0); }); // プレハブのインデックスを指定
                currentCommands.Add("抜かない", () => { Debug.Log("剣を抜かない"); if (arObjectSwitcher != null) arObjectSwitcher.HideARObject(); });
                break;

            case State.Sword:
                currentCommands.Add("一人で", () => { Debug.Log("一人で戦う"); if (arObjectSwitcher != null) arObjectSwitcher.SwitchARObject(0); }); // プレハブのインデックスを指定
                currentCommands.Add("仲間と", () => { Debug.Log("仲間と戦う"); if (arObjectSwitcher != null) arObjectSwitcher.SwitchARObject(1); });
                break;

            case State.Heroine:
                currentCommands.Add("助ける", () => { Debug.Log("助ける"); if (arObjectSwitcher != null) arObjectSwitcher.SwitchARObject(0); });
                currentCommands.Add("助けない", () => { Debug.Log("助けない"); if (arObjectSwitcher != null) arObjectSwitcher.SwitchARObject(1); });
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

    void OnDestroy()
    {
        if (recognizer != null)
        {
            recognizer.OnPhraseRecognized -= OnPhraseRecognized;
            recognizer.Dispose();
        }
    }
}