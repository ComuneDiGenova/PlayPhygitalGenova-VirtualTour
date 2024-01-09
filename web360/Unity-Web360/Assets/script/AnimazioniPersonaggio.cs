using System.Collections.Generic;
using UnityEngine;

public class AnimazioniPersonaggio : MonoBehaviour
{
    private Animator animator;
    //private string modelName;
    private List<string> stateNames = new List<string>();
    public AudioSource audioSource;

    private void Start()
    {
        //modelName = gameObject.name;
        animator = GetComponent<Animator>();
        //audioSource = GetComponent<AudioSource>();
    }

    //private List<string> GetAnimatorStateNames()
    //{
    //    AnimatorController ac = animator.runtimeAnimatorController as AnimatorController;

    //    if (ac != null)
    //    {
    //        foreach (AnimatorControllerLayer layer in ac.layers)
    //        {
    //            ChildAnimatorState[] states = layer.stateMachine.states;

    //            foreach (ChildAnimatorState state in states)
    //            {
    //                stateNames.Add(state.state.name);
    //            }
    //        }
    //    }

    //    return stateNames;
    //}

    private void Update()
    {
        // Controlla se l'AudioSource sta suonando
        bool isAudioPlaying = audioSource.isPlaying;

        // Imposta la condizione booleana nell'Animator in base allo stato dell'AudioSource
        animator.SetBool("parla", isAudioPlaying);
    }

    private void PlayIdleAnimation()
    {
        animator.Play("Idle");

    }

    //private void PlayRandomAnimation()
    //{
    //    int randomIndex = Random.Range(0, stateNames.Count);
    //    string randomAnimationName = stateNames[randomIndex];
    //    animator.Play(randomAnimationName);
    //    Debug.Log("Animazione random: " + randomAnimationName);

    //    isCooldown = true;
    //}
}
