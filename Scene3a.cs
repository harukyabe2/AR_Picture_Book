using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene3a : MonoBehaviour
{
    public GameObject attackEffect;
    public GameObject hero;

    private Animator DemonAttacking;
    private Animator HeroDying;

    private float startOffset = 3.0f;

    public GameObject targetTerrainGameObject1, targetTerrainGameObject2;

    // Start is called before the first frame update
    void Start()
    {
        DemonAttacking = GetComponent<Animator>();
        HeroDying = hero.GetComponent<Animator>();
        Invoke(nameof(DemonAttackTriggerOn), startOffset);
        Invoke(nameof(AttackEffectOn), startOffset + 0.6f);
        Invoke(nameof(AttackEffectOff), startOffset + 2.6f);
        Invoke(nameof(HeroDieTriggerOn), startOffset + 1.7f);
        Invoke(nameof(DemonVictoryTriggerOn), startOffset + 3.0f);
        targetTerrainGameObject1.SetActive(!targetTerrainGameObject1.activeSelf);
        targetTerrainGameObject2.SetActive(!targetTerrainGameObject2.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void DemonAttackTriggerOn()
    {
        DemonAttacking.SetTrigger("AttackTrigger");
    }

    void HeroDieTriggerOn()
    {
        HeroDying.SetTrigger("DieTrigger");
    }

    void DemonVictoryTriggerOn()
    {
        DemonAttacking.SetTrigger("VictoryTrigger");
    }

    void AttackEffectOn()
    {
        attackEffect.SetActive(true);
    }

    void AttackEffectOff()
    {
        attackEffect.SetActive(false);
    }
}