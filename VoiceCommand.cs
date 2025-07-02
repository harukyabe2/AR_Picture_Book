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
