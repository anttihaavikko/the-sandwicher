using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnttiStarterKit.Managers
{
    public class TextPopup : MonoBehaviour
    {
        [SerializeField] private List<TMP_Text> texts;
        [SerializeField] private float duration = 5f;
        
        private Animator anim;
        private int defaultState;

        private void Awake()
        {
            anim = GetComponent<Animator>();
        }

        public void Play(string content)
        {
            texts.ForEach(t => t.text = content);
            Invoke(nameof(Done), duration);

            if (!anim) return;
            anim.Play(defaultState, -1, 0);
        }

        private void Done()
        {
            EffectManager.Instance.ReturnToPool(this);
        }
    }
}