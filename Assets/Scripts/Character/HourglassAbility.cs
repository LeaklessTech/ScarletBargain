using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class HourglassAbility : MonoBehaviour
{
    public KeyCode abilityKey = KeyCode.R;

    public float freezeRange = 10f;

    public float freezeDuration = 5f;

    public float cooldownDuration = 60f;

    public Image cooldownImage;

    public Color goldenColour = new Color(1f, 0.84f, 0f);

    public AudioClip hourglassSound;
    public AudioSource audioSource;

    // internal
    private bool isAbilityActive;
    private bool isOnCooldown;
    private float cooldownTimer;

    // store original monster state during freeze
    private struct MaterialState
    {
        public Material material;
        public string colourPropertyName;
        public Color originalColour;
    }
    private readonly List<MaterialState> materialStates = new List<MaterialState>();
    private NavMeshAgent cachedAgent;
    private float originalAgentSpeed;
    private bool originalAgentStopped;
    private Rigidbody cachedRigidbody;
    private bool originalIsKinematic;
    private Vector3 originalVelocity;
    private Vector3 originalAngularVelocity;
    private List<Animator> cachedAnimators = new List<Animator>();
    private List<float> animatorOriginalSpeeds = new List<float>();
    private Behaviour cachedBehaviour;
    private bool originalBehaviourEnabled;
    private Monster targetMonster;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.unscaledDeltaTime;
            if (cooldownImage != null)
            {
                float totalDuration = freezeDuration + cooldownDuration;
                if (totalDuration > 0f)
                {
                    float fill = Mathf.Clamp01(cooldownTimer / totalDuration);
                    cooldownImage.fillAmount = fill;
                }
            }
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
            }
        }

        if (isAbilityActive || isOnCooldown) return;

        if (Input.GetKeyDown(abilityKey))
        {
            StartCoroutine(ActivateHourglass());
        }
    }

    private IEnumerator ActivateHourglass()
    {
        isAbilityActive = true;
        cooldownTimer = freezeDuration + cooldownDuration;
        if (cooldownImage != null)
        {
            float totalDuration = freezeDuration + cooldownDuration;
            if (totalDuration > 0f)
            {
                cooldownImage.fillAmount = 1f;
            }
        }

        // play activation sound
        if (hourglassSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hourglassSound);
        }

        // find a monster within range
        targetMonster = FindNearestMonsterWithinRange();
        if (targetMonster != null)
        {
            FreezeMonster(targetMonster);
        }

        yield return new WaitForSecondsRealtime(freezeDuration);

        // restore the monster
        if (targetMonster != null)
        {
            UnfreezeMonster(targetMonster);
            targetMonster = null;
        }

        isAbilityActive = false;

        // start cooldown
        if (cooldownDuration > 0f)
        {
            isOnCooldown = true;
            yield return new WaitForSecondsRealtime(cooldownDuration);
            isOnCooldown = false;
        }

        // reset UI and timer
        cooldownTimer = 0f;
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0f;
        }
    }

    private Monster FindNearestMonsterWithinRange()
    {
        Monster monster = FindAnyObjectByType<Monster>();
        Monster nearest = null;
        float minDistSq = freezeRange * freezeRange;
        float distSq = (monster.transform.position - transform.position).sqrMagnitude;
        if (distSq <= minDistSq)
        {
            minDistSq = distSq;
            nearest = monster;
        }
        return nearest;
    }

    private void FreezeMonster(Monster monster)
    {
        cachedAgent = monster.GetComponent<NavMeshAgent>();
        if (cachedAgent != null)
        {
            originalAgentSpeed = cachedAgent.speed;
            originalAgentStopped = cachedAgent.isStopped;
            cachedAgent.isStopped = true;
            cachedAgent.speed = 0f;
        }

        cachedRigidbody = monster.GetComponent<Rigidbody>();
        if (cachedRigidbody != null)
        {
            originalIsKinematic = cachedRigidbody.isKinematic;
            originalVelocity = cachedRigidbody.linearVelocity;
            originalAngularVelocity = cachedRigidbody.angularVelocity;
            cachedRigidbody.linearVelocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;
            cachedRigidbody.isKinematic = true;
        }

        cachedAnimators.Clear();
        animatorOriginalSpeeds.Clear();
        foreach (Animator anim in monster.GetComponentsInChildren<Animator>())
        {
            cachedAnimators.Add(anim);
            animatorOriginalSpeeds.Add(anim.speed);
            anim.speed = 0f;
        }

        cachedBehaviour = monster.GetComponent<Behaviour>();
        if (cachedBehaviour != null)
        {
            originalBehaviourEnabled = cachedBehaviour.enabled;
            cachedBehaviour.enabled = false;
        }

        materialStates.Clear();
        foreach (Renderer rend in monster.GetComponentsInChildren<Renderer>())
        {
            Material[] materials = rend.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                MaterialState ms = new MaterialState { material = mat };
                if (mat.HasProperty("_Color"))
                {
                    ms.colourPropertyName = "_Color";
                    ms.originalColour = mat.GetColor("_Color");
                    mat.SetColor("_Color", goldenColour);
                }
                else if (mat.HasProperty("_BaseColor"))
                {
                    ms.colourPropertyName = "_BaseColor";
                    ms.originalColour = mat.GetColor("_BaseColor");
                    mat.SetColor("_BaseColor", goldenColour);
                }
                else if (mat.HasProperty("_MainColor"))
                {
                    ms.colourPropertyName = "_MainColor";
                    ms.originalColour = mat.GetColor("_MainColor");
                    mat.SetColor("_MainColor", goldenColour);
                }
                else
                {
                    ms.colourPropertyName = null;
                    try
                    {
                        ms.originalColour = mat.color;
                        mat.color = goldenColour;
                    }
                    catch
                    {
                        continue;
                    }
                }
                materialStates.Add(ms);
            }
        }
    }


    private void UnfreezeMonster(Monster monster)
    {
        if (cachedAgent != null)
        {
            cachedAgent.isStopped = originalAgentStopped;
            cachedAgent.speed = originalAgentSpeed;
        }
        cachedAgent = null;

        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = originalIsKinematic;
            cachedRigidbody.linearVelocity = originalVelocity;
            cachedRigidbody.angularVelocity = originalAngularVelocity;
        }
        cachedRigidbody = null;

        for (int i = 0; i < cachedAnimators.Count; i++)
        {
            if (cachedAnimators[i] != null)
            {
                cachedAnimators[i].speed = animatorOriginalSpeeds[i];
            }
        }
        cachedAnimators.Clear();
        animatorOriginalSpeeds.Clear();

        if (cachedBehaviour != null)
        {
            cachedBehaviour.enabled = originalBehaviourEnabled;
        }
        cachedBehaviour = null;

        foreach (MaterialState ms in materialStates)
        {
            if (ms.material != null)
            {
                if (!string.IsNullOrEmpty(ms.colourPropertyName) && ms.material.HasProperty(ms.colourPropertyName))
                {
                    ms.material.SetColor(ms.colourPropertyName, ms.originalColour);
                }
                else
                {
                    try
                    {
                        ms.material.color = ms.originalColour;
                    }
                    catch
                    {
                        // unpresent
                    }
                }
            }
        }
        materialStates.Clear();
    }
}