using System.Diagnostics;
using UnityEngine;

public class Scene3d : MonoBehaviour
{
    // 状態を管理するためのenum
    private enum ActionState
    {
        MovingToHero, // 1. ヒーローへ移動中
        Rotating,     // 2. 回転中
        MovingToHeroin,
        CoolTime,
        HeroinAttack,
        Finished     
    }

    private ActionState currentState;

    [Header("Object")]
    public GameObject Heroin;
    public GameObject BarrenLand;
    public GameObject Plant1;
    public GameObject Plant2;
    public GameObject Plant3;

    [Header("Target Position")]
    // インスペクターでヒーローのオブジェクトを指定します
    public Transform heroPosition;
    public Transform heroinPosition;

    [Header("Movement Settings")]
    public float heroMoveSpeed = 40.0f;
    public float heroinMoveSpeed = 10.0f;
    public float rotateSpeed = 270.0f;

    [Header("Effect")]
    public GameObject HeroinAura;
    public GameObject HeroinAttackEffect;

    [Header("Material")]
    public Material TreeMaterial;

    // 目標の回転（Y軸180度）
    private Quaternion targetRotation = Quaternion.Euler(0, 270, 0);

    private Animator HeroMovement;
    private Animator HeroinMovement;

    private bool stateChange = false;

    void Start()
    {
        TreeMaterial.shader = Shader.Find("Fristy/Nature/Leaves");
        // heroPositionが設定されているか確認
        if (heroPosition == null)
        {
            currentState = ActionState.Finished; // エラーなので動作を停止
            return;
        }

        if (heroinPosition == null)
        {
            currentState = ActionState.Finished; // エラーなので動作を停止
            return;
        }

        // 最初の状態をセット
        currentState = ActionState.MovingToHero;

        HeroMovement = GetComponent<Animator>();
        HeroinMovement = Heroin.GetComponent<Animator>();
        // UIControllerにUI表示を命令
        SceneManager.Instance.ShowUIPrompt(UIController.UIScreenID.Scene3d_Bad);
    }

    // Scene3d.cs の Update()メソッド
    void Update()
    {
        switch (currentState)
        {
            case ActionState.MovingToHero:
                if (heroPosition == null) return;
                // ★.position -> .localPosition に変更
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, heroPosition.localPosition, heroMoveSpeed * Time.deltaTime);
                // ★.position -> .localPosition に変更
                if (Vector3.Distance(transform.localPosition, heroPosition.localPosition) < 0.01f)
                {
                    currentState = ActionState.Rotating;
                }
                break;

            case ActionState.Rotating:
                // ★.rotation -> .localRotation に変更
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, rotateSpeed * Time.deltaTime);
                // ★.rotation -> .localRotation に変更
                if (transform.localRotation == targetRotation)
                {
                    HeroMovement.SetTrigger("IdleTrigger");
                    currentState = ActionState.MovingToHeroin;
                }
                break;

            case ActionState.MovingToHeroin:
                Heroin.SetActive(true);
                // ★.position -> .localPosition に変更
                Heroin.transform.localPosition = Vector3.MoveTowards(Heroin.transform.localPosition, heroinPosition.localPosition, heroinMoveSpeed * Time.deltaTime);
                // ★.position -> .localPosition に変更
                if (Vector3.Distance(Heroin.transform.localPosition, heroinPosition.localPosition) < 0.01f)
                {
                    HeroinMovement.SetTrigger("IdleTrigger");
                    Invoke(nameof(HeroinAuraActive), 0.5f);
                    Invoke(nameof(BarrenLandApperance), 1.0f);
                    currentState = ActionState.CoolTime;
                }
                break;

            // (以下のcase文は変更なし)
            case ActionState.CoolTime:
                if (!stateChange)
                {
                    Invoke(nameof(StateToAttack), 2.0f);
                    stateChange = true;
                }
                break;
            case ActionState.HeroinAttack:
                HeroinMovement.SetTrigger("AttackTrigger");
                Invoke(nameof(HeroinAttackEffectOn), 0.9f);
                Invoke(nameof(HeroinAttackEffectOff), 1.9f);
                Invoke(nameof(HeroDieTriggerOn), 1f);
                Invoke(nameof(HeroinVictoryBoolTrue), 2.0f);
                currentState = ActionState.Finished;
                break;
            case ActionState.Finished:
                break;
        }
    }

    void HeroinAuraActive()
    {
        HeroinAura.SetActive(true);
    }

    void BarrenLandApperance()
    {
        BarrenLand.SetActive(true);
        TreeMaterial.shader = Shader.Find("GUI/Text Shader");
        Plant1.SetActive(false);
        Plant2.SetActive(false);
        Plant3.SetActive(false);
    }

    void StateToAttack()
    {
        currentState = ActionState.HeroinAttack;
    }

    void HeroinAttackEffectOn()
    {
        HeroinAttackEffect.SetActive(true);
    }

    void HeroinAttackEffectOff()
    {
        HeroinAttackEffect.SetActive(false);
    }

    void HeroDieTriggerOn()
    {
        HeroMovement.SetTrigger("DieTrigger");
    }

    void HeroinVictoryBoolTrue()
    {
        HeroinMovement.SetBool("VictoryBool", true);
    }
}