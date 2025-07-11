using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scene3b : MonoBehaviour
{
    public GameObject Heroin;
    public GameObject Wizard;
    public GameObject Demon;

    public GameObject DemonEffect;
    public GameObject HeroSwordEffect;
    public GameObject HeroAttackEffect;
    public GameObject HeroinAttackEffect1;
    public GameObject HeroinAttackEffect2;
    public GameObject HeroinAttackEffect3;
    public GameObject WizardAttackEffect;

    private Animator HeroWinning;
    private Animator HeroinWinning;
    private Animator WizardWinning;
    private Animator DemonDying;

    private float startOffset = 3.0f;

    public GameObject targetTerrainGameObject;

    // Start is called before the first frame update
    void Start()
    {
        // UIControllerにUI表示を命令
        SceneManager.Instance.ShowUIPrompt(UIController.UIScreenID.Scene3b_WithCompanion);
        HeroWinning = GetComponent<Animator>();
        HeroinWinning = Heroin.GetComponent<Animator>();
        WizardWinning = Wizard.GetComponent<Animator>();
        DemonDying = Demon.GetComponent<Animator>();

        Invoke(nameof(PartyAttackTriggerOn), startOffset);
        Invoke(nameof(HeroAttackTriggerOn), startOffset + 2.2f);
        //Invoke(nameof(DemonDieTriggerOn), startOffset + 3.5f);
        Invoke(nameof(VictoryTriggerOn), startOffset + 6.1f);
        targetTerrainGameObject.SetActive(!targetTerrainGameObject.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PartyAttackTriggerOn()
    {
        HeroinWinning.SetTrigger("AttackTrigger");
        WizardWinning.SetTrigger("AttackTrigger");
        DemonDying.SetTrigger("HitTrigger");

        Invoke(nameof(HeroinAttackEffect1On), 0.2f);
        Invoke(nameof(HeroinAttackEffect2On), 0.633f);
        Invoke(nameof(HeroinAttackEffect3On), 1.066f);
        Invoke(nameof(WizardAttackEffectOn), 0.667f);
    }

    void HeroAttackTriggerOn()
    {
        Invoke(nameof(HeroSwordEffectOn), 0.6f);
        Invoke(nameof(HeroAttackEffectOn), 1.75f);
        HeroWinning.SetTrigger("AttackTrigger");
        DemonDying.SetTrigger("NotHitTrigger");
        Invoke(nameof(DemonDieTriggerOn), 2.0f);
    }

    void DemonDieTriggerOn()
    {
        DemonEffect.SetActive(false);
        DemonDying.SetTrigger("DieTrigger");
    }

    void VictoryTriggerOn()
    {
        HeroWinning.SetTrigger("VictoryTrigger");
        HeroinWinning.SetTrigger("VictoryTrigger");
        WizardWinning.SetTrigger("VictoryTrigger");
    }

    void HeroSwordEffectOn()
    {
        HeroSwordEffect.SetActive(true);
    }

    void HeroAttackEffectOn()
    {
        HeroAttackEffect.SetActive(true);
    }

    void HeroinAttackEffect1On()
    {
        HeroinAttackEffect1.SetActive(true);
    }

    void HeroinAttackEffect2On()
    {
        HeroinAttackEffect2.SetActive(true);
    }

    void HeroinAttackEffect3On()
    {
        HeroinAttackEffect3.SetActive(true);
    }

    void WizardAttackEffectOn()
    {
        WizardAttackEffect.SetActive(true);
    }
}